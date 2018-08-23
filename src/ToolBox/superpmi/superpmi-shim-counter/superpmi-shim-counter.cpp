//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

//----------------------------------------------------------
// SuperPMI-Shim.cpp - thin shim for the jit
//----------------------------------------------------------

#include "standardpch.h"
#include "superpmi-shim-counter.h"
#include "runtimedetails.h"
#include "coreclrcallbacks.h"
#include "icorjitcompiler.h"
#include "errorhandling.h"
#include "logging.h"
#include "spmiutil.h"
#include "jithost.h"

HMODULE               g_hRealJit      = 0; // We leak this currently (could do the proper shutdown in process_detach)
WCHAR*                g_realJitPath   = nullptr; // We leak this (could do the proper shutdown in process_detach)
WCHAR*                g_logPath       = nullptr; // Again, we leak this one too...
char*                 g_logFilePath   = nullptr; // We *don't* leak this, hooray!
WCHAR*                g_HomeDirectory = nullptr;
WCHAR*                g_DefaultRealJitPath = nullptr;
MethodCallSummarizer* g_globalContext      = nullptr;

void SetDefaultPaths()
{
    if (g_HomeDirectory == nullptr)
    {
        g_HomeDirectory = GetEnvironmentVariableWithDefaultW(W("HOME"), W("."));
    }

    if (g_DefaultRealJitPath == nullptr)
    {
        size_t len           = wcslen(g_HomeDirectory) + 1 + wcslen(DEFAULT_REAL_JIT_NAME_W) + 1;
        g_DefaultRealJitPath = new WCHAR[len];
        wcscpy_s(g_DefaultRealJitPath, len, g_HomeDirectory);
        wcscat_s(g_DefaultRealJitPath, len, DIRECTORY_SEPARATOR_STR_W);
        wcscat_s(g_DefaultRealJitPath, len, DEFAULT_REAL_JIT_NAME_W);
    }
}

void SetLibName()
{
    if (g_realJitPath == nullptr)
    {
        g_realJitPath = GetEnvironmentVariableWithDefaultW(W("SuperPMIShimPath"), g_DefaultRealJitPath);
    }
}

void SetLogPath()
{
    if (g_logPath == nullptr)
    {
        g_logPath = GetEnvironmentVariableWithDefaultW(W("SuperPMIShimLogPath"), g_HomeDirectory);
    }
}

// TODO: this only works for ANSI file paths...
void SetLogFilePath()
{
    if (g_logFilePath == nullptr)
    {
        // If the environment variable isn't set, we don't enable file logging
        g_logFilePath = GetEnvironmentVariableWithDefaultA("SuperPMIShimLogFilePath", nullptr);
    }
}

extern "C" BOOL
#ifndef FEATURE_PAL
    APIENTRY
#endif // !FEATURE_PAL
    DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
        case DLL_PROCESS_ATTACH:
#ifdef FEATURE_PAL
            if (0 != PAL_InitializeDLL())
            {
                fprintf(stderr, "Error: Fail to PAL_InitializeDLL\n");
                exit(1);
            }
#endif // FEATURE_PAL

            Logger::Initialize();
            SetLogFilePath();
            Logger::OpenLogFile(g_logFilePath);
            break;

        case DLL_PROCESS_DETACH:
            Logger::Shutdown();

            delete[] g_logFilePath;
            g_logFilePath = nullptr;

            if (g_globalContext != nullptr)
            {
                g_globalContext->SaveTextFile();
            }

            break;

        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
            break;
    }
    return TRUE;
}

// Exported via def file
extern "C" void __stdcall jitStartup(ICorJitHost* host)
{
    SetDefaultPaths();
    SetLibName();

    if (!LoadRealJitLib(g_hRealJit, g_realJitPath))
    {
        return;
    }

    // Get the required entrypoint
    PjitStartup pnjitStartup = (PjitStartup)::GetProcAddress(g_hRealJit, "jitStartup");
    if (pnjitStartup == nullptr)
    {
        // This portion of the interface is not used by the JIT under test.
        g_ourJitHost = nullptr;
        return;
    }

    g_ourJitHost = new JitHost(host);

    if (g_globalContext == nullptr)
    {
        SetLogPath();
        g_globalContext = new MethodCallSummarizer(g_logPath);
    }

    g_ourJitHost->setMethodCallSummarizer(g_globalContext);

    pnjitStartup(g_ourJitHost);
}

// Exported via def file
extern "C" ICorJitCompiler* __stdcall getJit()
{
    DWORD             dwRetVal = 0;
    PgetJit           pngetJit;
    interceptor_ICJC* pJitInstance = nullptr;
    ICorJitCompiler*  tICJI        = nullptr;

    SetDefaultPaths();
    SetLibName();

    if (!LoadRealJitLib(g_hRealJit, g_realJitPath))
    {
        return nullptr;
    }

    // get the required entrypoints
    pngetJit = (PgetJit)::GetProcAddress(g_hRealJit, "getJit");
    if (pngetJit == 0)
    {
        LogError("getJit() - GetProcAddress 'getJit' failed (0x%08x)", ::GetLastError());
        return nullptr;
    }

    tICJI = pngetJit();
    if (tICJI == nullptr)
    {
        LogError("getJit() - pngetJit gave us null");
        return nullptr;
    }

    pJitInstance                           = new interceptor_ICJC();
    pJitInstance->original_ICorJitCompiler = tICJI;

    if (g_globalContext == nullptr)
    {
        SetLogPath();
        g_globalContext = new MethodCallSummarizer(g_logPath);
    }
    pJitInstance->mcs = g_globalContext;
    return pJitInstance;
}

// Exported via def file
extern "C" void __stdcall sxsJitStartup(CoreClrCallbacks const& original_cccallbacks)
{
    PsxsJitStartup pnsxsJitStartup;

    SetDefaultPaths();
    SetLibName();

    if (!LoadRealJitLib(g_hRealJit, g_realJitPath))
    {
        return;
    }

    // get entry point
    pnsxsJitStartup = (PsxsJitStartup)::GetProcAddress(g_hRealJit, "sxsJitStartup");
    if (pnsxsJitStartup == 0)
    {
        LogError("sxsJitStartup() - GetProcAddress 'sxsJitStartup' failed (0x%08x)", ::GetLastError());
        return;
    }

    // Setup CoreClrCallbacks and call sxsJitStartup
    original_CoreClrCallbacks                             = new CoreClrCallbacks();
    original_CoreClrCallbacks->m_hmodCoreCLR              = original_cccallbacks.m_hmodCoreCLR;
    original_CoreClrCallbacks->m_pfnIEE                   = original_cccallbacks.m_pfnIEE;
    original_CoreClrCallbacks->m_pfnGetCORSystemDirectory = original_cccallbacks.m_pfnGetCORSystemDirectory;
    original_CoreClrCallbacks->m_pfnGetCLRFunction        = original_cccallbacks.m_pfnGetCLRFunction;

    CoreClrCallbacks* temp = new CoreClrCallbacks();

    temp->m_hmodCoreCLR              = original_cccallbacks.m_hmodCoreCLR;
    temp->m_pfnIEE                   = IEE_t;
    temp->m_pfnGetCORSystemDirectory = original_cccallbacks.m_pfnGetCORSystemDirectory;
    temp->m_pfnGetCLRFunction        = GetCLRFunction;

    pnsxsJitStartup(*temp);
}

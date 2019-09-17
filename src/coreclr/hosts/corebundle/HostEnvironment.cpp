// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
// A simple CoreCLR host that runs a managed binary with the same name as this executable but with *.dll extension
// The dll binary must contain main entry point.
//

#include <windows.h>
#include <stdio.h>
#include "HostEnvironment.h"
#include "error.h"

// Attempts to load CoreCLR.dll from the given directory.
// On success pins the dll, sets m_coreCLRDirectoryPath and returns the HMODULE.
// On failure returns nullptr.

static HMODULE TryLoadCoreCLR(const char* directoryPath)
{
    char coreCLRPath[MAX_LONGPATH];
    strcpy(coreCLRPath, directoryPath);
    strcat(coreCLRPath, "CoreCLR.dll");

    HMODULE result = ::LoadLibraryExA(coreCLRPath, NULL, 0);

    if (!result) {
        return nullptr;
    }

    // Pin the module - CoreCLR.dll does not support being unloaded.
    HMODULE dummy_coreCLRModule;
    ::GetModuleHandleExA(GET_MODULE_HANDLE_EX_FLAG_PIN, coreCLRPath, &dummy_coreCLRModule);

    return result;
}


bool HostEnvironment::Setup()
{
    // Discover the path to this exe's module. All other files are expected to be in the same directory.
    DWORD hostPathLength = ::GetModuleFileNameA(::GetModuleHandleA(nullptr), m_hostPath, MAX_LONGPATH);

    if (hostPathLength == MAX_LONGPATH)
    {
        error("Host path is too long.");
        return false;
    }

    // Search for the last backslash in the host path.
    int lastBackslashIndex;
    for (lastBackslashIndex = hostPathLength - 1; lastBackslashIndex >= 0; lastBackslashIndex--) {
        if (m_hostPath[lastBackslashIndex] == '\\') {
            break;
        }
    }

    // Copy the directory path
    strncpy(m_hostDir, m_hostPath, lastBackslashIndex + 1);
    m_hostDir[lastBackslashIndex + 1] = 0;

    // Build the DLL name
    strncpy(m_appPath, m_hostPath, hostPathLength + 1);

    auto extension = strchr(m_appPath, '.');
    if (extension == NULL || (strcmp(extension, ".exe") != 0)) {
        error("This executable needs to have 'exe' extension.");
        return false;
    }
    // Change the extension from ".exe" to ".dll"
    extension[1] = 'd';
    extension[2] = 'l';
    extension[3] = 'l';

    // Load CoreCLR
    m_coreCLRModule = TryLoadCoreCLR(m_hostDir);

    if (m_coreCLRModule == nullptr) {
        error("Couldn't load coreclr.dll.");
        return false;
    }

    m_initializeCoreCLR =
        (coreclr_initialize_ptr)::GetProcAddress(m_coreCLRModule, "coreclr_initialize");
    m_executeAssembly =
        (coreclr_execute_assembly_ptr)::GetProcAddress(m_coreCLRModule, "coreclr_execute_assembly");
    m_shutdownCoreCLR =
        (coreclr_shutdown_2_ptr)::GetProcAddress(m_coreCLRModule, "coreclr_shutdown_2");

    m_Tpa.Compute(m_hostDir);

    return true;
}


//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

//----------------------------------------------------------
// SPMIUtil.cpp - General utility functions
//----------------------------------------------------------

#include "standardpch.h"
#include "logging.h"
#include "spmiutil.h"

static bool breakOnDebugBreakorAV = false;

bool BreakOnDebugBreakorAV()
{
    return breakOnDebugBreakorAV;
}

void SetBreakOnDebugBreakOrAV(bool value)
{
    breakOnDebugBreakorAV = value;
}

void DebugBreakorAV(int val)
{
    if (IsDebuggerPresent())
    {
        if (val == 0)
            __debugbreak();
        if (BreakOnDebugBreakorAV())
            __debugbreak();
    }

    int exception_code = EXCEPTIONCODE_DebugBreakorAV + val;
    // assert((EXCEPTIONCODE_DebugBreakorAV <= exception_code) && (exception_code < EXCEPTIONCODE_DebugBreakorAV_MAX))
    LogException(exception_code, "DebugBreak or AV Exception %d", val);
}

char* GetEnvironmentVariableWithDefaultA(const char* envVarName, const char* defaultValue)
{
    char* retString = nullptr;

    // Figure out how much space we need to allocate
    DWORD dwRetVal = ::GetEnvironmentVariableA(envVarName, nullptr, 0);
    if (dwRetVal != 0)
    {
        retString = new char[dwRetVal];
        dwRetVal  = ::GetEnvironmentVariableA(envVarName, retString, dwRetVal);
    }
    else
    {
        if (defaultValue != nullptr)
        {
            dwRetVal  = (DWORD)strlen(defaultValue) + 1; // add one for null terminator
            retString = new char[dwRetVal];
            memcpy_s(retString, dwRetVal, defaultValue, dwRetVal);
        }
    }

    return retString;
}

WCHAR* GetEnvironmentVariableWithDefaultW(const WCHAR* envVarName, const WCHAR* defaultValue)
{
    WCHAR* retString = nullptr;

    // Figure out how much space we need to allocate
    DWORD dwRetVal = ::GetEnvironmentVariableW(envVarName, nullptr, 0);
    if (dwRetVal != 0)
    {
        retString = new WCHAR[dwRetVal];
        dwRetVal  = ::GetEnvironmentVariableW(envVarName, retString, dwRetVal);
    }
    else
    {
        if (defaultValue != nullptr)
        {
            dwRetVal  = (DWORD)wcslen(defaultValue) + 1; // add one for null terminator
            retString = new WCHAR[dwRetVal];
            memcpy_s(retString, dwRetVal * sizeof(WCHAR), defaultValue, dwRetVal * sizeof(WCHAR));
        }
    }

    return retString;
}

#ifdef FEATURE_PAL
// For some reason, the PAL doesn't have GetCommandLineA(). So write it.
LPSTR GetCommandLineA()
{
    LPSTR  pCmdLine  = nullptr;
    LPWSTR pwCmdLine = GetCommandLineW();

    if (pwCmdLine != nullptr)
    {
        // Convert to ASCII

        int n = WideCharToMultiByte(CP_ACP, 0, pwCmdLine, -1, nullptr, 0, nullptr, nullptr);
        if (n == 0)
        {
            LogError("MultiByteToWideChar failed %d", GetLastError());
            return nullptr;
        }

        pCmdLine = new char[n];

        int n2 = WideCharToMultiByte(CP_ACP, 0, pwCmdLine, -1, pCmdLine, n, nullptr, nullptr);
        if ((n2 == 0) || (n2 != n))
        {
            LogError("MultiByteToWideChar failed %d", GetLastError());
            return nullptr;
        }
    }

    return pCmdLine;
}
#endif // FEATURE_PAL

bool LoadRealJitLib(HMODULE& jitLib, WCHAR* jitLibPath)
{
    // Load Library
    if (jitLib == NULL)
    {
        if (jitLibPath == nullptr)
        {
            LogError("LoadRealJitLib - No real jit path");
            return false;
        }
        jitLib = ::LoadLibraryW(jitLibPath);
        if (jitLib == NULL)
        {
            LogError("LoadRealJitLib - LoadLibrary failed to load '%ws' (0x%08x)", jitLibPath, ::GetLastError());
            return false;
        }
    }
    return true;
}

void cleanupExecutableName(WCHAR* executableName)
{
    WCHAR* quote = nullptr;

    // If there are any quotes in the file name convert them to spaces.
    while ((quote = wcsstr(executableName, W("\""))) != nullptr)
    {
        *quote = W(' ');
    }

    // Remove any illegal or annoying characters from the file name by converting them to underscores.
    while ((quote = wcspbrk(executableName, W("=<>:\"/\\|?! *.,"))) != nullptr)
    {
        *quote = W('_');
    }
}

void generateRandomSuffix(WCHAR* buffer, size_t length)
{
    unsigned randNumber = 0;
#ifdef FEATURE_PAL
    PAL_Random(&randNumber, sizeof(randNumber));
#else  // !FEATURE_PAL
    rand_s(&randNumber);
#endif // !FEATURE_PAL

    swprintf_s(buffer, length, W("%08X"), randNumber);
}

// All lengths in this function exclude the terminal NULL.
WCHAR* getResultFileName(const WCHAR* folderPath, WCHAR* executableName, const WCHAR* extension)
{
    cleanupExecutableName(executableName);

    size_t executableNameLength = wcslen(executableName);
    size_t extensionLength      = wcslen(extension);
    size_t folderPathLength     = wcslen(folderPath);

    size_t dataFileNameLength = folderPathLength + 1 + executableNameLength + 1 + extensionLength;

    const size_t maxAcceptablePathLength =
        MAX_PATH - 50; // subtract 50 because excel doesn't like paths longer then 230.

#ifdef FEATURE_PAL
        assert(executableNameLength == 0);
#endif // FEATURE_PAL

    if (dataFileNameLength > maxAcceptablePathLength || executableNameLength == 0)
    {
        // The path name is too long; creating the file will fail. This can happen because we use the command line,
        // which for ngen includes lots of environment variables, for example.
        // Shorten the executable file name and add a random string to it to avoid collisions.

        const size_t randStringLength = 8;

#ifndef FEATURE_CORECLR

        size_t lengthToBeDeleted = (dataFileNameLength - maxAcceptablePathLength) + randStringLength;

        if (executableNameLength <= lengthToBeDeleted)
        {
            LogError("getResultFileName - path to the output file is too long '%ws\\%ws.%ws(%d)'", folderPath,
                     executableName, extension, dataFileNameLength);
            return nullptr;
        }

        executableNameLength -= lengthToBeDeleted;
        executableName[executableNameLength] = 0;

#endif // FEATURE_CORECLR

        executableNameLength += randStringLength;
        WCHAR randNumberString[randStringLength + 1];
        generateRandomSuffix(randNumberString, randStringLength + 1);
        wcsncat_s(executableName, executableNameLength + 1, randNumberString, randStringLength);

        executableNameLength = wcslen(executableName);

        dataFileNameLength = folderPathLength + 1 + executableNameLength + 1 + extensionLength;
        if (dataFileNameLength > maxAcceptablePathLength)
        {
            LogError("getResultFileName - were not able to short the result file name.", folderPath, executableName,
                     extension, dataFileNameLength);
            return nullptr;
        }
    }

    WCHAR* dataFileName = new WCHAR[dataFileNameLength + 1];
    dataFileName[0]     = 0;
    wcsncat_s(dataFileName, dataFileNameLength + 1, folderPath, folderPathLength);
    wcsncat_s(dataFileName, dataFileNameLength + 1, DIRECTORY_SEPARATOR_STR_W, 1);
    wcsncat_s(dataFileName, dataFileNameLength + 1, executableName, executableNameLength);
    wcsncat_s(dataFileName, dataFileNameLength + 1, extension, extensionLength);
    return dataFileName;
}

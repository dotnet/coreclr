// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef HOST_ENVIRONMENT_H
#define HOST_ENVIRONMENT_H

#include "mscoree.h"
#include "coreclrhost.h"
#include "TPA.h"
#include "palclr.h"

// Encapsulates the environment that CoreCLR will run in, including the TPALIST
class HostEnvironment 
{
    // The path to this module
    char m_hostPath[MAX_LONGPATH];

    // The path to the directory containing this module
    char m_hostDir[MAX_LONGPATH];

    // The main app to run
    char m_appPath[MAX_LONGPATH];

    // The loaded coreclr Module
    HMODULE m_coreCLRModule;

    TPA m_Tpa;

    // A few procs from CoreCLR module
    coreclr_initialize_ptr m_initializeCoreCLR;
    coreclr_execute_assembly_ptr m_executeAssembly;
    coreclr_shutdown_2_ptr m_shutdownCoreCLR;

    bool TPAListContainsFile(_In_z_ char* fileNameWithoutExtension, _In_reads_(countExtensions) const char** rgTPAExtensions, int countExtensions);
    void AddFilesFromDirectoryToTPAList(_In_z_ const char* targetPath, _In_reads_(countExtensions) const char** rgTPAExtensions, int countExtensions);

public:

    HostEnvironment()
        :m_coreCLRModule(nullptr),
        m_initializeCoreCLR(nullptr),
        m_executeAssembly(nullptr),
        m_shutdownCoreCLR(nullptr)
    {}

    bool Setup();

    const char* AppPath() const { return m_appPath; }
    const char* HostPath() const { return m_hostPath; }
    const char* HostDir() const { return m_hostDir; }

    coreclr_initialize_ptr CoreCLRInitialize() const { return m_initializeCoreCLR; }
    coreclr_execute_assembly_ptr ExecuteAssembly() const { return m_executeAssembly; }
    coreclr_shutdown_2_ptr ShutdownCoreCLR() const { return m_shutdownCoreCLR; }

    // Returns the semicolon-separated list of paths to runtime dlls that are considered trusted.
    // On first call, scans the coreclr directory for dlls and adds them all to the list.
    const char* TpaList() { return m_Tpa.GetTpa(); }

};

#endif // HOST_ENVIRONMENT_H

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
// A simple CoreCLR host that runs a managed binary with the same name as this executable but with *.dll extension
// The dll binary must contain main entry point.
//

#include <stdio.h>
#include "HostEnvironment.h"
#include "error.h"
#include <app_bundle.h>
#include <bundle.h>

int __cdecl wmain(const int argc, const char* argv[])
{
    int exitCode = -1;

    //-------------------------------------------------------------
    // Load CoreCLR and initialize the host environment
    HostEnvironment hostEnvironment;
    if (!hostEnvironment.Setup())
    {
        error("Host Environment Initialization failed.");
        return false;
    }

    //-------------------------------------------------------------
    // Process the bundle contents

    if (!bundle::app_bundle_t::init(hostEnvironment.HostPath()))
    {
        error("Application is not a valid single-file bundle.");
        return false;
    }

    //-------------------------------------------------------------
    // Gather properties from the environment

    const char* systemGcServer = "1";
    char comPlusGcServer[5];
    if (GetEnvironmentVariableA("COMPlus_gcServer", comPlusGcServer, 5) != 0)
    {
        systemGcServer = comPlusGcServer;
    }

    //-------------------------------------------------------------
    // Setup Properties to pass to the Runtime

    // Passed Properties:
    // BUNDLE_PROBE
    // -  The bundle-probe callback is passed in masquarading as char*.
    //
    // TRUSTED_PLATFORM_ASSEMBLIES
    // - The list of complete paths to each of the fully trusted assemblies
    //
    // NATIVE_DLL_SEARCH_DIRECTORIES
    // - The list of paths that will be probed for native DLLs called by PInvoke
    //
    // "System.GC.Server",
    // - Whether to use Server GC. 

    // Check the environment to find whether to use Server GC.
    // As a temoporary hack in the single-exe prototype, this value is true by default
    // to suite the ASP.net scenario.

    const char* propertyKeys[] = {
        "BUNDLE_PROBE",
        "TRUSTED_PLATFORM_ASSEMBLIES",
        "NATIVE_DLL_SEARCH_DIRECTORIES",
        "System.GC.Server"
    };
    const char* propertyValues[] = {
        // BUNDLE_PROBE
        (char*)bundle::app_bundle_t::probe,
        // TRUSTED_PLATFORM_ASSEMBLIES
        hostEnvironment.TpaList(),
        // NATIVE_DLL_SEARCH_DIRECTORIES
        hostEnvironment.HostPath(),
        // System.GC.Server
        systemGcServer
    };

    void* hostHandle;
    UINT domainId;

    int st = hostEnvironment.CoreCLRInitialize()(
        hostEnvironment.HostPath(),
        "corebundle",
        sizeof(propertyKeys) / sizeof(propertyKeys[0]),
        propertyKeys,
        propertyValues,
        &hostHandle,
        &domainId);

    if (!SUCCEEDED(st))
    {
        error("coreclr_initialize failed", st);
        exitCode = -2;
        return false;
    }

    st = hostEnvironment.ExecuteAssembly()(
        hostHandle,
        domainId,
        argc,
        argv,
        hostEnvironment.AppPath(),
        (unsigned int*)& exitCode);

    if (!SUCCEEDED(st))
    {
        error("coreclr_execute_assembly failed", st);
        exitCode = -3;
    }

    exitCode = 0;
    st = hostEnvironment.ShutdownCoreCLR()(hostHandle, domainId, &exitCode);
    if (!SUCCEEDED(st))
    {
        error("coreclr_shutdown failed", st);
        exitCode = -4;
    }

}

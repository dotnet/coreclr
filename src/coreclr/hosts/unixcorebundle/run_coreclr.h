// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <string>

// Get the path to entrypoint executable
bool GetEntrypointExecutableAbsolutePath(std::string& entrypointExecutable);

// Add all *.dll, *.ni.dll, *.exe, and *.ni.exe files from the specified directory to the tpaList string.
void AddFilesFromDirectoryToTpaList(const char* directory, std::string& tpaList);

//
// Execute the specified managed assembly. 
//
// Parameters:
//  currentExePath          - Path to the current executable
//  clrFilesAbsolutePath    - Absolute path to the folder where the libcoreclr.so and CLR managed assemblies are stored
//  managedAssemblyPath     - Path to the managed assembly to execute
//  managedAssemblyArgc     - Number of arguments passed to the executed assembly
//  managedAssemblyArgv     - Array of arguments passed to the executed assembly
//
// Returns:
//  ExitCode of the assembly
//
int ExecuteManagedAssembly(
            const char* currentExeAbsolutePath,
            const char* clrFilesAbsolutePath,
            const char* managedAssemblyAbsolutePath,
            int managedAssemblyArgc,
            const char** managedAssemblyArgv);

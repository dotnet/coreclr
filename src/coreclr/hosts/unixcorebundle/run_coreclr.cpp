// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
// Helper for running coreclr
//

#include "config.h"

#include <cstdlib>
#include <cstring>
#include <assert.h>
#include <dirent.h>
#include <limits.h>
#include <set>
#include <string>
#include <string.h>
#include <sys/stat.h>
#if defined(__FreeBSD__)
#include <sys/types.h>
#include <sys/param.h>
#endif
#if HAVE_GETAUXVAL
#include <sys/auxv.h>
#endif
#if defined(HAVE_SYS_SYSCTL_H) || defined(__FreeBSD__)
#include <sys/sysctl.h>
#endif
#include "run_coreclr.h"
#include <coreclrhost.h>
#include <unistd.h>
#ifndef SUCCEEDED
#define SUCCEEDED(Status) ((Status) >= 0)
#endif // !SUCCEEDED

// Name of the environment variable controlling server GC.
// If set to 1, server GC is enabled on startup. If 0, server GC is
// disabled. Server GC is off by default.
static const char* serverGcVar = "COMPlus_gcServer";

// Name of environment variable to control "System.Globalization.Invariant"
// Set to 1 for Globalization Invariant mode to be true. Default is false.
static const char* globalizationInvariantVar = "CORECLR_GLOBAL_INVARIANT";

#if defined(__linux__)
#define symlinkEntrypointExecutable "/proc/self/exe"
#elif !defined(__APPLE__)
#define symlinkEntrypointExecutable "/proc/curproc/exe"
#endif

namespace
{
    bool GetAbsolutePath(const char* path, std::string& absolutePath)
    {
        bool result = false;

        char realPath[PATH_MAX];
        if (realpath(path, realPath) != nullptr && realPath[0] != '\0')
        {
            absolutePath.assign(realPath);
            // realpath should return canonicalized path without the trailing slash
            assert(absolutePath.back() != '/');

            result = true;
        }

        return result;
    }
}

bool GetEntrypointExecutableAbsolutePath(std::string& entrypointExecutable)
{
    bool result = false;
    
    entrypointExecutable.clear();

    // Get path to the executable for the current process using
    // platform specific means.
#if defined(__APPLE__)
    
    // On Mac, we ask the OS for the absolute path to the entrypoint executable
    uint32_t lenActualPath = 0;
    if (_NSGetExecutablePath(nullptr, &lenActualPath) == -1)
    {
        // OSX has placed the actual path length in lenActualPath,
        // so re-attempt the operation
        std::string resizedPath(lenActualPath, '\0');
        char *pResizedPath = const_cast<char *>(resizedPath.c_str());
        if (_NSGetExecutablePath(pResizedPath, &lenActualPath) == 0)
        {
            entrypointExecutable.assign(pResizedPath);
            result = true;
        }
    }
#elif defined (__FreeBSD__)
    static const int name[] = {
        CTL_KERN, KERN_PROC, KERN_PROC_PATHNAME, -1
    };
    char path[PATH_MAX];
    size_t len;

    len = sizeof(path);
    if (sysctl(name, 4, path, &len, nullptr, 0) == 0)
    {
        entrypointExecutable.assign(path);
        result = true;
    }
    else
    {
        // ENOMEM
        result = false;
    }
#elif defined(__NetBSD__) && defined(KERN_PROC_PATHNAME)
    static const int name[] = {
        CTL_KERN, KERN_PROC_ARGS, -1, KERN_PROC_PATHNAME,
    };
    char path[MAXPATHLEN];
    size_t len;

    len = sizeof(path);
    if (sysctl(name, __arraycount(name), path, &len, NULL, 0) != -1)
    {
        entrypointExecutable.assign(path);
        result = true;
    }
    else
    {
        result = false;
    }
#else

#if HAVE_GETAUXVAL && defined(AT_EXECFN)
    const char *execfn = (const char *)getauxval(AT_EXECFN);

    if (execfn)
    {
        entrypointExecutable.assign(execfn);
        result = true;
    }
    else
#endif
    // On other OSs, return the symlink that will be resolved by GetAbsolutePath
    // to fetch the entrypoint EXE absolute path, inclusive of filename.
    result = GetAbsolutePath(symlinkEntrypointExecutable, entrypointExecutable);
#endif 

    return result;
}

void AddFilesFromDirectoryToTpaList(const char* directory, std::string& tpaList)
{
    const char * const tpaExtensions[] = {
                ".ni.dll",      // Probe for .ni.dll first so that it's preferred if ni and il coexist in the same dir
                ".dll",
                ".ni.exe",
                ".exe",
                };

    DIR* dir = opendir(directory);
    if (dir == nullptr)
    {
        return;
    }

    std::set<std::string> addedAssemblies;

    // Walk the directory for each extension separately so that we first get files with .ni.dll extension,
    // then files with .dll extension, etc.
    for (size_t extIndex = 0; extIndex < sizeof(tpaExtensions) / sizeof(tpaExtensions[0]); extIndex++)
    {
        const char* ext = tpaExtensions[extIndex];
        int extLength = strlen(ext);

        struct dirent* entry;

        // For all entries in the directory
        while ((entry = readdir(dir)) != nullptr)
        {
            // We are interested in files only
            switch (entry->d_type)
            {
            case DT_REG:
                break;

            // Handle symlinks and file systems that do not support d_type
            case DT_LNK:
            case DT_UNKNOWN:
                {
                    std::string fullFilename;

                    fullFilename.append(directory);
                    fullFilename.append("/");
                    fullFilename.append(entry->d_name);

                    struct stat sb;
                    if (stat(fullFilename.c_str(), &sb) == -1)
                    {
                        continue;
                    }

                    if (!S_ISREG(sb.st_mode))
                    {
                        continue;
                    }
                }
                break;

            default:
                continue;
            }

            std::string filename(entry->d_name);

            // Check if the extension matches the one we are looking for
            int extPos = filename.length() - extLength;
            if ((extPos <= 0) || (filename.compare(extPos, extLength, ext) != 0))
            {
                continue;
            }

            std::string filenameWithoutExt(filename.substr(0, extPos));

            // Make sure if we have an assembly with multiple extensions present,
            // we insert only one version of it.
            if (addedAssemblies.find(filenameWithoutExt) == addedAssemblies.end())
            {
                addedAssemblies.insert(filenameWithoutExt);

                tpaList.append(directory);
                tpaList.append("/");
                tpaList.append(filename);
                tpaList.append(":");
            }
        }
        
        // Rewind the directory stream to be able to iterate over it for the next extension
        rewinddir(dir);
    }
    
    closedir(dir);
}

const char* GetEnvValueBoolean(const char* envVariable)
{
    const char* envValue = std::getenv(envVariable);
    if (envValue == nullptr)
    {
        envValue = "0";
    }
    // CoreCLR expects strings "true" and "false" instead of "1" and "0".
    return (std::strcmp(envValue, "1") == 0 || strcasecmp(envValue, "true") == 0) ? "true" : "false";
}

int ExecuteManagedAssembly(
    const char* currentExeAbsolutePath,
    const char* clrFilesAbsolutePath,
    const char* managedAssemblyAbsolutePath,
    int managedAssemblyArgc,
    const char** managedAssemblyArgv)
{
    // Indicates failure
    int exitCode = -1;

#ifdef _ARM_
    // libunwind library is used to unwind stack frame, but libunwind for ARM
    // does not support ARM vfpv3/NEON registers in DWARF format correctly.
    // Therefore let's disable stack unwinding using DWARF information
    // See https://github.com/dotnet/coreclr/issues/6698
    //
    // libunwind use following methods to unwind stack frame.
    // UNW_ARM_METHOD_ALL          0xFF
    // UNW_ARM_METHOD_DWARF        0x01
    // UNW_ARM_METHOD_FRAME        0x02
    // UNW_ARM_METHOD_EXIDX        0x04
    putenv(const_cast<char *>("UNW_ARM_UNWIND_METHOD=6"));
#endif // _ARM_

    // App path is the same as CLR path (self-contained bundle)
    std::string appPath(clrFilesAbsolutePath);

    std::string tpaList(managedAssemblyAbsolutePath);
    tpaList.append(":");
    AddFilesFromDirectoryToTpaList(clrFilesAbsolutePath, tpaList);

    // Construct native search directory paths
    std::string nativeDllSearchDirs(clrFilesAbsolutePath);

    // Check whether we are enabling server GC (off by default)
    const char* useServerGc = GetEnvValueBoolean(serverGcVar);

    // Check Globalization Invariant mode (false by default)
    const char* globalizationInvariant = GetEnvValueBoolean(globalizationInvariantVar);

    // Allowed property names:
    // APPBASE
    // - The base path of the application from which the exe and other assemblies will be loaded
    //
    // TRUSTED_PLATFORM_ASSEMBLIES
    // - The list of complete paths to each of the fully trusted assemblies
    //
    // APP_PATHS
    // - The list of paths which will be probed by the assembly loader
    //
    // APP_NI_PATHS
    // - The list of additional paths that the assembly loader will probe for ngen images
    //
    // NATIVE_DLL_SEARCH_DIRECTORIES
    // - The list of paths that will be probed for native DLLs called by PInvoke
    //
    const char *propertyKeys[] = {
        "TRUSTED_PLATFORM_ASSEMBLIES",
        "APP_PATHS",
        "APP_NI_PATHS",
        "NATIVE_DLL_SEARCH_DIRECTORIES",
        "System.GC.Server",
        "System.Globalization.Invariant",
        "OVERRIDE_SYSTEM_PATH"
    };
    const char *propertyValues[] = {
        // TRUSTED_PLATFORM_ASSEMBLIES
        tpaList.c_str(),
        // APP_PATHS
        appPath.c_str(),
        // APP_NI_PATHS
        appPath.c_str(),
        // NATIVE_DLL_SEARCH_DIRECTORIES
        nativeDllSearchDirs.c_str(),
        // System.GC.Server
        useServerGc,
        // System.Globalization.Invariant
        globalizationInvariant,
        // OVERRIDE_SYSTEM_PATH
        clrFilesAbsolutePath
    };

    void* hostHandle;
    unsigned int domainId;

    int st = coreclr_initialize(
                currentExeAbsolutePath, 
                "unixcorebundle", 
                sizeof(propertyKeys) / sizeof(propertyKeys[0]), 
                propertyKeys, 
                propertyValues, 
                &hostHandle, 
                &domainId);

    if (!SUCCEEDED(st))
    {
        fprintf(stderr, "coreclr_initialize failed - status: 0x%08x\n", st);
        exitCode = -1;
    }
    else 
    {
        st = coreclr_execute_assembly(
                hostHandle,
                domainId,
                managedAssemblyArgc,
                managedAssemblyArgv,
                managedAssemblyAbsolutePath,
                (unsigned int*)&exitCode);

        if (!SUCCEEDED(st))
        {
            fprintf(stderr, "coreclr_execute_assembly failed - status: 0x%08x\n", st);
            exitCode = -1;
        }

        int latchedExitCode = 0;
        st = coreclr_shutdown_2(hostHandle, domainId, &latchedExitCode);
        if (!SUCCEEDED(st))
        {
            fprintf(stderr, "coreclr_shutdown failed - status: 0x%08x\n", st);
            exitCode = -1;
        }

        if (exitCode != -1)
        {
            exitCode = latchedExitCode;
        }
    }

    return exitCode;
}

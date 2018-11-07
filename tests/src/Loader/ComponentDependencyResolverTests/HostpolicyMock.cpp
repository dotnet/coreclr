// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Mock implementation of the hostpolicy.cpp exported methods.
// Used for testing CoreCLR/Corlib functionality which calls into hostpolicy.

#include <string>

// dllexport
#if defined _WIN32

#define DLL_EXPORT __declspec(dllexport)
typedef wchar_t char_t;
typedef std::wstring string_t;

#else //!_Win32

#if __GNUC__ >= 4
#define DLL_EXPORT __attribute__ ((visibility ("default")))
#else
#define DLL_EXPORT
#endif

typedef char char_t;
typedef std::string string_t;

#endif //_WIN32

int g_corehost_resolve_component_dependencies_returnValue = -1;
string_t g_corehost_resolve_component_dependencies_assemblyPaths;
string_t g_corehost_resolve_component_dependencies_nativeSearchPaths;
string_t g_corehost_resolve_component_dependencies_resourceSearchPaths;

typedef void(*corehost_resolve_component_dependencies_result_fn)(
    const char_t* assembly_paths,
    const char_t* native_search_paths,
    const char_t* resource_search_paths);

extern "C" DLL_EXPORT int corehost_resolve_component_dependencies(
    const char_t *component_main_assembly_path,
    corehost_resolve_component_dependencies_result_fn result)
{
    if (g_corehost_resolve_component_dependencies_returnValue == 0)
    {
        result(
            g_corehost_resolve_component_dependencies_assemblyPaths.data(),
            g_corehost_resolve_component_dependencies_nativeSearchPaths.data(),
            g_corehost_resolve_component_dependencies_resourceSearchPaths.data());
    }

    return g_corehost_resolve_component_dependencies_returnValue;
}

extern "C" DLL_EXPORT void Set_corehost_resolve_component_dependencies_Values(
    int returnValue,
    const char_t *assemblyPaths,
    const char_t *nativeSearchPaths,
    const char_t *resourceSearchPaths)
{
    g_corehost_resolve_component_dependencies_returnValue = returnValue;
    g_corehost_resolve_component_dependencies_assemblyPaths.assign(assemblyPaths);
    g_corehost_resolve_component_dependencies_nativeSearchPaths.assign(nativeSearchPaths);
    g_corehost_resolve_component_dependencies_resourceSearchPaths.assign(resourceSearchPaths);
}
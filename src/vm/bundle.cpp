// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//*****************************************************************************
// Bundle.cpp
//
// Helpers to access meta-data stored in single-file bundles
//
//*****************************************************************************

#include "common.h"
#include "bundle.h"
#include <utilcode.h>
#include <corhost.h>

Bundle *Bundle::AppBundle = nullptr;

static LPCSTR UnicodeToUtf8(LPCWSTR str)
{
    STANDARD_VM_CONTRACT;

    int length = WideCharToMultiByte(CP_UTF8, 0, str, -1, NULL, 0, 0, 0);
    _ASSERTE(length != 0);

    LPSTR result = new (nothrow) CHAR[length];
    _ASSERTE(result != NULL);

    length = WideCharToMultiByte(CP_UTF8, 0, str, -1, result, length, 0, 0);
    _ASSERTE(length != 0);

    return result;
}

const SString &BundleFileLocation::Path() const
{
    LIMITED_METHOD_CONTRACT;

    // Currently, there is only one bundle -- the bundle for the main App.
    // Therefore, obtain the path from the global AppBundle.
    // If there is more than one bundle in one application (ex: single file plugins)
    // the BundlePath may be stored in the BundleFileLocation structure.

    _ASSERTE(IsValid());
    _ASSERTE(Bundle::AppBundle != nullptr);

    return Bundle::AppBundle->Path();
}

Bundle::Bundle(LPCWSTR bundlePath, BundleProbe *probe)
{
    STANDARD_VM_CONTRACT;

    _ASSERTE(probe != nullptr);

    m_path.Set(bundlePath);
    m_probe = probe;

    // In this prototype, the bundle-base path is simply the directory containing 
    // the single-file bundle. When the Probe() function searches within the bundle,
    // it masks out the basePath from the assembly-path (if found).
    //
    //  For example: 
    //  Bundle.Probe("lib.dll") => m_probe("lib.dll")
    //  Bundle.Probe("path/to/exe/lib.dll") => m_probe("lib.dll")
    //  Bundle.Probe("path/to/exe/and/some/more/lib.dll") => m_probe("and/some/more/lib.dll")
    //
    // This strategy obviously hides any actual files in path/to/exe from being loaded.
    // 
    // In the final implementation, we should set base_path to a known prefix, say "#\"
    // and teach the host and other parts of the runtime to expect this kind of path.
    // This is related to the question of what Assembly.Location is for bundled assemblies.

    LPCWSTR pos = wcsrchr(bundlePath, DIRECTORY_SEPARATOR_CHAR_W);
    _ASSERTE(pos != nullptr);

    size_t baseLen = pos - bundlePath + 1; // Include DIRECTORY_SEPARATOR_CHAR_W in m_basePath
    m_basePath.Set(bundlePath, (COUNT_T)baseLen);
}

BundleFileLocation Bundle::Probe(LPCWSTR path, bool pathIsBundleRelative) const
{
    STANDARD_VM_CONTRACT;

    BundleFileLocation loc;

    // Skip over m_base_path, if any.
    // TODO: Normalize paths
    // TODO: Normalize path casing on Windows.

    if (!pathIsBundleRelative)
    {
        size_t baseLen = m_basePath.GetCount();
        if (wcsncmp(m_basePath, path, baseLen) == 0)
        {
            path += baseLen; // m_basePath includes count for DIRECTORY_SEPARATOR_CHAR_W
        }
        else
        {
            // This is not a file within the bundle
            return loc;
        }
    }

    m_probe(UnicodeToUtf8(path), &loc.Size, &loc.Offset);

    return loc;
}

BundleFileLocation Bundle::ProbeAppBundle(LPCWSTR path, bool pathIsBundleRelative)
{
    STANDARD_VM_CONTRACT;

    return AppIsBundle() ? AppBundle->Probe(path, pathIsBundleRelative) : BundleFileLocation::Invalid();
}


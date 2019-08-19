// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//*****************************************************************************
// CorHost.cpp
//
// Implementation for the meta data dispenser code.
//

//*****************************************************************************

#include "common.h"
#include "corbundle.h"
#include <utilcode.h>
#include <corhost.h>

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

BundleInfo::BundleInfo(LPCWSTR bundlePath, bool(*probe)(LPCSTR, INT64*, INT64*))
{
    STANDARD_VM_CONTRACT;

    m_path = bundlePath;
    m_probe = probe;

    // In this prototype, the bundle-base path is simply the directory containing 
    // the single-file bundle. When the Probe() function searches within the bundle,
    // it masks out the base_path from the assembly-path (if found).
    //
    //  For example: 
    //  BundleInfo.Probe("lib.dll") => m_probe("lib.dll")
    //  BundleInfo.Probe("path/to/exe/lib.dll") => m_probe("lib.dll")
    //  BundleInfo.Probe("path/to/exe/and/some/more/lib.dll") => m_probe("and/some/more/lib.dll")
    //
    // This strategy obviously hides any actual files in path/to/exe from being loaded.
    // 
    // In the final implementation, we should set base_path to a known prefix, say "#\"
    // and teach the host and other parts of the runtime to expect this kind of path.
    // This is related to the question of what Assembly.Location is for bundled assemblies.

    LPCWSTR pos = wcsrchr(bundlePath, DIRECTORY_SEPARATOR_CHAR_W);
    _ASSERTE(pos != nullptr);

    m_base_len = pos - bundlePath;
    wcsncpy(m_base_path, bundlePath, m_base_len);
}

LPCWSTR BundleInfo::Path() const
{ 
    LIMITED_METHOD_CONTRACT; 
	
    return m_path; 
}

bool BundleInfo::Probe(LPCWSTR path, INT64* size, INT64* offset) const
{
    STANDARD_VM_CONTRACT;

    // Skip over m_base_path, if any.
    if (wcsncmp(m_base_path, path, m_base_len) == 0)
    {
        path += m_base_len + 1;
    }

    return m_probe(UnicodeToUtf8(path), size, offset);
}

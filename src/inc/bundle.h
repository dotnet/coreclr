// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*****************************************************************************
 **                                                                         **
 ** bundle.h - Information about applications bundled as a single-file  **
 **                                                                         **
 *****************************************************************************/

#ifndef _BUNDLE_H_
#define _BUNDLE_H_

#include <sstring.h>

class Bundle;

struct BundleLoc
{
    INT64 Size;
    INT64 Offset;

    BundleLoc() 
    { 
        LIMITED_METHOD_CONTRACT;

        Size = 0; 
        Offset = 0; 
    }

    static BundleLoc Invalid() { LIMITED_METHOD_CONTRACT; return BundleLoc(); }

    LPCWSTR Path() const;

    bool IsValid() const { LIMITED_METHOD_CONTRACT; return Offset != 0; }
};

typedef bool(__stdcall BundleProbe)(LPCSTR, INT64*, INT64*);

class Bundle
{
public:
    Bundle(LPCWSTR bundlePath, BundleProbe *probe);
    BundleLoc Probe(LPCWSTR path, bool pathIsBundleRelative = false) const;

    LPCWSTR Path() const { LIMITED_METHOD_CONTRACT; return m_path; }
    LPCWSTR BasePath() const { LIMITED_METHOD_CONTRACT; return m_basePath; }

    static Bundle* AppBundle; // The BundleInfo for the current app, initialized by coreclr_initialize.
    static bool AppIsBundle() { LIMITED_METHOD_CONTRACT; return AppBundle != nullptr; }
    static BundleLoc ProbeAppBundle(LPCWSTR path, bool pathIsBundleRelative = false);

private:

    LPCWSTR m_path; // The path to single-file executable
    BundleProbe *m_probe;

    SString m_basePath; // The prefix to denote a path within the bundle
};

#endif // _BUNDLE_H_
// EOF =======================================================================

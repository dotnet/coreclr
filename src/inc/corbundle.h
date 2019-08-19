// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*****************************************************************************
 **                                                                         **
 ** corbundle.h - Header to process single-file bundles                     **
 **                                                                         **
 *****************************************************************************/

#ifndef _COR_BUNDLE_H_
#define _COR_BUNDLE_H_

class BundleInfo
{
public:
    BundleInfo(LPCWSTR bundlePath, bool(*probe)(LPCSTR, INT64*, INT64*));
    bool Probe(LPCWSTR path, INT64* size, INT64* offset) const;
    LPCWSTR Path() const;

private:

    LPCWSTR m_path; // The path to single-file executable
    bool(*m_probe)(LPCSTR, INT64*, INT64*);

    WCHAR m_base_path[MAX_PATH]; // The prefix to denote a path within the bundle
    size_t m_base_len; // The length of the above prefix
};

#endif // _COR_BUNDLE_H_
// EOF =======================================================================

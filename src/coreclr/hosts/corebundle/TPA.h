// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef TPA_H
#define TPA_H

#include "StringBuffer.h"

// TPA handler for CoreBundle host.
// The TPA handler can be removed if we build a host linked in with the coreclr libraries
// in the scenario where all files are expected to be found within the bundle.

class TPA
{
public:
    TPA() {}
    void Compute(const char* baseDir);
    const char* GetTpa() { return m_tpa.CStr();  }

private:
    const char* m_baseDir;
    StringBuffer m_tpa;

    const char* const rgTPAExtensions[6] = { "*.ni.dll", "*.dll", "*.ni.exe", "*.exe", "*.ni.winmd", "*.winmd" };
    const int countExtensions = 6;

    bool HasFile(_In_z_ char* fileNameWithoutExtension);
};

#endif // TPA_H

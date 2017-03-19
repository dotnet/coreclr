// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _CLRCONFIGURATION_H_
#define _CLRCONFIGURATION_H_

class ClrConfiguration
{
public:
    static BOOL QCALLTYPE GetConfigBoolValue(LPCWSTR name);
};

#endif // _CLRCONFIGURATION_H_

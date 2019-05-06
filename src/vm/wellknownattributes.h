// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __WELLKNOWNATTRIBUTES_H_
#define __WELLKNOWNATTRIBUTES_H_

enum class WellKnownAttribute : DWORD
{
    CountOfWellKnownAttributes
};

const char *GetWellKnownAttributeName(WellKnownAttribute attribute)
{
    switch (attribute)
    {
    }
    _ASSERTE(false); // Should not be possible
    return nullptr;
}

#endif // __WELLKNOWNATTRIBUTES_H_

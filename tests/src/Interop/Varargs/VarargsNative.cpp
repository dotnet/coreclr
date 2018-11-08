// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>
#include <stdarg.h>

extern "C" DLL_EXPORT void TestVarArgs(LPWSTR formattedString, SIZE_T bufferSize, LPCWSTR format, ...)
{
    va_list args;
    va_start(args, format);

    vswprintf_s(formattedString, bufferSize, format, args);
}

extern "C" DLL_EXPORT void TestArgIterator(LPWSTR formattedString, SIZE_T bufferSize, LPCWSTR format, va_list args)
{
    vswprintf_s(formattedString, bufferSize, format, args);
}

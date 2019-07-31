// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "../Native/StringMarshalingNative.h"

using StringType = LPWSTR;
using Tests = StringMarshalingTests<StringType, TP_slen>;

#define FUNCTION_NAME __FUNCTIONW__

#include "../Native/StringTestEntrypoints.inl"

// Verify that we append extra null terminators to our StringBuilder native buffers.
// Although this is a hidden implementation detail, it would be breaking behavior to stop doing this
// so we have a test for it. In particular, this detail prevents us from optimizing marshalling StringBuilders by pinning.
extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE Verify_NullTerminators_PastEnd(LPCWSTR buffer, int length)
{
    return buffer[length+1] == W('\0');
}

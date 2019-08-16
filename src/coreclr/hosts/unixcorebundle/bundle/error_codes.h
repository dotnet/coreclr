// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __ERROR_CODES_H__
#define __ERROR_CODES_H__

// These error and exit codes are document in the host-error-codes.md
enum StatusCode
{
    // Success
    Success                             = 0,

    BundleExtractionFailure             = 0x8000809f,
    BundleExtractionIOError             = 0x800080a0,
};

#endif // __ERROR_CODES_H__

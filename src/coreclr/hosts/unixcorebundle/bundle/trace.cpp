// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "trace.h"

void trace::info(const pal::char_t* format, ...)
{
    // va_list args;
    // va_start(args, format);
    // ::vfprintf(stdout, format, args);
    // ::fputc('\n', stdout);
    // va_end(args);
}

void trace::error(const pal::char_t* format, ...)
{
    // va_list args;
    // va_start(args, format);
    // ::vfprintf(stderr, format, args);
    // ::fputc('\n', stderr);
    // va_end(args);
}

void trace::warning(const pal::char_t* format, ...)
{
    // va_list args;
    // va_start(args, format);
    // ::vfprintf(stdout, format, args);
    // ::fputc('\n', stdout);
    // va_end(args);
}

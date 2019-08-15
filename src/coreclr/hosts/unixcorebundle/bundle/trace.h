// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef TRACE_H
#define TRACE_H

#include "pal.h"

namespace trace
{
    void info(const pal::char_t* format, ...);
    void warning(const pal::char_t* format, ...);
    void error(const pal::char_t* format, ...);
}
#endif // TRACE_H

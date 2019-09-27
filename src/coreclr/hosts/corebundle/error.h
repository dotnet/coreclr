// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef ERROR_H
#define ERROR_H

inline void error(const char* msg)
{
    fprintf(stderr, "%s\n", msg);
}

inline void error(const char* msg, int status)
{
    fprintf(stderr, "%s - status: 0x%08x\n", msg, status);
}

#endif // ERROR_H

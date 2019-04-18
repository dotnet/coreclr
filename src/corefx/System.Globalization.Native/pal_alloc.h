// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include <stdlib.h>
#include <stdint.h>

static void *pal_allocarray(size_t numMembers, size_t count)
{
    if (numMembers == 0 || count == 0)
        return NULL;
    if (SIZE_MAX / numMembers < count)
        return NULL;
    return malloc(numMembers * count);
}

static void *pal_reallocarray(void *ptr, size_t numMembers, size_t count)
{
    if (numMembers == 0 || count == 0)
        return NULL;
    if (SIZE_MAX / numMembers < count)
        return NULL;
    return realloc(ptr, numMembers * count);
}

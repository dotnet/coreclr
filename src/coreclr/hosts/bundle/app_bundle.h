// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This class represents the bundle for the currently executing application
// It is the bundle-processing module's interface with the outside world.

#ifndef __APP_BUNDLE_H__
#define __APP_BUNDLE_H__

#include <stdint.h>

namespace bundle
{
    class runner_t;

    class app_bundle_t
    {
    public:
        static bool init(const char *path);
        static bool probe(const char* path, int64_t* size, int64_t* offset);

    private:
        static runner_t *s_runner;
    };
}

#endif // __APP_BUNDLE_H__

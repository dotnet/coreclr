// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __RUNNER_H__
#define __RUNNER_H__

#include "error_codes.h"
#include "pal.h"
#include "manifest.h"

namespace bundle
{
    class runner_t
    {
    public:
        runner_t(const pal::string_t& bundle_path="")
            : m_bundle_path(bundle_path)
            , m_bundle_map(nullptr)
            , m_bundle_length(0)
        {
        }

        StatusCode process();

        bool probe (const char *relative_path, int64_t *size, int64_t *offset);

        const pal::char_t* get_bundle_path() { return m_bundle_path.c_str(); }

    private:
        void map_host();
        void unmap_host();

        manifest_t m_manifest;
        pal::string_t m_bundle_path;
        int8_t* m_bundle_map;
        size_t m_bundle_length;
    };
}

#endif // __RUNNER_H__

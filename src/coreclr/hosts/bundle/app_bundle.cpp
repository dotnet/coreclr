// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "app_bundle.h"
#include "marker.h"
#include "runner.h"

using namespace bundle;

runner_t* app_bundle_t::s_runner = nullptr;

bool app_bundle_t::init(const char* exe_path)
{
    if (!bundle::marker_t::is_bundle())
    {
        return false;
    }

    pal::string_t self_path;
    pal::clr_palstring(exe_path, self_path);

    static runner_t runner(exe_path);
    s_runner = &runner;

    StatusCode status = runner.process();

    return status == StatusCode::Success;
}

bool app_bundle_t::probe(const char* path, int64_t* size, int64_t* offset)
{
    return s_runner->probe(path, size, offset);
}

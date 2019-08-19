// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
// A simple CoreCLR host that runs as a single-exe bundle
//

#include <coreruncommon.h>
#include <string>
#include <string.h>
#include <sys/stat.h>

#include <marker.h>
#include <runner.h>
#include <utils.h>

static bundle::runner_t bundle_runner;

bool probe_bundle(const pal::char_t* path, int64_t *size, int64_t *offset)
{
    return bundle_runner.probe(path, size, offset);
}

int main(const int argc, const char* argv[])
{
    // Make sure we have a full path for argv[0].
    std::string exe_path;
    if (!GetEntrypointExecutableAbsolutePath(exe_path))
    {
        perror("Could not get full path to current executable");
        return -1;
    }

    if (!bundle::marker_t::is_bundle())
    {
        perror("Executuable is not a bundle");
        return -1;
    }

    bundle_runner = bundle::runner_t(exe_path);
    StatusCode bundle_status = bundle_runner.process();	        

    if (bundle_status != StatusCode::Success)
    {
        perror("Could not extract contents of the bundle");
        return bundle_status;
    }

    std::string root_dir = get_directory(exe_path);
    std::string app_path(root_dir);
    app_path.append(get_filename(exe_path.c_str()));
    app_path.append(".dll");

    const char** app_argv = nullptr;
    int app_argc = argc - 1;
    if (app_argc != 0)
    {
        app_argv = &argv[1];
    }

    int exitCode = ExecuteManagedAssembly(
                        exe_path.c_str(),
                        root_dir.c_str(),
                        app_path.c_str(),
                        probe_bundle,
                        app_argc,
                        app_argv);

    return exitCode;
}

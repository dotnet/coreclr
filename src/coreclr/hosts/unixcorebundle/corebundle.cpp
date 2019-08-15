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

#include <bundle_runner.h>
#include <utils.h>

#define EMBED_HASH_HI_PART_UTF8 "c3ab8ff13720e8ad9047dd39466b3c89" // SHA-256 of "foobar" in UTF-8
#define EMBED_HASH_LO_PART_UTF8 "74e592c2fa383d4a3960714caef0c4f2"
#define EMBED_HASH_FULL_UTF8    (EMBED_HASH_HI_PART_UTF8 EMBED_HASH_LO_PART_UTF8) // NUL terminated

constexpr int EMBED_SZ = sizeof(EMBED_HASH_FULL_UTF8) / sizeof(EMBED_HASH_FULL_UTF8[0]);
constexpr int EMBED_MAX = (EMBED_SZ > 1025 ? EMBED_SZ : 1025); // 1024 DLL name length, 1 NUL
static char embed[EMBED_MAX] = EMBED_HASH_FULL_UTF8;     // series of NULs followed by embed hash string

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

    fprintf(stdout, "Running bundle: %s\n", exe_path.c_str());

    bundle::bundle_runner_t extractor(exe_path);
    StatusCode bundle_status = extractor.extract();
    if (bundle_status != StatusCode::Success)
    {
        perror("Could not extract contents of the bundle");
        return bundle_status;
    }

    std::string root_dir = extractor.get_extraction_dir();
    std::string app_path(root_dir);
    app_path.push_back(DIR_SEPARATOR);

    std::string binding(&embed[0]);
    app_path.append(binding.c_str());

    const char** app_argv = nullptr;
    int app_argc = argc - 1;
    if (app_argc != 0)
    {
        app_argv = &argv[1];
    }

    std::fflush(stdout);
    int exitCode = ExecuteManagedAssembly(
                        exe_path.c_str(),
                        root_dir.c_str(),
                        app_path.c_str(),
                        app_argc,
                        app_argv);

    return exitCode;
}

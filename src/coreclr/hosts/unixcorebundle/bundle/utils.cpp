// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "utils.h"
#include "trace.h"

namespace
{
    bool ends_with(const pal::string_t& value, const pal::string_t& suffix, bool match_case)
    {
        auto cmp = match_case ? pal::strcmp : pal::strcasecmp;
        return (value.size() >= suffix.size()) &&
            cmp(value.c_str() + value.size() - suffix.size(), suffix.c_str()) == 0;
    }
}

void append_path(pal::string_t* path1, const pal::char_t* path2)
{
    if (pal::is_path_rooted(path2))
    {
        path1->assign(path2);
    }
    else
    {
        if (!path1->empty() && path1->back() != DIR_SEPARATOR)
        {
            path1->push_back(DIR_SEPARATOR);
        }
        path1->append(path2);
    }
}

pal::string_t strip_executable_ext(const pal::string_t& filename)
{

#if defined(_WIN32)
    pal::string_t exe_suffix = _X(".exe");
#else
    pal::string_t exe_suffix = _X("");

#endif

    if (exe_suffix.empty())
    {
        return filename;
    }

    if (ends_with(filename, exe_suffix, false))
    {
        // We need to strip off the old extension
        pal::string_t result(filename);
        result.erase(result.size() - exe_suffix.size());
        return result;
    }

    return filename;
}

pal::string_t get_filename(const pal::string_t& path)
{
    if (path.empty())
    {
        return path;
    }

    auto name_pos = path.find_last_of(DIR_SEPARATOR);
    if (name_pos == pal::string_t::npos)
    {
        return path;
    }

    return path.substr(name_pos + 1);
}

pal::string_t get_directory(const pal::string_t& path)
{
    pal::string_t ret = path;
    while (!ret.empty() && ret.back() == DIR_SEPARATOR)
    {
        ret.pop_back();
    }

    // Find the last dir separator
    auto path_sep = ret.find_last_of(DIR_SEPARATOR);
    if (path_sep == pal::string_t::npos)
    {
        return ret + DIR_SEPARATOR;
    }

    int pos = static_cast<int>(path_sep);
    while (pos >= 0 && ret[pos] == DIR_SEPARATOR)
    {
        pos--;
    }
    return ret.substr(0, static_cast<size_t>(pos) + 1) + DIR_SEPARATOR;
}
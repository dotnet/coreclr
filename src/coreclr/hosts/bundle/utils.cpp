// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "utils.h"

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
    std::string ret = path;
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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "file_entry.h"
#include "trace.h"
#include "error_codes.h"

using namespace bundle;

bool file_entry_t::is_valid() const
{
    return m_offset > 0 && m_size > 0 &&
        static_cast<file_type_t>(m_type) < file_type_t::__last;
}

// Fixup a path to have current platform's directory separator.
void fixup_path_separator(pal::string_t& path)
{
    const pal::char_t bundle_dir_separator = '/';

    if (bundle_dir_separator != DIR_SEPARATOR)
    {
        for (size_t pos = path.find(bundle_dir_separator);
            pos != pal::string_t::npos;
            pos = path.find(bundle_dir_separator, pos))
        {
            path[pos] = DIR_SEPARATOR;
        }
    }
}

file_entry_t file_entry_t::read(reader_t &reader)
{
    // First read the fixed-sized portion of file-entry
    const file_entry_fixed_t* fixed_data = reinterpret_cast<const file_entry_fixed_t*>(reader.read_direct(sizeof(file_entry_fixed_t)));
    file_entry_t entry(fixed_data);

    if (!entry.is_valid())
    {
        trace::error(_X("Failure processing application bundle; possible file corruption."));
        trace::error(_X("Invalid FileEntry detected."));
        throw StatusCode::BundleExtractionFailure;
    }

    reader.read_path_string(entry.m_relative_path);
    fixup_path_separator(entry.m_relative_path);

    return entry;
}

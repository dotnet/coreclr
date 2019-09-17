// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <memory>
#include "runner.h"
#include "trace.h"
#include "header.h"
#include "marker.h"
#include "manifest.h"

using namespace bundle;

void runner_t::map_host()
{
    m_bundle_map = (int8_t *) pal::map_file_readonly(m_bundle_path, m_bundle_length);

    if (m_bundle_map == nullptr)
    {
        trace::error(_X("Failure processing application bundle."));
        trace::error(_X("Couldn't memory map the bundle file for reading."));
        throw StatusCode::BundleExtractionIOError;
    }
}

void runner_t::unmap_host()
{
    if (!pal::unmap_file(m_bundle_map, m_bundle_length))
    {
        trace::warning(_X("Failed to unmap bundle after extraction."));
    }
}

bool runner_t::probe(const char *relative_path, int64_t *size, int64_t *offset)
{
    for (file_entry_t& entry : m_manifest.files)
    {
        if (strcmp(entry.relative_path().c_str(), relative_path) == 0)
        {
            *size = entry.size();
            *offset = entry.offset();
            return true;
        }
    }
    return false;
}

StatusCode runner_t::process()
{
    try
    {
        map_host();
        reader_t reader(m_bundle_map, m_bundle_length);

        reader.set_offset(marker_t::header_offset());
        header_t header = header_t::read(reader);

        m_manifest = manifest_t::read(reader, header.num_embedded_files());

        unmap_host();
        return StatusCode::Success;
    }
    catch (StatusCode e)
    {
        return e;
    }
}


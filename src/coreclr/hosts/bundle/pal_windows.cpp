// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#include "pal.h"

void* pal::map_file_readonly(const pal::string_t& path, size_t& length)
{
    HANDLE file = CreateFileA(path.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

    if (file == INVALID_HANDLE_VALUE)
    {
        return nullptr;
    }

    LARGE_INTEGER fileSize;
    if (GetFileSizeEx(file, &fileSize) == 0)
    {
        CloseHandle(file);
        return nullptr;
    }
    length = (size_t)fileSize.QuadPart;

    HANDLE map = CreateFileMappingW(file, NULL, PAGE_READONLY, 0, 0, NULL);

    if (map == NULL)
    {
        CloseHandle(file);
        return nullptr;
    }

    void* address = MapViewOfFile(map, FILE_MAP_READ, 0, 0, 0);

    if (map == NULL)
    {
        CloseHandle(file);
        return nullptr;
    }

    return address;
}

bool pal::unmap_file(void* addr, size_t length) 
{ 
    return UnmapViewOfFile(addr) != 0; 
}


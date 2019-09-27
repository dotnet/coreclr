// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "pal.h"

#include <sys/stat.h>
#include <fcntl.h>
#include <dirent.h>

void* pal::map_file_readonly(const pal::string_t& path, size_t& length)
{
    int fd = open(path.c_str(), O_RDONLY, (S_IRUSR | S_IRGRP | S_IROTH));
    if (fd == -1)
    {
        return nullptr;
    }

    struct stat buf;
    if (fstat(fd, &buf) != 0)
    {
        close(fd);
        return nullptr;
    }

     length = buf.st_size;
    void* address = mmap(nullptr, length, PROT_READ, MAP_SHARED, fd, 0);

     if(address == nullptr)
    {
        close(fd);
        return nullptr;
    }

     close(fd);
    return address;
}

bool pal::unmap_file(void* addr, size_t length) 
{ 
    return munmap(addr, length) == 0; 
}

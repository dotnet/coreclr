// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef PAL_H
#define PAL_H

#include <string>
#include <vector>
#include <fstream>
#include <sstream>
#include <iostream>
#include <cstring>
#include <cstdarg>
#include <cstdint>
#include <cassert>

#if defined(_WIN32)
#include <windows.h>
#else // defined(_WIN32)
#include <cstdlib>
#include <unistd.h>
#include <libgen.h>
#include <mutex>
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/mman.h>
#endif // defined(_WIN32)

#if defined(_WIN32)
#define DIR_SEPARATOR '\\'
#define PATH_MAX MAX_PATH

#else // defined(_WIN32)
#define DIR_SEPARATOR '/'

#if !defined(PATH_MAX) 
#define PATH_MAX    4096
#endif
#endif // defined(_WIN32)

namespace pal
{
    typedef std::basic_ifstream<char> ifstream_t;
    typedef std::istreambuf_iterator<ifstream_t::char_type> istreambuf_iterator_t;
    typedef std::basic_istream<char> istream_t;

    typedef char char_t;
    typedef std::string string_t;
    typedef std::stringstream stringstream_t;

    inline bool clr_palstring(const char* cstr, string_t& out) { out.assign(cstr); return true; }

#if defined(_WIN32)
    typedef HMODULE dll_t;
    typedef FARPROC proc_t;
    inline int pathcmp(const char_t* path1, const char_t* path2) { return ::_stricmp(path1, path2); }
#else // defined(_WIN32)
    typedef void* dll_t;
    typedef void* proc_t;
    inline int pathcmp(const char_t* path1, const char_t* path2) { return ::strcmp(path1, path2); }
#endif // defined(_WIN32)

    void* map_file_readonly(const string_t& path, size_t& length);
    bool unmap_file(void* addr, size_t length);
}

#endif // PAL_H

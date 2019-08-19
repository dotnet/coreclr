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

#include <cstdlib>
#include <unistd.h>
#include <libgen.h>
#include <mutex>
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/mman.h>

#define xerr std::cerr
#define xout std::cout
#define DIR_SEPARATOR '/'
#define PATH_SEPARATOR ':'
#define _X(s) s

#if !defined(PATH_MAX) 
#define PATH_MAX    4096
#endif

namespace pal
{
    typedef std::basic_ifstream<char> ifstream_t;
    typedef std::istreambuf_iterator<ifstream_t::char_type> istreambuf_iterator_t;
    typedef std::basic_istream<char> istream_t;

    typedef char char_t;
    typedef std::string string_t;
    typedef std::stringstream stringstream_t;

    typedef void* dll_t;
    typedef void* proc_t;

    inline int strcmp(const char_t* str1, const char_t* str2) { return ::strcmp(str1, str2); }
    inline int strcasecmp(const char_t* str1, const char_t* str2) { return ::strcasecmp(str1, str2); }

    inline FILE * file_open(const string_t& path, const char_t* mode) { return fopen(path.c_str(), mode); }
    inline int str_vprintf(char_t* str, size_t size, const char_t* format, va_list vl) { return ::vsnprintf(str, size, format, vl); }

    inline bool clr_palstring(const char* cstr, string_t* out) { out->assign(cstr); return true; }

    inline bool mkdir(const char_t* dir, int mode) { return ::mkdir(dir, mode) == 0; }
    inline bool rmdir(const char_t* path) { return ::rmdir(path) == 0; }
    inline int rename(const char_t* old_name, const char_t* new_name) { return ::rename(old_name, new_name); }
    inline int remove(const char_t* path) { return ::remove(path); }
    inline int get_pid() { return getpid(); }
    inline bool unmap_file(void* addr, size_t length) { return munmap(addr, length) == 0; }
    inline void sleep(uint32_t milliseconds) { usleep(milliseconds * 1000); }

    inline int snwprintf(char_t* buffer, size_t count, const char_t* format, ...)
    {
        va_list args;
        va_start(args, format);
        int ret = str_vprintf(buffer, count, format, args);
        va_end(args);
        return ret;
    }

    void* map_file_readonly(const string_t& path, size_t& length);
    bool realpath(string_t* path, bool skip_error_logging = false);
    bool file_exists(const string_t& path);
    inline bool directory_exists(const string_t& path) { return file_exists(path); }
    void readdir(const string_t& path, std::vector<string_t>* list);
    void readdir_onlydirectories(const string_t& path, std::vector<string_t>* list);

    bool getenv(const char_t* name, string_t* recv);

    bool is_path_rooted(const string_t& path);

    bool get_temp_directory(string_t& tmp_dir);

    bool load_library(const string_t* path, dll_t* dll);
    proc_t get_symbol(dll_t library, const char* name);
}

#endif // PAL_H

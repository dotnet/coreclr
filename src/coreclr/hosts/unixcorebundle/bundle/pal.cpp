// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "pal.h"
#include "trace.h"

#include <sys/stat.h>
#include <fcntl.h>
#include <dirent.h>

// Returns true only if an env variable can be read successfully to be non-empty.
bool pal::getenv(const pal::char_t* name, pal::string_t* recv)
{
    recv->clear();

    auto result = ::getenv(name);
    if (result != nullptr)
    {
        recv->assign(result);
    }

    return (recv->length() > 0);
}

bool pal::get_temp_directory(pal::string_t& tmp_dir)
{
    // First, check for the POSIX standard environment variable
    if (pal::getenv(_X("TMPDIR"), &tmp_dir))
    {
        return pal::realpath(&tmp_dir);
    }

    // On non-compliant systems (ex: Ubuntu) try /var/tmp or /tmp directories.
    // /var/tmp is prefered since its contents are expected to survive across
    // machine reboot.
    pal::string_t _var_tmp = _X("/var/tmp/");
    if (pal::realpath(&_var_tmp))
    {
        tmp_dir.assign(_var_tmp);
        return true;
    }

    pal::string_t _tmp = _X("/tmp/");
    if (pal::realpath(&_tmp))
    {
        tmp_dir.assign(_tmp);
        return true;
    }

    return false;
}

bool pal::realpath(pal::string_t* path, bool skip_error_logging)
{
    auto resolved = ::realpath(path->c_str(), nullptr);
    if (resolved == nullptr)
    {
        if (errno == ENOENT)
        {
            return false;
        }

        if (!skip_error_logging)
        {
            trace::error(_X("realpath(%s) failed: %s"), path->c_str(), strerror(errno));
        }

        return false;
    }

    path->assign(resolved);
    ::free(resolved);
    return true;
}


void* pal::map_file_readonly(const pal::string_t& path, size_t& length)
{
    int fd = open(path.c_str(), O_RDONLY, (S_IRUSR | S_IRGRP | S_IROTH));
    if (fd == -1)
    {
        trace::warning(_X("Failed to map file. open(%s) failed with error %d"), path.c_str(), errno);
        return nullptr;
    }

     struct stat buf;
    if (fstat(fd, &buf) != 0)
    {
        trace::warning(_X("Failed to map file. fstat(%s) failed with error %d"), path.c_str(), errno);
        close(fd);
        return nullptr;
    }

     length = buf.st_size;
    void* address = mmap(nullptr, length, PROT_READ, MAP_SHARED, fd, 0);

     if(address == nullptr)
    {
        trace::warning(_X("Failed to map file. mmap(%s) failed with error %d"), path.c_str(), errno);
        close(fd);
        return nullptr;
    }

     close(fd);
    return address;
}

bool pal::file_exists(const pal::string_t& path)
{
    return (::access(path.c_str(), F_OK) == 0);
}

namespace
{
    void readdir_impl(const pal::string_t& path, bool onlydirectories, std::vector<pal::string_t>* list)
    {
        assert(list != nullptr);

        std::vector<pal::string_t>& files = *list;

        auto dir = opendir(path.c_str());
        if (dir != nullptr)
        {
            struct dirent* entry = nullptr;
            while ((entry = readdir(dir)) != nullptr)
            {
                // We are interested in files only
                switch (entry->d_type)
                {
                case DT_DIR:
                    break;

                case DT_REG:
                    if (onlydirectories)
                    {
                        continue;
                    }
                    break;

                // Handle symlinks and file systems that do not support d_type
                case DT_LNK:
                case DT_UNKNOWN:
                    {
                        struct stat sb;

                        if (fstatat(dirfd(dir), entry->d_name, &sb, 0) == -1)
                        {
                            continue;
                        }

                        if (onlydirectories)
                        {
                            if (!S_ISDIR(sb.st_mode))
                            {
                                continue;
                            }
                            break;
                        }
                        else if (!S_ISREG(sb.st_mode) && !S_ISDIR(sb.st_mode))
                        {
                            continue;
                        }
                    }
                    break;

                default:
                    continue;
                }

                if (!strcmp(entry->d_name, ".") || !strcmp(entry->d_name, ".."))
                {
                    continue;
                }

                files.emplace_back(entry->d_name);
            }

            closedir(dir);
        }
    }
}

void pal::readdir(const pal::string_t& path, std::vector<pal::string_t>* list)
{
    readdir_impl(path, false, list);
}

void pal::readdir_onlydirectories(const pal::string_t& path, std::vector<pal::string_t>* list)
{
    readdir_impl(path, true, list);
}

bool pal::is_path_rooted(const pal::string_t& path)
{
    return path.front() == '/';
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ===========================================================================

#if defined(__linux__) && !defined(CROSSGEN_COMPILE)
#define JITDUMP_SUPPORTED
#endif

#include "pal/palinternal.h"
#include "pal/dbgmsg.h"

#include <cstddef>
#include "perfjitdump.h"

#ifdef JITDUMP_SUPPORTED

#include <fcntl.h>
#include <pthread.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <sys/mman.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/syscall.h>
#include <time.h>
#include <unistd.h>
#include <linux/limits.h>

#include "../inc/llvm/ELF.h"

SET_DEFAULT_DEBUG_CHANNEL(MISC);

namespace
{
    enum
    {
        JIT_DUMP_MAGIC = 0x4A695444,
        JIT_DUMP_VERSION = 1,

#if defined(_X86_)
        ELF_MACHINE = EM_386,
#elif defined(_ARM_)
        ELF_MACHINE = EM_ARM,
#elif defined(_AMD64_)
        ELF_MACHINE = EM_X86_64,
#elif defined(_ARM64_)
        ELF_MACHINE = EM_AARCH64,
#else
#error ELF_MACHINE unsupported for target
#endif

        JIT_CODE_LOAD = 0,
    };

    uint64_t GetTimeStampNS()
    {
#if HAVE_CLOCK_MONOTONIC
        struct timespec ts;
        int result = clock_gettime(CLOCK_MONOTONIC, &ts);

        if (result != 0)
        {
            ASSERT("clock_gettime(CLOCK_MONOTONIC) failed: %d\n", result);
            return 0;
        }
        else
        {
            return  ts.tv_sec * 1000000000ULL + ts.tv_nsec;
        }
#else
    #error "The PAL jitdump requires clock_gettime(CLOCK_MONOTONIC) to be supported."
#endif
    }

    struct FileHeader
    {
        FileHeader() :
            magic(JIT_DUMP_MAGIC),
            version(JIT_DUMP_VERSION),
            total_size(sizeof(FileHeader)),
            elf_mach(ELF_MACHINE),
            pad1(0),
            pid(getpid()),
            timestamp(GetTimeStampNS()),
            flags(0)
        {}

        uint32_t magic;
        uint32_t version;
        uint32_t total_size;
        uint32_t elf_mach;
        uint32_t pad1;
        uint32_t pid;
        uint64_t timestamp;
        uint64_t flags;
    };

    struct RecordHeader
    {
        uint32_t id;
        uint32_t total_size;
        uint64_t timestamp;
    };

    struct JitCodeLoadRecord
    {
        JitCodeLoadRecord() :
            pid(getpid()),
            tid(syscall(SYS_gettid))
        {
            header.id = JIT_CODE_LOAD;
            header.timestamp = GetTimeStampNS();
        }

        RecordHeader header;
        uint32_t pid;
        uint32_t tid;
        uint64_t vma;
        uint64_t code_addr;
        uint64_t code_size;
        uint64_t code_index;
        // Null terminated name
        // Optional native code
    };
};

struct PerfJitDumpState
{
    PerfJitDumpState() :
        enabled(false),
        fd(-1),
        mmapAddr(nullptr),
        mutex(PTHREAD_MUTEX_INITIALIZER),
        codeIndex(0)
    {}

    bool enabled;
    int fd;
    void *mmapAddr;
    pthread_mutex_t mutex;
    uint64_t codeIndex;

    int Start(const char* path)
    {
        int result = 0;

        // Write file header
        FileHeader header;

        result = pthread_mutex_lock(&mutex);

        if (enabled)
            goto exit;

        if (result != 0)
            goto exit;

        char jitdumpPath[PATH_MAX];

        result = snprintf(jitdumpPath, sizeof(jitdumpPath), "%s/jit-%i.dump", path, getpid());

        if (result >= PATH_MAX)
            goto exit;

        result = open(jitdumpPath, O_CREAT|O_TRUNC|O_RDWR|O_CLOEXEC, S_IRUSR|S_IWUSR );

        if (result == -1)
            goto exit;

        fd = result;

        result = write(fd, &header, sizeof(FileHeader));

        if (result == -1)
            goto exit;

        result = fsync(fd);

        if (result == -1)
            goto exit;

        // mmap jitdump file
        // this is a marker for perf inject to find the jitdumpfile
        mmapAddr = mmap(nullptr, sizeof(FileHeader), PROT_READ | PROT_EXEC, MAP_PRIVATE, fd, 0);

        if (mmapAddr == MAP_FAILED)
            goto exit;

        enabled = true;

exit:
        result = pthread_mutex_unlock(&mutex);

        return result;
    }

    int LogMethod(void* pCode, size_t codeSize, const char* symbol, void* debugInfo, void* unwindInfo)
    {
        int result = 0;

        if (enabled)
        {
            size_t symbolLen = strlen(symbol);

            JitCodeLoadRecord record;

            record.vma = (uint64_t) pCode;
            record.code_addr = (uint64_t) pCode;
            record.code_size = codeSize;
            record.code_index = ++codeIndex;
            record.header.total_size = sizeof(JitCodeLoadRecord) + symbolLen + 1 + codeSize;

            result = pthread_mutex_lock(&mutex);

            if (result != 0)
                goto exit;

            if (!enabled)
                goto exit;

            // ToDo write debugInfo and unwindInfo immediately before the JitCodeLoadRecord (while lock is held).

            record.header.timestamp = GetTimeStampNS();

            result = write(fd, &record, sizeof(JitCodeLoadRecord));

            if (result == -1)
                goto exit;

            result = write(fd, symbol, symbolLen + 1);

            if (result == -1)
                goto exit;

            result = write(fd, pCode, codeSize);

            if (result == -1)
                goto exit;

            result = fsync(fd);

            if (result == -1)
                goto exit;

exit:
            if (result != 0)
                enabled = false;

            result = pthread_mutex_unlock(&mutex);
        }
        return result;
    }

    int Finish()
    {
        int result = 0;

        if (enabled)
        {
            enabled = false;

            // Lock the mutex
            result = pthread_mutex_lock(&mutex);

            if (result != 0)
                goto exit;

            result = munmap(mmapAddr, sizeof(FileHeader));

            if (result == -1)
                goto exit;

            result = fsync(fd);

            if (result == -1)
                goto exit;

            result = close(fd);

            if (result == -1)
                goto exit;

exit:
            result = pthread_mutex_unlock(&mutex);
        }
        return result;
    }
};


PerfJitDumpState& GetState()
{
    static PerfJitDumpState s;

    return s;
}

int
PALAPI
PAL_PerfJitDump_Start(const char* path)
{
    return GetState().Start(path);
}

int
PALAPI
PAL_PerfJitDump_LogMethod(void* pCode, size_t codeSize, const char* symbol, void* debugInfo, void* unwindInfo)
{
    return GetState().LogMethod(pCode, codeSize, symbol, debugInfo, unwindInfo);
}

int
PALAPI
PAL_PerfJitDump_Finish()
{
    return GetState().Finish();
}

#else // JITDUMP_SUPPORTED

int
PALAPI
PAL_PerfJitDump_Start(const char* path)
{
    return 0;
}

int
PALAPI
PAL_PerfJitDump_LogMethod(void* pCode, size_t codeSize, const char* symbol, void* debugInfo, void* unwindInfo)
{
    return 0;
}

int
PALAPI
PAL_PerfJitDump_Finish()
{
    return 0;
}

#endif // JITDUMP_SUPPORTED

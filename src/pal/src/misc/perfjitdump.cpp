// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ===========================================================================

#if defined(__linux__)
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

    void Start()
    {
        pthread_mutex_lock(&mutex);

        char jitdumpPath[1024];

        snprintf(jitdumpPath, sizeof(jitdumpPath), "/tmp/jit-%i.dump", getpid());

        fd = open(jitdumpPath, O_CREAT|O_TRUNC|O_RDWR|O_CLOEXEC, S_IRUSR|S_IWUSR );

        // Write file header
        FileHeader header;

        write(fd, &header, sizeof(FileHeader));

        fsync(fd);

        // mmap jitdump file
        // this is a marker for perf inject to find the jitdumpfile
        mmapAddr = mmap(nullptr, sizeof(FileHeader), PROT_READ | PROT_EXEC, MAP_PRIVATE, fd, 0);

        enabled = true;

        pthread_mutex_unlock(&mutex);
    }

    void LogMethod(void* pCode, size_t codeSize, const char* symbol)
    {
        if (enabled)
        {
            pthread_mutex_lock(&mutex);

            size_t symbolLen = strlen(symbol);

            JitCodeLoadRecord record;

            record.vma = (uint64_t) pCode;
            record.code_addr = (uint64_t) pCode;
            record.code_size = codeSize;
            record.code_index = ++codeIndex;
            record.header.total_size = sizeof(JitCodeLoadRecord) + symbolLen + 1 + codeSize;
            record.header.timestamp = GetTimeStampNS();

            write(fd, &record, sizeof(JitCodeLoadRecord));

            write(fd, symbol, symbolLen + 1);

            write(fd, pCode, codeSize);

            fsync(fd);

            pthread_mutex_unlock(&mutex);
        }
    }

    void Finish()
    {
        if (enabled)
        {
            // Lock the mutex
            pthread_mutex_lock(&mutex);

            enabled = false;

            munmap(mmapAddr, sizeof(FileHeader));

            fsync(fd);

            close(fd);

            pthread_mutex_unlock(&mutex);
        }
    }
};


PerfJitDumpState& PerfJitDump::GetState()
{
    static PerfJitDumpState s;

    return s;
}

void PerfJitDump::Start()
{
    GetState().Start();
}

void PerfJitDump::LogMethod(void* pCode, size_t codeSize, const char* symbol)
{
    GetState().LogMethod(pCode, codeSize, symbol);
}

void PerfJitDump::Finish()
{
    GetState().Finish();
}

#else // JITDUMP_SUPPORTED

struct PerfJitDumpState
{
};

PerfJitDumpState& PerfJitDump::GetState()
{
    static PerfJitDumpState s;

    return s;
}

void PerfJitDump::Start()
{
}

void PerfJitDump::LogMethod(void* pCode, size_t codeSize, const char* symbol)
{
}

void PerfJitDump::Finish()
{
}

#endif // JITDUMP_SUPPORTED

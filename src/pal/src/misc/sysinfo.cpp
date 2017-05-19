// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*++



Module Name:

    sysinfo.c

Abstract:

    Implements GetSystemInfo.

Revision History:



--*/

#include "pal/palinternal.h"

#include <sched.h>
#include <errno.h>
#include <unistd.h>
#include <sys/types.h>
#if HAVE_SYSCTL
#include <sys/sysctl.h>
#elif !HAVE_SYSCONF
#error Either sysctl or sysconf is required for GetSystemInfo.
#endif

#if HAVE_SYSINFO
#include <sys/sysinfo.h>
#endif

#include <sys/param.h>

#if HAVE_SYS_VMPARAM_H
#include <sys/vmparam.h>
#endif  // HAVE_SYS_VMPARAM_H

#if HAVE_XSWDEV
#include <vm/vm_param.h>
#endif // HAVE_XSWDEV

#if HAVE_MACH_VM_TYPES_H
#include <mach/vm_types.h>
#endif // HAVE_MACH_VM_TYPES_H

#if HAVE_MACH_VM_PARAM_H
#include <mach/vm_param.h>
#endif  // HAVE_MACH_VM_PARAM_H

#if HAVE_MACHINE_VMPARAM_H
#include <machine/vmparam.h>
#endif  // HAVE_MACHINE_VMPARAM_H

#if defined(_TARGET_MAC64)
#include <mach/vm_statistics.h>
#include <mach/mach_types.h>
#include <mach/mach_init.h>
#include <mach/mach_host.h>
#endif // defined(_TARGET_MAC64)

// On some platforms sys/user.h ends up defining _DEBUG; if so
// remove the definition before including the header and put
// back our definition afterwards
#if USER_H_DEFINES_DEBUG
#define OLD_DEBUG _DEBUG
#undef _DEBUG
#endif
#include <sys/user.h>
#if USER_H_DEFINES_DEBUG
#undef _DEBUG
#define _DEBUG OLD_DEBUG
#undef OLD_DEBUG
#endif

#include "pal/dbgmsg.h"


SET_DEFAULT_DEBUG_CHANNEL(MISC);

#ifndef __APPLE__
#if HAVE_SYSCONF && HAVE__SC_AVPHYS_PAGES
#define SYSCONF_PAGES _SC_AVPHYS_PAGES
#elif HAVE_SYSCONF && HAVE__SC_PHYS_PAGES
#define SYSCONF_PAGES _SC_PHYS_PAGES
#else
#error Dont know how to get page-size on this architecture!
#endif
#endif // __APPLE__

#if HAVE_CPUSET_T
typedef cpuset_t cpu_set_t;
#endif

DWORD
PALAPI
PAL_GetLogicalCpuCountFromOS()
{
    int nrcpus = 0;

#if HAVE_SYSCONF
    nrcpus = sysconf(_SC_NPROCESSORS_ONLN);
    if (nrcpus < 1)
    {
        ASSERT("sysconf failed for _SC_NPROCESSORS_ONLN (%d)\n", errno);
    }
#elif HAVE_SYSCTL
    int rc;
    size_t sz;
    int mib[2];

    sz = sizeof(nrcpus);
    mib[0] = CTL_HW;
    mib[1] = HW_NCPU;
    rc = sysctl(mib, 2, &nrcpus, &sz, NULL, 0);
    if (rc != 0)
    {
        ASSERT("sysctl failed for HW_NCPU (%d)\n", errno);
    }
#endif // HAVE_SYSCONF

    return nrcpus;
}

//******************************************************************************
// Returns the number of processors that a process has been configured to run on
//******************************************************************************
int
PALAPI
PAL_GetCurrentProcessCpuCount()
{
#ifdef HAVE_SCHED_GETAFFINITY

    int pid = getpid();
    size_t setsize;
    cpu_set_t* set;
    int ncpus = CPU_SETSIZE;
    int rv;
    do
    {
        setsize = CPU_ALLOC_SIZE(ncpus);
        set = CPU_ALLOC(ncpus);
        if (set == nullptr)
            return 1;

        rv = sched_getaffinity(pid, setsize, set);
        if (rv != 0)
        {
            CPU_FREE(set);

            if (errno != EINVAL)
                return 1;

            ncpus = ncpus + CPU_SETSIZE;
        }
    } while (rv != 0);

    int count = CPU_COUNT_S(setsize, set);

    CPU_FREE(set);

    return count;

#else

    return PAL_GetLogicalCpuCountFromOS();

#endif // HAVE_SCHED_GETAFFINITY
}

/*++
Function:
  GetSystemInfo

GetSystemInfo

The GetSystemInfo function returns information about the current system.

Parameters

lpSystemInfo
       [out] Pointer to a SYSTEM_INFO structure that receives the information.

Return Values

This function does not return a value.

Note:
  fields returned by this function are:
    dwNumberOfProcessors
    dwPageSize
Others are set to zero.

--*/
VOID
PALAPI
GetSystemInfo(
          OUT LPSYSTEM_INFO lpSystemInfo)
{
    int nrcpus = 0;
    long pagesize;

    PERF_ENTRY(GetSystemInfo);
    ENTRY("GetSystemInfo (lpSystemInfo=%p)\n", lpSystemInfo);

    pagesize = getpagesize();

    lpSystemInfo->wProcessorArchitecture_PAL_Undefined = 0;
    lpSystemInfo->wReserved_PAL_Undefined = 0;
    lpSystemInfo->dwPageSize = pagesize;
    lpSystemInfo->dwActiveProcessorMask_PAL_Undefined = 0;

    nrcpus = PAL_GetLogicalCpuCountFromOS();
    TRACE("dwNumberOfProcessors=%d\n", nrcpus);
    lpSystemInfo->dwNumberOfProcessors = nrcpus;

#ifdef VM_MAXUSER_ADDRESS
    lpSystemInfo->lpMaximumApplicationAddress = (PVOID) VM_MAXUSER_ADDRESS;
#elif defined(__linux__)
    lpSystemInfo->lpMaximumApplicationAddress = (PVOID) (1ull << 47);
#elif defined(USERLIMIT)
    lpSystemInfo->lpMaximumApplicationAddress = (PVOID) USERLIMIT;
#elif defined(_WIN64)
#if defined(USRSTACK64)
    lpSystemInfo->lpMaximumApplicationAddress = (PVOID) USRSTACK64;
#else // !USRSTACK64
#error How come USRSTACK64 is not defined for 64bit?
#endif // USRSTACK64
#elif defined(USRSTACK)
    lpSystemInfo->lpMaximumApplicationAddress = (PVOID) USRSTACK;
#else
#error The maximum application address is not known on this platform.
#endif

    lpSystemInfo->lpMinimumApplicationAddress = (PVOID) pagesize;

    lpSystemInfo->dwProcessorType_PAL_Undefined = 0;

    lpSystemInfo->dwAllocationGranularity = pagesize;

    lpSystemInfo->wProcessorLevel_PAL_Undefined = 0;
    lpSystemInfo->wProcessorRevision_PAL_Undefined = 0;

    LOGEXIT("GetSystemInfo returns VOID\n");
    PERF_EXIT(GetSystemInfo);
}

/*++
Function:
  GlobalMemoryStatusEx

GlobalMemoryStatusEx

Retrieves information about the system's current usage of both physical and virtual memory.

Return Values

This function returns a BOOL to indicate its success status.

--*/
BOOL
PALAPI
GlobalMemoryStatusEx(
            IN OUT LPMEMORYSTATUSEX lpBuffer)
{

    PERF_ENTRY(GlobalMemoryStatusEx);
    ENTRY("GlobalMemoryStatusEx (lpBuffer=%p)\n", lpBuffer);

    lpBuffer->dwMemoryLoad = 0;
    lpBuffer->ullTotalPhys = 0;
    lpBuffer->ullAvailPhys = 0;
    lpBuffer->ullTotalPageFile = 0;
    lpBuffer->ullAvailPageFile = 0;
    lpBuffer->ullTotalVirtual = 0;
    lpBuffer->ullAvailVirtual = 0;
    lpBuffer->ullAvailExtendedVirtual = 0;

    BOOL fRetVal = FALSE;
    int mib[3];
    int rc;

    // Get the physical memory size
#if HAVE_SYSCONF && HAVE__SC_PHYS_PAGES
    int64_t physical_memory;

    // Get the Physical memory size
    physical_memory = sysconf( _SC_PHYS_PAGES ) * sysconf( _SC_PAGE_SIZE );
    lpBuffer->ullTotalPhys = (DWORDLONG)physical_memory;
    fRetVal = TRUE;
#elif HAVE_SYSCTL
    int64_t physical_memory;
    size_t length;

    // Get the Physical memory size
    mib[0] = CTL_HW;
    mib[1] = HW_MEMSIZE;
    length = sizeof(INT64);
    rc = sysctl(mib, 2, &physical_memory, &length, NULL, 0);
    if (rc != 0)
    {
        ASSERT("sysctl failed for HW_MEMSIZE (%d)\n", errno);
    }
    else
    {
        lpBuffer->ullTotalPhys = (DWORDLONG)physical_memory;
        fRetVal = TRUE;
    }

#endif // HAVE_SYSCTL

    // Get swap file size, consider the ability to get the values optional
    // (don't return FALSE from the GlobalMemoryStatusEx)
#if HAVE_XSW_USAGE
    // This is available on OSX
    struct xsw_usage xsu;
    mib[0] = CTL_VM;
    mib[1] = VM_SWAPUSAGE;
    size_t length = sizeof(xsu);
    rc = sysctl(mib, 2, &xsu, &length, NULL, 0);
    if (rc == 0)
    {
        lpBuffer->ullTotalPageFile = xsu.xsu_total;
        lpBuffer->ullAvailPageFile = xsu.xsu_avail;
    }
#elif HAVE_XSWDEV
    // E.g. FreeBSD
    struct xswdev xsw;

    size_t length = 2;
    rc = sysctlnametomib("vm.swap_info", mib, &length);
    if (rc == 0)
    {
        int pagesize = getpagesize();
        // Aggregate the information for all swap files on the system
        for (mib[2] = 0; ; mib[2]++)
        {
            length = sizeof(xsw);
            rc = sysctl(mib, 3, &xsw, &length, NULL, 0);
            if ((rc < 0) || (xsw.xsw_version != XSWDEV_VERSION))
            {
                // All the swap files were processed or coreclr was built against
                // a version of headers not compatible with the current XSWDEV_VERSION.
                break;
            }

            DWORDLONG avail = xsw.xsw_nblks - xsw.xsw_used;
            lpBuffer->ullTotalPageFile += (DWORDLONG)xsw.xsw_nblks * pagesize;
            lpBuffer->ullAvailPageFile += (DWORDLONG)avail * pagesize;
        }
    }
#elif HAVE_SYSINFO
    // Linux
    struct sysinfo info;
    rc = sysinfo(&info);
    if (rc == 0)
    {
        lpBuffer->ullTotalPageFile = info.totalswap;
        lpBuffer->ullAvailPageFile = info.freeswap;
#if HAVE_SYSINFO_WITH_MEM_UNIT
        // A newer version of the sysinfo structure represents all the sizes
        // in mem_unit instead of bytes
        lpBuffer->ullTotalPageFile *= info.mem_unit;
        lpBuffer->ullAvailPageFile *= info.mem_unit;
#endif // HAVE_SYSINFO_WITH_MEM_UNIT
    }
#endif // HAVE_SYSINFO

    // Get the physical memory in use - from it, we can get the physical memory available.
    // We do this only when we have the total physical memory available.
    if (lpBuffer->ullTotalPhys > 0)
    {
#ifndef __APPLE__
        lpBuffer->ullAvailPhys = sysconf(SYSCONF_PAGES) * sysconf(_SC_PAGE_SIZE);
        INT64 used_memory = lpBuffer->ullTotalPhys - lpBuffer->ullAvailPhys;
        lpBuffer->dwMemoryLoad = (DWORD)((used_memory * 100) / lpBuffer->ullTotalPhys);
#else
        vm_size_t page_size;
        mach_port_t mach_port;
        mach_msg_type_number_t count;
        vm_statistics_data_t vm_stats;
        mach_port = mach_host_self();
        count = sizeof(vm_stats) / sizeof(natural_t);
        if (KERN_SUCCESS == host_page_size(mach_port, &page_size))
        {
            if (KERN_SUCCESS == host_statistics(mach_port, HOST_VM_INFO, (host_info_t)&vm_stats, &count))
            {
                lpBuffer->ullAvailPhys = (int64_t)vm_stats.free_count * (int64_t)page_size;
                INT64 used_memory = ((INT64)vm_stats.active_count + (INT64)vm_stats.inactive_count + (INT64)vm_stats.wire_count) *  (INT64)page_size;
                lpBuffer->dwMemoryLoad = (DWORD)((used_memory * 100) / lpBuffer->ullTotalPhys);
            }
        }
        mach_port_deallocate(mach_task_self(), mach_port);
#endif // __APPLE__
    }

    // There is no API to get the total virtual address space size on 
    // Unix, so we use a constant value representing 128TB, which is 
    // the approximate size of total user virtual address space on
    // the currently supported Unix systems.
    static const UINT64 _128TB = (1ull << 47); 
    lpBuffer->ullTotalVirtual = _128TB;
    lpBuffer->ullAvailVirtual = lpBuffer->ullAvailPhys;

    LOGEXIT("GlobalMemoryStatusEx returns %d\n", fRetVal);
    PERF_EXIT(GlobalMemoryStatusEx);

    return fRetVal;
}

PALIMPORT
DWORD
PALAPI
GetCurrentProcessorNumber()
{
#if HAVE_SCHED_GETCPU
    return sched_getcpu();
#else //HAVE_SCHED_GETCPU
    return -1;
#endif //HAVE_SCHED_GETCPU
}

BOOL
PALAPI
PAL_HasGetCurrentProcessorNumber()
{
    return HAVE_SCHED_GETCPU;
}

size_t
PALAPI
PAL_GetLogicalProcessorCacheSizeFromOS()
{
    size_t cacheSize = 0;

#ifdef _SC_LEVEL1_DCACHE_SIZE
    cacheSize = max(cacheSize, sysconf(_SC_LEVEL1_DCACHE_SIZE));
#endif
#ifdef _SC_LEVEL2_CACHE_SIZE
    cacheSize = max(cacheSize, sysconf(_SC_LEVEL2_CACHE_SIZE));
#endif
#ifdef _SC_LEVEL3_CACHE_SIZE
    cacheSize = max(cacheSize, sysconf(_SC_LEVEL3_CACHE_SIZE));
#endif
#ifdef _SC_LEVEL4_CACHE_SIZE
    cacheSize = max(cacheSize, sysconf(_SC_LEVEL4_CACHE_SIZE));
#endif

    return cacheSize;
}

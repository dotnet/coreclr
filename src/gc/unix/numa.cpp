// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*++



Module Name:

    numa.cpp

Abstract:

    Implementation of NUMA related APIs

--*/

#include <cstdint>
#include <cstddef>
#include <cassert>
#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <sys/resource.h>
#include <errno.h>
#include <sched.h>
#include "config.h"

#if HAVE_PTHREAD_NP_H
#include <pthread_np.h>
#endif

#if HAVE_SYS_MMAN_H
 #include <sys/mman.h>
#else
 #error "sys/mman.h required by GC PAL"
#endif // HAVE_SYS_MMAN_H

#include <pthread.h>
#include <dlfcn.h>
#ifdef __FreeBSD__
#include <stdlib.h>
#else
#include <alloca.h>
#endif

#include <algorithm>

typedef struct _PROCESSOR_NUMBER {
    uint16_t Group;
    uint8_t Number;
    uint8_t Reserved;
} PROCESSOR_NUMBER, *PPROCESSOR_NUMBER;

#include "numa_gc.h"

#if HAVE_NUMA_H
#include <numa.h>
#include <numaif.h>
#endif // HAVE_NUMA_H

#if HAVE_CPUSET_T
typedef cpuset_t cpu_set_t;
#endif

// Array mapping global CPU index to its affinity
CpuAffinity *g_cpuToAffinity = NULL;
uint32_t g_lastError = -1;

// Array mapping CPU group and index in the group to the global CPU index
short *g_groupAndIndexToCpu = NULL;
// Array mapping CPU group to the corresponding affinity mask of the CPUs in the group
KAFFINITY *g_groupToCpuMask = NULL;
// Array mapping CPU group to the number of processors in the group
uint8_t *g_groupToCpuCount = NULL;

// Total number of processors in the system
int g_cpuCount = 0;
// Total number of possible processors in the system
int g_possibleCpuCount = 0;
// Total number of CPU groups
int g_groupCount = 0;
// The highest NUMA node available
int g_highestNumaNode = 0;
// Is numa available
bool g_numaAvailable = false;

void* numaHandle = nullptr;

#if HAVE_NUMA_H
#define numa_free_cpumask numa_bitmask_free

// List of all functions from the numa library that are used
#define FOR_ALL_NUMA_FUNCTIONS \
    PER_FUNCTION_BLOCK(numa_available) \
    PER_FUNCTION_BLOCK(mbind) \
    PER_FUNCTION_BLOCK(numa_num_possible_cpus) \
    PER_FUNCTION_BLOCK(numa_max_node) \
    PER_FUNCTION_BLOCK(numa_allocate_cpumask) \
    PER_FUNCTION_BLOCK(numa_node_to_cpus) \
    PER_FUNCTION_BLOCK(numa_bitmask_weight) \
    PER_FUNCTION_BLOCK(numa_bitmask_isbitset) \
    PER_FUNCTION_BLOCK(numa_bitmask_free)

// Declare pointers to all the used numa functions
#define PER_FUNCTION_BLOCK(fn) extern decltype(fn)* fn##_ptr;
FOR_ALL_NUMA_FUNCTIONS
#undef PER_FUNCTION_BLOCK

// Redefine all calls to numa functions as calls through pointers that are set
// to the functions of libnuma in the initialization.
#define numa_available() numa_available_ptr()
#define mbind(...) mbind_ptr(__VA_ARGS__)
#define numa_num_possible_cpus() numa_num_possible_cpus_ptr()
#define numa_max_node() numa_max_node_ptr()
#define numa_allocate_cpumask() numa_allocate_cpumask_ptr()
#define numa_node_to_cpus(...) numa_node_to_cpus_ptr(__VA_ARGS__)
#define numa_bitmask_weight(...) numa_bitmask_weight_ptr(__VA_ARGS__)
#define numa_bitmask_isbitset(...) numa_bitmask_isbitset_ptr(__VA_ARGS__)
#define numa_bitmask_free(...) numa_bitmask_free_ptr(__VA_ARGS__)

#endif // HAVE_NUMA_H

static const int MaxCpusPerGroup = 8 * sizeof(KAFFINITY);
static const uint16_t NO_GROUP = 0xffff;

void SetLastError(uint32_t error)
{
    g_lastError = error;
}

uint32_t GetLastError()
{
    return g_lastError;
}

/*++
Function:
  FreeLookupArrays

Free CPU and group lookup arrays
--*/
void FreeLookupArrays()
{
    free(g_groupAndIndexToCpu);
    free(g_cpuToAffinity);
    free(g_groupToCpuMask);
    free(g_groupToCpuCount);

    g_groupAndIndexToCpu = NULL;
    g_cpuToAffinity = NULL;
    g_groupToCpuMask = NULL;
    g_groupToCpuCount = NULL;
}

/*++
Function:
  AllocateLookupArrays

Allocate CPU and group lookup arrays
Return true if the allocation succeeded
--*/
bool AllocateLookupArrays()
{
    g_groupAndIndexToCpu = (short*)malloc(g_groupCount * MaxCpusPerGroup * sizeof(short));
    if (g_groupAndIndexToCpu == NULL)
    {
        goto FAILED;
    }

    g_cpuToAffinity = (CpuAffinity*)malloc(g_possibleCpuCount * sizeof(CpuAffinity));
    if (g_cpuToAffinity == NULL)
    {
        goto FAILED;
    }

    g_groupToCpuMask = (KAFFINITY*)malloc(g_groupCount * sizeof(KAFFINITY));
    if (g_groupToCpuMask == NULL)
    {
        goto FAILED;
    }

    g_groupToCpuCount = (uint8_t*)malloc(g_groupCount * sizeof(uint8_t));
    if (g_groupToCpuCount == NULL)
    {
        goto FAILED;
    }

    memset(g_groupAndIndexToCpu, 0xff, g_groupCount * MaxCpusPerGroup * sizeof(short));
    memset(g_cpuToAffinity, 0xff, g_possibleCpuCount * sizeof(CpuAffinity));
    memset(g_groupToCpuMask, 0, g_groupCount * sizeof(KAFFINITY));
    memset(g_groupToCpuCount, 0, g_groupCount * sizeof(uint8_t));

    return true;

FAILED:
    FreeLookupArrays();

    return false;
}

/*++
Function:
  GetFullAffinityMask

Get affinity mask for the specified number of processors with all
the processors enabled.
--*/
KAFFINITY GetFullAffinityMask(int cpuCount)
{
    return ((KAFFINITY)1 << (cpuCount)) - 1;
}

uint32_t GetLogicalCpuCountFromOS()
{
    int nrcpus = 0;

#if HAVE_SYSCONF

#if defined(_ARM_) || defined(_ARM64_)
#define SYSCONF_GET_NUMPROCS       _SC_NPROCESSORS_CONF
#define SYSCONF_GET_NUMPROCS_NAME "_SC_NPROCESSORS_CONF"
#else
#define SYSCONF_GET_NUMPROCS       _SC_NPROCESSORS_ONLN
#define SYSCONF_GET_NUMPROCS_NAME "_SC_NPROCESSORS_ONLN"
#endif
    nrcpus = sysconf(SYSCONF_GET_NUMPROCS);
    if (nrcpus < 1)
    {
        ASSERT("sysconf failed for %s (%d)\n", SYSCONF_GET_NUMPROCS_NAME, errno);
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

/*++
Function:
  NUMASupportInitialize

Initialize data structures for getting and setting thread affinities to processors and
querying NUMA related processor information.
On systems with no NUMA support, it behaves as if there was a single NUMA node with
a single group of processors.
--*/
bool NUMASupportInitialize()
{
#if HAVE_NUMA_H
    numaHandle = dlopen("libnuma.so", RTLD_LAZY);
    if (numaHandle == 0)
    {
        numaHandle = dlopen("libnuma.so.1", RTLD_LAZY);
    }
    if (numaHandle != 0)
    {
        dlsym(numaHandle, "numa_allocate_cpumask");
#define PER_FUNCTION_BLOCK(fn) \
    fn##_ptr = (decltype(fn)*)dlsym(numaHandle, #fn); \
    if (fn##_ptr == NULL) { fprintf(stderr, "Cannot get symbol " #fn " from libnuma\n"); abort(); }
FOR_ALL_NUMA_FUNCTIONS
#undef PER_FUNCTION_BLOCK

        if (numa_available() == -1)
        {
            dlclose(numaHandle);
        }
        else
        {
            g_numaAvailable = true;

            struct bitmask *mask = numa_allocate_cpumask();
            int numaNodesCount = numa_max_node() + 1;

            g_possibleCpuCount = numa_num_possible_cpus();
            g_cpuCount = 0;
            g_groupCount = 0;

            for (int i = 0; i < numaNodesCount; i++)
            {
                int st = numa_node_to_cpus(i, mask);
                // The only failure that can happen is that the mask is not large enough
                // but that cannot happen since the mask was allocated by numa_allocate_cpumask
                assert(st == 0);
                unsigned int nodeCpuCount = numa_bitmask_weight(mask);
                g_cpuCount += nodeCpuCount;
                unsigned int nodeGroupCount = (nodeCpuCount + MaxCpusPerGroup - 1) / MaxCpusPerGroup;
                g_groupCount += nodeGroupCount;
            }

            if (!AllocateLookupArrays())
            {
                dlclose(numaHandle);
                return false;
            }

            uint16_t currentGroup = 0;
            int currentGroupCpus = 0;

            for (int i = 0; i < numaNodesCount; i++)
            {
                int st = numa_node_to_cpus(i, mask);
                // The only failure that can happen is that the mask is not large enough
                // but that cannot happen since the mask was allocated by numa_allocate_cpumask
                assert(st == 0);
                unsigned int nodeCpuCount = numa_bitmask_weight(mask);
                unsigned int nodeGroupCount = (nodeCpuCount + MaxCpusPerGroup - 1) / MaxCpusPerGroup;
                for (int j = 0; j < g_possibleCpuCount; j++)
                {
                    if (numa_bitmask_isbitset(mask, j))
                    {
                        if (currentGroupCpus == MaxCpusPerGroup)
                        {
                            g_groupToCpuCount[currentGroup] = MaxCpusPerGroup;
                            g_groupToCpuMask[currentGroup] = GetFullAffinityMask(MaxCpusPerGroup);
                            currentGroupCpus = 0;
                            currentGroup++;
                        }
                        g_cpuToAffinity[j].Node = i;
                        g_cpuToAffinity[j].Group = currentGroup;
                        g_cpuToAffinity[j].Number = currentGroupCpus;
                        g_groupAndIndexToCpu[currentGroup * MaxCpusPerGroup + currentGroupCpus] = j;
                        currentGroupCpus++;
                    }
                }

                if (currentGroupCpus != 0)
                {
                    g_groupToCpuCount[currentGroup] = currentGroupCpus;
                    g_groupToCpuMask[currentGroup] = GetFullAffinityMask(currentGroupCpus);
                    currentGroupCpus = 0;
                    currentGroup++;
                }
            }

            numa_free_cpumask(mask);

            g_highestNumaNode = numa_max_node();
        }
    }
#endif // HAVE_NUMA_H
    if (!g_numaAvailable)
    {
        // No NUMA
        g_possibleCpuCount = GetLogicalCpuCountFromOS();
        g_cpuCount = GetLogicalCpuCountFromOS();
        g_groupCount = 1;
        g_highestNumaNode = 0;

        if (!AllocateLookupArrays())
        {
            return false;
        }

        for (int i = 0; i < g_possibleCpuCount; i++)
        {
            g_cpuToAffinity[i].Number = i;
            g_cpuToAffinity[i].Group = 0;
        }
    }

    return true;
}

/*++
Function:
  NUMASupportCleanup

Cleanup of the NUMA support data structures
--*/
void NUMASupportCleanup()
{
    FreeLookupArrays();
#if HAVE_NUMA_H
    if (g_numaAvailable)
    {
        dlclose(numaHandle);
    }
#endif // HAVE_NUMA_H
}

/*++
Function:
  GetNumaHighestNodeNumber

See MSDN doc.
--*/
bool GetNumaHighestNodeNumber(uint32_t *HighestNodeNumber)
{
    *HighestNodeNumber = (uint32_t)g_highestNumaNode;

    return true;
}

/*++
Function:
  GetNumaProcessorNodeEx

See MSDN doc.
--*/
bool GetNumaProcessorNodeEx(PPROCESSOR_NUMBER Processor, uint16_t *NodeNumber)
{
    bool success = false;

    if ((Processor->Group < g_groupCount) && 
        (Processor->Number < MaxCpusPerGroup) && 
        (Processor->Reserved == 0))
    {  
        short cpu = g_groupAndIndexToCpu[Processor->Group * MaxCpusPerGroup + Processor->Number];
        if (cpu != -1)
        {
            *NodeNumber = g_cpuToAffinity[cpu].Node;
            success = true;
        }
    }

    if (!success)
    {
        *NodeNumber = 0xffff;
        SetLastError(ERROR_INVALID_PARAMETER);
    }

    return success;
}

/*++
Function:
  GetLogicalProcessorInformationEx

See MSDN doc.
--*/
bool GetLogicalProcessorInformationEx(
  LOGICAL_PROCESSOR_RELATIONSHIP RelationshipType,
  PSYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX Buffer,
  uint32_t *ReturnedLength)
{
    bool success = false;

    if (RelationshipType == RelationGroup)
    {
        size_t requiredSize = __builtin_offsetof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX, Group);
        requiredSize += __builtin_offsetof(GROUP_RELATIONSHIP, GroupInfo);
        requiredSize += g_groupCount * sizeof(PROCESSOR_GROUP_INFO);

        if (*ReturnedLength >= requiredSize)
        {
            Buffer->Relationship = RelationGroup;
            Buffer->Size = requiredSize;
            Buffer->Group.MaximumGroupCount = g_groupCount;
            Buffer->Group.ActiveGroupCount = g_groupCount;
            for (int i = 0; i < g_groupCount; i++)
            {
                Buffer->Group.GroupInfo[i].MaximumProcessorCount = MaxCpusPerGroup;
                Buffer->Group.GroupInfo[i].ActiveProcessorCount = g_groupToCpuCount[i];
                Buffer->Group.GroupInfo[i].ActiveProcessorMask = g_groupToCpuMask[i];
            }

            success = true;
        }
        else
        {
            SetLastError(ERROR_INSUFFICIENT_BUFFER);
        }

        *ReturnedLength = requiredSize;
    }
    else
    {
        // We only support the group relationship
        SetLastError(ERROR_INVALID_PARAMETER);  
    }

    return success;
}

uint32_t GetCurrentProcessorNumber()
{
#if HAVE_SCHED_GETCPU
    return sched_getcpu();
#else //HAVE_SCHED_GETCPU
    return -1;
#endif //HAVE_SCHED_GETCPU
}

bool HasGetCurrentProcessorNumber()
{
    return HAVE_SCHED_GETCPU;
}

/*++
Function:
  GetCurrentProcessorNumberEx

See MSDN doc.
--*/
void GetCurrentProcessorNumberEx(PPROCESSOR_NUMBER ProcNumber)
{
    uint32_t cpu = GetCurrentProcessorNumber();
    assert(cpu < g_possibleCpuCount);
    ProcNumber->Group = g_cpuToAffinity[cpu].Group;
    ProcNumber->Number = g_cpuToAffinity[cpu].Number;
}

/*++
Function:
  GetProcessAffinityMask

See MSDN doc.
--*/
bool GetProcessAffinityMask(uint64_t *lpProcessAffinityMask, uint64_t *lpSystemAffinityMask)
{
    bool success = false;

    uint64_t systemMask = GetFullAffinityMask(g_cpuCount);

#if HAVE_SCHED_GETAFFINITY
    int pid = getpid();
    cpu_set_t cpuSet;
    int st = sched_getaffinity(pid, sizeof(cpu_set_t), &cpuSet);
    if (st == 0)
    {
        uint16_t group = NO_GROUP;
        uint64_t processMask = 0;

        for (int i = 0; i < g_possibleCpuCount; i++)
        {
            if (CPU_ISSET(i, &cpuSet))
            {
                uint16_t g = g_cpuToAffinity[i].Group;
                if (group == NO_GROUP || g == group)
                {
                    group = g;
                    processMask |= ((uint64_t)1) << g_cpuToAffinity[i].Number;
                }
                else
                {
                    // The process has affinity in more than one group, in such case
                    // the function needs to return zero in both masks.
                    processMask = 0;
                    systemMask = 0;
                    group = NO_GROUP;
                    break;
                }
            }
        }

        success = true;

        *lpProcessAffinityMask = processMask;
        *lpSystemAffinityMask = systemMask;
    }
    else if (errno == EINVAL)
    {
        // There are more processors than can fit in a cpu_set_t
        // return zero in both masks.
        *lpProcessAffinityMask = 0;
        *lpSystemAffinityMask = 0;
        success = true;
    }
    else
    {
        // We should not get any of the errors that the sched_getaffinity can return since none
        // of them applies for the current thread, so this is an unexpected kind of failure.
        SetLastError(ERROR_GEN_FAILURE);
    }
#else // HAVE_SCHED_GETAFFINITY
    // There is no API to manage thread affinity, so let's return both affinity masks
    // with all the CPUs on the system set.
    *lpSystemAffinityMask = systemMask;
    *lpProcessAffinityMask = systemMask;

    success = true;
#endif // HAVE_SCHED_GETAFFINITY
    
    return success;
}

/*++
Function:
  VirtualAllocExNuma

See MSDN doc.
--*/
void *VirtualAllocExNuma(
  void *lpAddress,
  size_t dwSize,
  uint32_t flAllocationType,
  uint32_t flProtect,
  uint32_t nndPreferred)
{

    void *result = NULL;

    if (nndPreferred <= g_highestNumaNode)
    {
        // GC only uses VirtualAllocExNuma with MEM_COMMIT and PAGE_READWRITE,
        // this method would need changed for other flags
        if (flAllocationType == MEM_COMMIT && flProtect == PAGE_READWRITE
            && mprotect(lpAddress, dwSize, PROT_WRITE | PROT_READ) == 0)
        {
#if HAVE_NUMA_H
            if (result != NULL && g_numaAvailable)
            {
                int nodeMaskLength = (g_highestNumaNode + 1 + sizeof(unsigned long) - 1) / sizeof(unsigned long);
                unsigned long *nodeMask = (unsigned long*)alloca(nodeMaskLength * sizeof(unsigned long));
                memset(nodeMask, 0, nodeMaskLength);

                int index = nndPreferred / sizeof(unsigned long);
                int mask = ((unsigned long)1) << (nndPreferred & (sizeof(unsigned long) - 1));
                nodeMask[index] = mask;

                int st = mbind(result, dwSize, MPOL_PREFERRED, nodeMask, g_highestNumaNode, 0);

                assert(st == 0);
                // If the mbind fails, we still return the allocated memory since the nndPreferred is just a hint
            }
#endif // HAVE_NUMA_H
        }
        else
        {
            assert(!"VirtualAllocExNuma only works with MEM_COMMIT and PAGE_READWRITE for standalone gc.");
        }
    }
    else
    {
        // The specified node number is larger than the maximum available one
        SetLastError(ERROR_INVALID_PARAMETER);
    }

    return result;
}

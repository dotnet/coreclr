// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifdef _WIN32
#include <Windows.h>
#endif // _WIN32
#include <cstdint>
#include <cstddef>
#include <cassert>
#include <memory>

#include "common.h"

#include "gcenv.structs.h"
#include "gcenv.base.h"
#include "gcenv.os.h"
#include "volatile.h"

#ifdef PLATFORM_UNIX
#include "config.h"
#include "../unix/numa.h"
#endif // PLATFORM_UNIX

static bool g_fEnableGCNumaAware;

struct CPU_Group_Info 
{
    WORD    nr_active;  // at most 64
    WORD    reserved[1];
    WORD    begin;
    WORD    end;
    DWORD_PTR   active_mask;
    DWORD   groupWeight;
    DWORD   activeThreadWeight;
};

static bool g_fEnableGCCPUGroups;
static bool g_fHadSingleProcessorAtStartup;
static DWORD  g_nGroups;
static DWORD g_nProcessors;
static CPU_Group_Info *g_CPUGroupInfoArray;

void InitNumaNodeInfo()
{
    ULONG highest = 0;
    
    g_fEnableGCNumaAware = false;

    if (!g_fIsNumaAwareEnabledByConfig)
        return;

    // fail to get the highest numa node number
    if (!GetNumaHighestNodeNumber(&highest) || (highest == 0))
        return;

    g_fEnableGCNumaAware = true;
    return;
}

#if (defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_))
// Calculate greatest common divisor
DWORD GCD(DWORD u, DWORD v)
{
    while (v != 0)
    {
        DWORD dwTemp = v;
        v = u % v;
        u = dwTemp;
    }

    return u;
}

// Calculate least common multiple
DWORD LCM(DWORD u, DWORD v)
{
    return u / GCD(u, v) * v;
}
#endif

bool InitCPUGroupInfoArray()
{
#if (defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_))
    BYTE *bBuffer = NULL;
    SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX *pSLPIEx = NULL;
    SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX *pRecord = NULL;
    DWORD cbSLPIEx = 0;
    DWORD byteOffset = 0;
    DWORD dwNumElements = 0;
    DWORD dwWeight = 1;

    if (GetLogicalProcessorInformationEx(RelationGroup, pSLPIEx, &cbSLPIEx) &&
                      GetLastError() != ERROR_INSUFFICIENT_BUFFER)
        return false;

    assert(cbSLPIEx);

    // Fail to allocate buffer
    bBuffer = new (std::nothrow) BYTE[ cbSLPIEx ];
    if (bBuffer == NULL)
        return false;

    pSLPIEx = (SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX *)bBuffer;
    if (!GetLogicalProcessorInformationEx(RelationGroup, pSLPIEx, &cbSLPIEx))
    {
        delete[] bBuffer;
        return false;
    }

    pRecord = pSLPIEx;
    while (byteOffset < cbSLPIEx)
    {
        if (pRecord->Relationship == RelationGroup)
        {
            g_nGroups = pRecord->Group.ActiveGroupCount;
            break;
        }
        byteOffset += pRecord->Size;
        pRecord = (SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX *)(bBuffer + byteOffset);
    }

    g_CPUGroupInfoArray = new (std::nothrow) CPU_Group_Info[g_nGroups];
    if (g_CPUGroupInfoArray == NULL) 
    {
        delete[] bBuffer;
        return false;
    }

    for (DWORD i = 0; i < g_nGroups; i++)
    {
        g_CPUGroupInfoArray[i].nr_active   = (WORD)pRecord->Group.GroupInfo[i].ActiveProcessorCount;
        g_CPUGroupInfoArray[i].active_mask = pRecord->Group.GroupInfo[i].ActiveProcessorMask;
        g_nProcessors += g_CPUGroupInfoArray[i].nr_active;
        dwWeight = LCM(dwWeight, (DWORD)g_CPUGroupInfoArray[i].nr_active);
    }

    // The number of threads per group that can be supported will depend on the number of CPU groups
    // and the number of LPs within each processor group. For example, when the number of LPs in
    // CPU groups is the same and is 64, the number of threads per group before weight overflow
    // would be 2^32/2^6 = 2^26 (64M threads)
    for (DWORD i = 0; i < g_nGroups; i++)
    {
        g_CPUGroupInfoArray[i].groupWeight = dwWeight / (DWORD)g_CPUGroupInfoArray[i].nr_active;
        g_CPUGroupInfoArray[i].activeThreadWeight = 0;
    }

    delete[] bBuffer;  // done with it; free it
    return true;
#else
    return false;
#endif
}

bool InitCPUGroupInfoRange()
{
#if (defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_))
    WORD begin   = 0;
    WORD nr_proc = 0;

    for (WORD i = 0; i < g_nGroups; i++) 
    {
        nr_proc += g_CPUGroupInfoArray[i].nr_active;
        g_CPUGroupInfoArray[i].begin = begin;
        g_CPUGroupInfoArray[i].end   = nr_proc - 1;
        begin = nr_proc;
    }

    return true;
#else
    return false;
#endif
}

void InitCPUGroupInfo()
{
    g_fEnableGCCPUGroups = false;

#if (defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_))
    if (!g_fIsCPUGroupEnabledByConfig)
        return;

    if (!InitCPUGroupInfoArray())
        return;

    if (!InitCPUGroupInfoRange())
        return;

    // only enable CPU groups if more than one group exists
    g_fEnableGCCPUGroups = g_nGroups > 1;
#endif // _TARGET_AMD64_ || _TARGET_ARM64_

    // Determine if the process is affinitized to a single processor (or if the system has a single processor)
    DWORD_PTR processAffinityMask, systemAffinityMask;
    if (GetProcessAffinityMask(
#ifdef _WIN32
                                GetCurrentProcess(), 
#endif
                                &processAffinityMask, &systemAffinityMask))
    {
        processAffinityMask &= systemAffinityMask;
        if (processAffinityMask != 0 && // only one CPU group is involved
            (processAffinityMask & (processAffinityMask - 1)) == 0) // only one bit is set
        {
            g_fHadSingleProcessorAtStartup = true;
        }
    }
}

bool GCToOSInterface::CanEnableGCNumaAware()
{
    return g_fEnableGCNumaAware;
}

bool GCToOSInterface::GetNumaProcessorNodeEx(PPROCESSOR_NUMBER proc_no, uint16_t *node_no)
{
    assert(g_fEnableGCNumaAware);
    return ::GetNumaProcessorNodeEx(proc_no, node_no);
}

void* GCToOSInterface::VirtualAllocExNuma(void *lpAddr, size_t dwSize, uint32_t allocType, uint32_t prot, uint32_t node)
{
    assert(g_fEnableGCNumaAware);
    return ::VirtualAllocExNuma(
#ifdef _WIN32
                                ::GetCurrentProcess(), 
#endif
                                lpAddr, dwSize, allocType, prot, node);
}

bool GCToOSInterface::CanEnableGCCPUGroups()
{
    return g_fEnableGCCPUGroups;
}

uint16_t GCToOSInterface::GetNumActiveProcessors()
{
    assert(g_fEnableGCCPUGroups);
    return (uint16_t)g_nProcessors;
}

void GCToOSInterface::GetGroupForProcessor(uint16_t processor_number, uint16_t* group_number, uint16_t* group_processor_number)
{
    assert(g_fEnableGCCPUGroups);

#if !defined(FEATURE_REDHAWK) && (defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_))
    WORD bTemp = 0;
    WORD bDiff = processor_number - bTemp;

    for (WORD i=0; i < g_nGroups; i++)
    {
        bTemp += g_CPUGroupInfoArray[i].nr_active;
        if (bTemp > processor_number)
        {
            *group_number = i;
            *group_processor_number = bDiff;
            break;
        }
        bDiff = processor_number - bTemp;
    }
#else
    *group_number = 0;
    *group_processor_number = 0;
#endif
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __NUMA_H
#define __NUMA_H


#define PAGE_NOACCESS                   0x01
#define PAGE_READONLY                   0x02
#define PAGE_READWRITE                  0x04
#define PAGE_WRITECOPY                  0x08
#define PAGE_EXECUTE                    0x10
#define PAGE_EXECUTE_READ               0x20
#define PAGE_EXECUTE_READWRITE          0x40
#define PAGE_EXECUTE_WRITECOPY          0x80
#define MEM_COMMIT                      0x1000
#define MEM_RESERVE                     0x2000
#define MEM_DECOMMIT                    0x4000
#define MEM_RELEASE                     0x8000
#define MEM_RESET                       0x80000
#define MEM_FREE                        0x10000
#define MEM_PRIVATE                     0x20000
#define MEM_MAPPED                      0x40000
#define MEM_TOP_DOWN                    0x100000
#define MEM_WRITE_WATCH                 0x200000
#define MEM_RESERVE_EXECUTABLE          0x40000000 // reserve memory using executable memory allocator

#define ERROR_INVALID_HANDLE 6L
#define ERROR_GEN_FAILURE 31L
#define ERROR_INVALID_PARAMETER 87L
#define ERROR_INSUFFICIENT_BUFFER 122L

typedef enum _PROCESSOR_CACHE_TYPE { 
  CacheUnified,
  CacheInstruction,
  CacheData,
  CacheTrace
} PROCESSOR_CACHE_TYPE;

typedef enum _LOGICAL_PROCESSOR_RELATIONSHIP { 
  RelationProcessorCore,
  RelationNumaNode,
  RelationCache,
  RelationProcessorPackage,
  RelationGroup,
  RelationAll               = 0xffff
} LOGICAL_PROCESSOR_RELATIONSHIP;

typedef uint64_t KAFFINITY;

#define ANYSIZE_ARRAY 1

typedef struct _GROUP_AFFINITY {
  KAFFINITY Mask;
  uint16_t      Group;
  uint16_t      Reserved[3];
} GROUP_AFFINITY, *PGROUP_AFFINITY;

typedef struct _PROCESSOR_GROUP_INFO {
  uint8_t      MaximumProcessorCount;
  uint8_t      ActiveProcessorCount;
  uint8_t      Reserved[38];
  KAFFINITY ActiveProcessorMask;
} PROCESSOR_GROUP_INFO, *PPROCESSOR_GROUP_INFO;

typedef struct _PROCESSOR_RELATIONSHIP {
  uint8_t           Flags;
  uint8_t           EfficiencyClass;
  uint8_t           Reserved[21];
  uint8_t           GroupCount;
  GROUP_AFFINITY GroupMask[ANYSIZE_ARRAY];
} PROCESSOR_RELATIONSHIP, *PPROCESSOR_RELATIONSHIP;

typedef struct _GROUP_RELATIONSHIP {
  uint16_t                 MaximumGroupCount;
  uint16_t                 ActiveGroupCount;
  uint8_t                 Reserved[20];
  PROCESSOR_GROUP_INFO GroupInfo[ANYSIZE_ARRAY];
} GROUP_RELATIONSHIP, *PGROUP_RELATIONSHIP;

typedef struct _NUMA_NODE_RELATIONSHIP {
  uint32_t          NodeNumber;
  uint8_t           Reserved[20];
  GROUP_AFFINITY GroupMask;
} NUMA_NODE_RELATIONSHIP, *PNUMA_NODE_RELATIONSHIP;

typedef struct _CACHE_RELATIONSHIP {
  uint8_t                 Level;
  uint8_t                 Associativity;
  uint16_t                 LineSize;
  uint32_t                CacheSize;
  PROCESSOR_CACHE_TYPE Type;
  uint8_t                 Reserved[20];
  GROUP_AFFINITY       GroupMask;
} CACHE_RELATIONSHIP, *PCACHE_RELATIONSHIP;

typedef struct _SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX {
  LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
  uint32_t                          Size;
  union {
    PROCESSOR_RELATIONSHIP Processor;
    NUMA_NODE_RELATIONSHIP NumaNode;
    CACHE_RELATIONSHIP     Cache;
    GROUP_RELATIONSHIP     Group;
  };
} SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX, *PSYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX;

// CPU affinity descriptor
struct CpuAffinity
{
    // NUMA node
    uint8_t Node;
    // CPU number relative to the group the CPU is in
    uint8_t Number;
    // CPU group
    uint16_t Group;
};

uint32_t GetLastError();

bool GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP RelationshipType,
                                      PSYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX Buffer,
                                      uint32_t *ReturnedLength);
bool GetProcessAffinityMask(uint64_t *lpProcessAffinityMask, uint64_t *lpSystemAffinityMask);

bool NUMASupportInitialize();
bool GetNumaHighestNodeNumber(uint32_t *HighestNodeNumber);
bool GetNumaProcessorNodeEx(PPROCESSOR_NUMBER Processor, uint16_t *NodeNumber);
void *VirtualAllocExNuma(void *lpAddress,
                         size_t dwSize,
                         uint32_t flAllocationType,
                         uint32_t flProtect,
                         uint32_t nndPreferred);
#endif // __NUMA_H
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __GCCONFIG_H__
#define __GCCONFIG_H__

// Flags that may inhabit the number returned for the HeapVerifyLevel config option.
// Keep this in sync with vm\eeconfig.h if this ever changes.
enum HeapVerifyFlags {
    HEAPVERIFY_NONE             = 0,
    HEAPVERIFY_GC               = 1,   // Verify the heap at beginning and end of GC
    HEAPVERIFY_BARRIERCHECK     = 2,   // Verify the brick table
    HEAPVERIFY_SYNCBLK          = 4,   // Verify sync block scanning

    // the following options can be used to mitigate some of the overhead introduced
    // by heap verification.  some options might cause heap verifiction to be less
    // effective depending on the scenario.

    HEAPVERIFY_NO_RANGE_CHECKS  = 0x10,   // Excludes checking if an OBJECTREF is within the bounds of the managed heap
    HEAPVERIFY_NO_MEM_FILL      = 0x20,   // Excludes filling unused segment portions with fill pattern
    HEAPVERIFY_POST_GC_ONLY     = 0x40,   // Performs heap verification post-GCs only (instead of before and after each GC)
    HEAPVERIFY_DEEP_ON_COMPACT  = 0x80    // Performs deep object verfication only on compacting GCs.
};

// GCConfigStringHolder is a wrapper around a configuration string obtained
// from the EE. Such strings must be disposed using GCToEEInterface::FreeStringConfigValue,
// so this class ensures that is done correctly.
class GCConfigStringHolder
{
private:
    const char* m_str;

public:
    // Constructs a new GCConfigStringHolder around a string obtained from
    // GCToEEInterface::GetStringConfigValue.
    explicit GCConfigStringHolder(const char* str)
      : m_str(str) {}

    GCConfigStringHolder(const GCConfigStringHolder&) = delete;
    GCConfigStringHolder& operator=(const GCConfigStringHolder&) = delete;
    GCConfigStringHolder(GCConfigStringHolder&&) = default;

    // Frees a string config value by delegating to GCToEEInterface::FreeStringConfigValue.
    ~GCConfigStringHolder()
    {
        if (m_str)
        {
            GCToEEInterface::FreeStringConfigValue(m_str);
        }

        m_str = nullptr;
    }

    // Retrieves the wrapped config string.
    const char* Get() const { return m_str; }
};

#define GC_CONFIGURATION_KEYS \
  BOOL_CONFIG(ServerGC,     "gcServer",     false, "Whether we should be using Server GC")     \
  BOOL_CONFIG(ConcurrentGC, "gcConcurrent", true,  "Whether we should be using Concurrent GC") \
  BOOL_CONFIG(ConservativeGC, "gcConservative", false, "Enables/Disables conservative GC")     \
  BOOL_CONFIG(ForceCompact, "gcForceCompact", false,                                           \
      "When set to true, always do compacting GC")                                             \
  BOOL_CONFIG(RetainVM,     "GCRetainVM",   false,                                             \
      "When set we put the segments that should be deleted on a standby list (instead of "     \
      "releasing them back to the OS) which will be considered to satisfy new segment requests"\
     " (note that the same thing can be specified via API which is the supported way)")        \
  BOOL_CONFIG(StressMix,    "GCStressMix",  false,                                             \
      "Specifies whether the GC mix mode is enabled or not")                                   \
  BOOL_CONFIG(BreakOnOOM,   "GCBreakOnOOM", false,                                             \
      "Does a DebugBreak at the soonest time we detect an OOM")                                \
  BOOL_CONFIG(NoAffinitize, "GCNoAffinitize", false,                                           \
      "If set, do not affinitize server GC threads")                                           \
  BOOL_CONFIG(AppDomainLeaks, "AppDomainAgilityChecked", false, "unused")                      \
  INT_CONFIG(HeapVerifyLevel, "HeapVerify", HEAPVERIFY_NONE,                                   \
      "When set verifies the integrity of the managed heap on entry and exit of each GC")      \
  INT_CONFIG(GCtraceStart,  "GCtraceStart", 0, "unused")                                       \
  INT_CONFIG(GCtraceEnd,    "GCtraceEnd",   0, "unused")                                       \
  INT_CONFIG(GCtraceFac,    "GCtraceFac",   0, "unused")                                       \
  INT_CONFIG(GCprnLvl,      "GCprnLvl",     0, "unused")                                       \
  INT_CONFIG(LOHCompactionMode, "GCLOHCompact", 0, "Specifies the LOH compaction mode")        \
  INT_CONFIG(BGCSpinCount,  "BGCSpinCount", 140, "Specifies the bgc spin count")               \
  INT_CONFIG(BGCSpin,       "BGCSpin",      2,   "Specifies the bgc spin time")                \
  INT_CONFIG(HeapCount,     "GCHeapCount",  0,   "Specifies the number of server GC heaps")    \
  INT_CONFIG(Gen0Size,      "GCgen0size",   0, "Specifies the smallest gen0 size")             \
  INT_CONFIG(SegmentSize,   "GCSegmentSize", 0, "Specifies the managed heap segment size")     \
  INT_CONFIG(LatencyMode,   "GCLatencyMode", 0,                                                \
      "Specifies the GC latency mode - batch, interactive or low latency (note that the same " \
      "thing can be specified via API which is the supported way")                             \
  STRING_CONFIG(LogFile,    "GCLogFile",    "Specifies the name of the GC log file")

// This class is responsible for retreiving configuration information
// for how the GC should operate.
class GCConfig
{
public:
#define BOOL_CONFIG(name, unused_key, unused_default, unused_doc) \
  static bool Get##name();
#define INT_CONFIG(name, unused_key, unused_default, unused_doc) \
  static int64_t Get##name();
#define STRING_CONFIG(name, unused_key, unused_doc) \
  static GCConfigStringHolder Get##name();
GC_CONFIGURATION_KEYS
#undef BOOL_CONFIG
#undef INT_CONFIG
#undef STRING_CONFIG
};

#endif // __GCCONFIG_H__

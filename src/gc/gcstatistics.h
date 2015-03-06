//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

#ifndef __GCSTATISTICS_H 
#define __GCSTATISTICS_H
#include "gcpriv.h"
#include "gc.h"

// GC specific statistics, tracking counts and timings for GCs occuring in the system.
// This writes the statistics to a file every 60 seconds, if a file is specified in
// COMPLUS_GcMixLog

struct GCStatistics
    : public StatisticsBase
{
    // initialized to the contents of COMPLUS_GcMixLog, or NULL, if not present
    static WCHAR* logFileName;
    static FILE*  logFile;

    // number of times we executed a background GC, a foreground GC, or a
    // non-concurrent GC
    int cntBGC, cntFGC, cntNGC;

    // min, max, and total time spent performing BGCs, FGCs, NGCs
    // (BGC time includes everything between the moment the BGC starts until 
    // it completes, i.e. the times of all FGCs occuring concurrently)
    MinMaxTot bgc, fgc, ngc;

    // number of times we executed a compacting GC (sweeping counts can be derived)
    int cntCompactNGC, cntCompactFGC;

    // count of reasons
    int cntReasons[reason_max];

    // count of condemned generation, by NGC and FGC:
    int cntNGCGen[max_generation+1];
    int cntFGCGen[max_generation];
    
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Internal mechanism:

    virtual void Initialize();
    virtual void DisplayAndUpdate();

    // Public API

    static BOOL Enabled()
    { return logFileName != NULL; }

    void AddGCStats(const gc_mechanisms& settings, size_t timeInMSec);
};

#endif // __GCSTATISTICS_H
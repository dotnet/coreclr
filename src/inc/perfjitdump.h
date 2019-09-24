// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ===========================================================================

#ifndef PERF_JITDUMP_H
#define PERF_JITDUMP_H

struct PerfJitDumpState;

// Generates a perf jitdump file.
class PerfJitDump
{
private:
    static PerfJitDumpState& GetState();

public:
    // Start the jitdump file
    static void Start();

    // Log a method to the jitdump file.
    static void LogMethod(void* pCode, size_t codeSize, const char* symbol);

    // Finish the jitdump file
    static void Finish();
};

#endif // PERF_JITDUMP_H


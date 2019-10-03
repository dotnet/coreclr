// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ===========================================================================

#ifndef PERF_JITDUMP_H
#define PERF_JITDUMP_H

int
PALAPI
// Start the jitdump file
PAL_PerfJitDump_Start(const char* path);

int
PALAPI
// Log a method to the jitdump file.
PAL_PerfJitDump_LogMethod(void* pCode, size_t codeSize, const char* symbol, void* debugInfo, void* unwindInfo);

int
PALAPI
// Finish the jitdump file
PAL_PerfJitDump_Finish();

#endif // PERF_JITDUMP_H


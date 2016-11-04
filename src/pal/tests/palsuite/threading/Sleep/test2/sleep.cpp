// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** Source: Sleep.c
**
** Purpose: Test to establish whether the Sleep function stops the thread from 
** executing for the specified times.
**
** Dependencies: GetTickCount
** 

**
**=========================================================*/

#include <palsuite.h>

/* 
 * times in 10^(-3) seconds
 */

int SleepTimes[] =
{
    60000,
    300000,
    1800000,
    3200000
};

/* Milliseconds of error which are acceptable Function execution time, etc. */
const int AcceptableEarlyDiff = -300;

int __cdecl main( int argc, char **argv ) 
{
    UINT64 OldTimeStamp;
    UINT64 NewTimeStamp;
    int MaxDelta;
    int TimeDelta;
    DWORD i;

    if(0 != (PAL_Initialize(argc, argv)))
    {
        return ( FAIL );
    }

    LARGE_INTEGER performanceFrequency;
    if (!QueryPerformanceFrequency(&performanceFrequency))
    {
        return FAIL;
    }

    for( i = 0; i < sizeof(SleepTimes) / sizeof(SleepTimes[0]); i++)
    {
        OldTimeStamp = GetHighPrecisionTimeStamp(performanceFrequency);
        Sleep(SleepTimes[i]);
        NewTimeStamp = GetHighPrecisionTimeStamp(performanceFrequency);

        TimeDelta = static_cast<int>(NewTimeStamp - OldTimeStamp);

        /* For longer intervals use a 10 percent tolerance */
        int AcceptableLateDiff = 300;
        if ((SleepTimes[i] * 0.1) > AcceptableLateDiff)
        {
            AcceptableLateDiff = (int)(SleepTimes[i] * 0.1);
        }

        int diff = TimeDelta - SleepTimes[i];
        if (diff < AcceptableEarlyDiff || diff > AcceptableLateDiff)
        {
            Fail("The sleep function slept for %u ms when it should have "
             "slept for %u ms\n", TimeDelta, SleepTimes[i]);
        }
    }
    PAL_Terminate();
    return ( PASS );

}

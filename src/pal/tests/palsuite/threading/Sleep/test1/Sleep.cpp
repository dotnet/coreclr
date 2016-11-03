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
** Dependencies: GetSystemTime
**               Fail   
**               Trace
** 

**
**=========================================================*/

#include <palsuite.h>

int SleepTimes[] =
{
    0,
    50,
    100,
    500,
    2000
};

/* Milliseconds of error which are acceptable Function execution time, etc. */
const int AcceptableEarlyDiff = -100;

int __cdecl main( int argc, char **argv ) 
{
    DWORD OldTimeStamp;
    DWORD NewTimeStamp;
    int TimeDelta;
    DWORD i;

    if(0 != (PAL_Initialize(argc, argv)))
    {
        return ( FAIL );
    }

    for( i = 0; i < sizeof(SleepTimes) / sizeof(SleepTimes[0]); i++)
    {
        OldTimeStamp = GetTickCount();
        Sleep(SleepTimes[i]);
        NewTimeStamp = GetTickCount();

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
            Fail("The sleep function slept for %d ms when it should have "
             "slept for %d ms\n", TimeDelta, SleepTimes[i]);
        }
    }
    PAL_Terminate();
    return ( PASS );

}

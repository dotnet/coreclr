// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=====================================================================
**
** Source:  test1.c
**
** Purpose: Tests that SleepEx correctly sleeps for a given amount of time,
**          regardless of the alertable flag.
**
**
**===================================================================*/

#include <palsuite.h>

typedef struct
{
    int SleepTime;
    BOOL Alertable;
} testCase;

testCase testCases[] =
{
    {0, FALSE},
    {50, FALSE},
    {100, FALSE},
    {500, FALSE},
    {2000, FALSE},

    {0, TRUE},
    {50, TRUE},
    {100, TRUE},
    {500, TRUE},
    {2000, TRUE},
};

/* Milliseconds of error which are acceptable Function execution time, etc. */
const int AcceptableEarlyDiff = -100;

int __cdecl main( int argc, char **argv ) 
{
    DWORD OldTimeStamp;
    DWORD NewTimeStamp;
    int TimeDelta;
    DWORD i;

    if (0 != (PAL_Initialize(argc, argv)))
    {
        return FAIL;
    }

    for (i = 0; i<sizeof(testCases) / sizeof(testCases[0]); i++)
    {
        OldTimeStamp = GetTickCount();

        SleepEx(testCases[i].SleepTime, testCases[i].Alertable);

        NewTimeStamp = GetTickCount();

        TimeDelta = static_cast<int>(NewTimeStamp - OldTimeStamp);

        /* For longer intervals use a 10 percent tolerance */
        int AcceptableLateDiff = 300;
        if ((testCases[i].SleepTime * 0.1) > AcceptableLateDiff)
        {
            AcceptableLateDiff = (int)(testCases[i].SleepTime * 0.1);
        }

        int diff = TimeDelta - testCases[i].SleepTime;
        if (diff < AcceptableEarlyDiff || diff > AcceptableLateDiff)
        {
            Fail("The sleep function slept for %d ms when it should have "
             "slept for %d ms\n", TimeDelta, testCases[i].SleepTime);
        }
    }

    PAL_Terminate();
    return PASS;
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*++



Module Name:

    cruntime/misctls.ccpp

Abstract:

    Implementation of C runtime functions that don't fit anywhere else
    and depend on per-thread data



--*/

#include "pal/thread.hpp"
#include "pal/palinternal.h"

extern "C"
{
#include "pal/dbgmsg.h"
#include "pal/misc.h"
}

#include <errno.h>
/* <stdarg.h> needs to be included after "palinternal.h" to avoid name
   collision for va_start and va_end */
#include <stdarg.h>
#include <time.h>
#if HAVE_CRT_EXTERNS_H
#include <crt_externs.h>
#endif  // HAVE_CRT_EXTERNS_H

using namespace CorUnix;

SET_DEFAULT_DEBUG_CHANNEL(CRT);

/*++
Function:

    localtime

See MSDN for more details.
--*/

struct PAL_tm *
__cdecl
PAL_localtime(const PAL_time_t *clock)
{
    CPalThread *pThread = NULL;
    struct tm tmpResult;
    struct PAL_tm *result = NULL;

    PERF_ENTRY(localtime);
    ENTRY( "localtime( clock=%p )\n",clock );

    /* Get the per-thread buffer from the thread structure. */
    pThread = InternalGetCurrentThread();

    result = &pThread->crtInfo.localtimeBuffer;

    localtime_r(reinterpret_cast<const time_t*>(clock), &tmpResult);

    // Copy the result into the Windows struct.
    result->tm_sec = tmpResult.tm_sec;
    result->tm_min = tmpResult.tm_min;
    result->tm_hour = tmpResult.tm_hour;
    result->tm_mday = tmpResult.tm_mday;
    result->tm_mon  = tmpResult.tm_mon;
    result->tm_year = tmpResult.tm_year;
    result->tm_wday = tmpResult.tm_wday;
    result->tm_yday = tmpResult.tm_yday;
    result->tm_isdst = tmpResult.tm_isdst;

    LOGEXIT( "localtime returned %p\n", result );
    PERF_EXIT(localtime);

    return result;
}

/*++
Function:

    ctime

    There appears to be a difference between the FreeBSD and windows
    implementations.  FreeBSD gives Wed Dec 31 18:59:59 1969 for a
    -1 param, and Windows returns NULL

See MSDN for more details.
--*/
char *
__cdecl
PAL_ctime( const PAL_time_t *clock )
{
    CPalThread *pThread = NULL;
    char * retval = NULL;

    PERF_ENTRY(ctime);
    ENTRY( "ctime( clock=%p )\n",clock );
    if(*clock < 0)
    {
        /*If the input param is less than zero the value
         *returned is less than the Unix epoch
         *1st of January 1970*/
        WARN("The input param is less than zero");
        goto done;
    }

    /* Get the per-thread buffer from the thread structure. */
    pThread = InternalGetCurrentThread();

    retval = pThread->crtInfo.ctimeBuffer;

    ctime_r(reinterpret_cast<const time_t*>(clock),retval);

done:

    LOGEXIT( "ctime() returning %p (%s)\n",retval,retval);
    PERF_EXIT(ctime);

    return retval;
}

unsigned int GetExponent(double d)
{
    return (*((unsigned int*)&d + 1) >> 20) & 0x000007ff;
}

unsigned long long GetMantissa(double d)
{
    return (unsigned long long)*((unsigned int*)&d) | ((unsigned long long)(*((unsigned int*)&d + 1) & 0x000fffff) << 32);
}

int GetSign(double d)
{
    return (int)(*((unsigned int*)&d + 1) >> 31);
}
/**
Function:

    _ecvt

See MSDN for more information.

NOTES:
    There is a difference between PAL _ecvt and Win32 _ecvt.

    If Window's _ecvt receives a double 0.000000000000000000005, and count 50
    the result is "49999999999999998000000000000000000000000000000000"

    Under BSD the same call will result in :
    49999999999999998021734900744965462766153934333829

    The difference is due to the difference between BSD and Win32 sprintf.

--*/
char * __cdecl
_ecvt( double value, int count, int * dec, int * sign )
{
    PERF_ENTRY(_ecvt);
    ENTRY( "_ecvt( value=%.30g, count=%d, dec=%p, sign=%p )\n",
           value, count, dec, sign );
    
    _ASSERTE(dec != nullptr && sign != nullptr);
    CPalThread *pThread = InternalGetCurrentThread();
    LPSTR lpStartOfReturnBuffer = pThread->crtInfo.ECVTBuffer;

    if (count > ECVT_MAX_COUNT_SIZE)
    {
        count = ECVT_MAX_COUNT_SIZE;
    }

    const int SCALE_NAN = (int)0x80000000;
    const int SCALE_INF = 0x7FFFFFFF;

    if (GetExponent(value) == 0x7ff)
    {
        *dec = GetMantissa(value) != 0 ? SCALE_NAN : SCALE_INF;
        *sign = GetSign(value);
        lpStartOfReturnBuffer[0] = '\0';
        return lpStartOfReturnBuffer;
    }

#ifndef __APPLE__
    // OSX doesn't support evct_r. it support evct but we cannot use it either as it is not thread safe
    ecvt_r(value, count, dec, sign, lpStartOfReturnBuffer, ECVT_MAX_BUFFER_SIZE);
    return lpStartOfReturnBuffer;
#else // __APPLE__

    CHAR TempBuffer[ECVT_MAX_BUFFER_SIZE];
   
    *dec = *sign = 0;

    if (value < 0.0)
    {
        *sign = 1;
    }
    
    if (value == 0.0)
    {
        for (int j = 0; j < count; j++)
        {
            lpStartOfReturnBuffer[j] = '0';
        }
        lpStartOfReturnBuffer[count] = '\0';
        return lpStartOfReturnBuffer;
    } 
    
    int tempBufferLength = snprintf(TempBuffer, ECVT_MAX_BUFFER_SIZE, "%.40e", value);
    _ASSERTE(tempBufferLength > 0 && ECVT_MAX_BUFFER_SIZE > tempBufferLength);
    
    //
    // Calculate the exponent value
    //

    int exponentIndex = tempBufferLength - 1;
    while (TempBuffer[exponentIndex] != 'e' && exponentIndex > 0)
    {
        exponentIndex--;
    }

    _ASSERTE(exponentIndex > 0 && (exponentIndex < tempBufferLength - 1));

    int i = exponentIndex + 1;
    int exponentSign = 1;
    if (TempBuffer[i] == '-')
    {
        exponentSign = -1;
        i++;
    }
    else if (TempBuffer[i] == '+')
    {
        i++;
    }

    int exponentValue = 0;
    while (i < tempBufferLength)
    {
        _ASSERTE(TempBuffer[i] >= '0' && TempBuffer[i] <= '9');
        exponentValue = exponentValue * 10 + ((BYTE) TempBuffer[i] - (BYTE) '0');
        i++;
    }
    exponentValue *= exponentSign;
    
    //
    // Determine decimal location.
    // 

    if (exponentValue == 0)
    {
        *dec = 1;
    }
    else
    {
        *dec = exponentValue + 1;
    }
    
    //
    // Copy the string from the temp buffer upto precision characters, removing the sign, and decimal as required.
    // 

    i = 0;
    int mantissaIndex = 0;
    while (i < count && mantissaIndex < exponentIndex)
    {
        if (TempBuffer[mantissaIndex] >= '0' && TempBuffer[mantissaIndex] <= '9')
        {
            lpStartOfReturnBuffer[i] = TempBuffer[mantissaIndex];
            i++;
        }
        mantissaIndex++;
    }

    while (i < count)
    {
        lpStartOfReturnBuffer[i] = '0'; // append zeros as needed
        i++;
    }

    lpStartOfReturnBuffer[i] = '\0';
    
    //
    // Round if needed
    //

    if (mantissaIndex >= exponentIndex || lpStartOfReturnBuffer[mantissaIndex] < '5')
    {
        return lpStartOfReturnBuffer; // rounding is not needed
    }

    i = count - 1;
    while (lpStartOfReturnBuffer[i] == '9' && i > 0)
    {
        lpStartOfReturnBuffer[i] = '0';
        i--;
    }

    if (i == 0 && lpStartOfReturnBuffer[i] == '9')
    {
        lpStartOfReturnBuffer[i] = '1';
        (*dec)++;
    }
    else
    {
        lpStartOfReturnBuffer[i]++;
    }    
    
    return lpStartOfReturnBuffer;
#endif // __APPLE__
}


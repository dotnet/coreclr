// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*++



Module Name:

    miscpalapi.c

Abstract:

    Implementation misc PAL APIs

Revision History:



--*/

#include "pal/palinternal.h"
#include "pal/dbgmsg.h"
#include "pal/file.h"
#include "pal/process.h"
#include "pal/module.h"
#include "pal/malloc.hpp"
#include "pal/stackstring.hpp"

#include <errno.h>
#include <unistd.h>
#include <time.h>
#include <pthread.h>
#include <dlfcn.h>

#include <pal_endian.h>

#ifdef __APPLE__
#include <mach-o/dyld.h>
#endif // __APPLE__

#if HAVE_SYS_GETRANDOM
#include <sys/syscall.h>

#ifndef GRND_NONBLOCK
#define GRND_NONBLOCK 0x01
#endif // GRND_NONBLOCK
#endif // HAVE_SYS_GETRANDOM

SET_DEFAULT_DEBUG_CHANNEL(MISC);

static const char URANDOM_DEVICE_NAME[]="/dev/urandom";

/*++

Function :

    PAL_GetPALDirectoryW

    Returns the fully qualified path name
    where the PALL DLL was loaded from.

    On failure it returns FALSE and sets the
    proper LastError code.

--*/
BOOL
PAL_GetPALDirectoryW(PathWCharString& lpDirectoryName)
{
    LPCWSTR lpFullPathAndName = NULL;
    LPCWSTR lpEndPoint = NULL;
    BOOL bRet = FALSE;

    PERF_ENTRY(PAL_GetPALDirectoryW);

    MODSTRUCT *module = LOADGetPalLibrary();
    if (!module)
    {
        SetLastError(ERROR_INTERNAL_ERROR);
        goto EXIT;
    }
    lpFullPathAndName = module->lib_name;
    if (lpFullPathAndName == NULL)
    {
        SetLastError(ERROR_INTERNAL_ERROR);
        goto EXIT;
    }
    lpEndPoint = PAL_wcsrchr( lpFullPathAndName, '/' );
    if ( lpEndPoint )
    {
        /* The path that we return is required to have
           the trailing slash on the end.*/
        lpEndPoint++;


        if(!lpDirectoryName.Set(lpFullPathAndName,lpEndPoint - lpFullPathAndName))
        {
            ASSERT( "The buffer was not large enough.\n" );
            SetLastError( ERROR_INSUFFICIENT_BUFFER );
            goto EXIT;
        }

        bRet = TRUE;
    }
    else
    {
        ASSERT( "Unable to determine the path.\n" );
        /* Error path, should not be executed. */
        SetLastError( ERROR_INTERNAL_ERROR );
    }

EXIT:
    PERF_EXIT(PAL_GetPALDirectoryW);
    return bRet;
}

BOOL
PAL_GetPALDirectoryA(PathCharString& lpDirectoryName)
{
    BOOL bRet;
    PathWCharString directory;

    PERF_ENTRY(PAL_GetPALDirectoryA);

    bRet = PAL_GetPALDirectoryW(directory);

    if (bRet)
    {

        int length = WideCharToMultiByte(CP_ACP, 0, directory.GetString(), -1, NULL, 0, NULL, 0);
        LPSTR DirectoryName = lpDirectoryName.OpenStringBuffer(length);
        if (NULL == DirectoryName)
        {
            SetLastError( ERROR_INSUFFICIENT_BUFFER );
            bRet = FALSE;
        }

        length = WideCharToMultiByte(CP_ACP, 0, directory.GetString(), -1, DirectoryName, length, NULL, 0);

        if (0 == length)
        {
            bRet = FALSE;
            length++;
        }

        lpDirectoryName.CloseBuffer(length - 1);
    }

    PERF_EXIT(PAL_GetPALDirectoryA);
    return bRet;
}

/*++

Function :

    PAL_GetPALDirectoryW

    Returns the fully qualified path name
    where the PALL DLL was loaded from.

    On failure it returns FALSE and sets the
    proper LastError code.

--*/
PALIMPORT
BOOL
PALAPI
PAL_GetPALDirectoryW( OUT LPWSTR lpDirectoryName, IN OUT UINT* cchDirectoryName )
{
    PathWCharString directory;
    BOOL bRet;
    PERF_ENTRY(PAL_GetPALDirectoryW);
    ENTRY( "PAL_GetPALDirectoryW( %p, %d )\n", lpDirectoryName, *cchDirectoryName );

    bRet = PAL_GetPALDirectoryW(directory);

    if (bRet) {

        if (directory.GetCount() > *cchDirectoryName)
        {
            SetLastError( ERROR_INSUFFICIENT_BUFFER );
            bRet = FALSE;
        }
        else
        {
            PAL_wcscpy(lpDirectoryName, directory.GetString());
        }

        *cchDirectoryName = directory.GetCount();
    }

    LOGEXIT( "PAL_GetPALDirectoryW returns BOOL %d.\n", bRet);
    PERF_EXIT(PAL_GetPALDirectoryW);
    return bRet;

}

PALIMPORT
BOOL
PALAPI
PAL_GetPALDirectoryA(
             OUT LPSTR lpDirectoryName,
             IN UINT*  cchDirectoryName)
{
    BOOL bRet;
    PathCharString directory;

    PERF_ENTRY(PAL_GetPALDirectoryA);
    ENTRY( "PAL_GetPALDirectoryA( %p, %d )\n", lpDirectoryName, *cchDirectoryName );

    bRet = PAL_GetPALDirectoryA(directory);

    if (bRet)
    {
        if (directory.GetCount() > *cchDirectoryName)
        {
            SetLastError( ERROR_INSUFFICIENT_BUFFER );
            bRet = FALSE;
            *cchDirectoryName = directory.GetCount();
        }
        else if (strcpy_s(lpDirectoryName, directory.GetCount(), directory.GetString()) == SAFECRT_SUCCESS)
        {
        }
        else
        {
            bRet = FALSE;
        }
    }

    LOGEXIT( "PAL_GetPALDirectoryA returns BOOL %d.\n", bRet);
    PERF_EXIT(PAL_GetPALDirectoryA);
    return bRet;
}

static
BOOL
PAL_RandomFromUrandom(
        IN OUT LPVOID lpBuffer,
        IN DWORD dwLength)
{
    static BOOL sMissingDevURandom;
    int rand_des;

    if (sMissingDevURandom)
    {
        return FALSE;
    }

    do
    {
        rand_des = open(URANDOM_DEVICE_NAME, O_RDONLY | O_CLOEXEC);
    } while ((rand_des == -1) && (errno == EINTR));

    if (rand_des == -1)
    {
        if (errno == ENOENT)
        {
            sMissingDevURandom = TRUE;
        }
        else
        {
            ASSERT("PAL__open() failed, errno:%d (%s)\n", errno, strerror(errno));
        }

        return FALSE;
    }

    DWORD offset = 0;
    do
    {
        ssize_t n = read(rand_des, (BYTE*)lpBuffer + offset , dwLength - offset);
        if (n == -1)
        {
            if (errno == EINTR)
            {
                continue;
            }
            ASSERT("read() failed, errno:%d (%s)\n", errno, strerror(errno));

            break;
        }

        offset += n;
    }
    while (offset != dwLength);

    _ASSERTE(offset == dwLength);

    close(rand_des);

    return TRUE;
}

#if HAVE_SYS_GETRANDOM
static
BOOL
PAL_RandomFromGetRandom(
        IN OUT LPVOID lpBuffer,
        IN DWORD dwLength)
{
    ssize_t length = (ssize_t)dwLength;
    static BOOL sMissingGetRandom;

    if (sMissingGetRandom)
    {
        return FALSE;
    }

    do
    {
        ssize_t res = syscall(SYS_getrandom, lpBuffer, length,
                              GRND_NONBLOCK);
        if (res == -1)
        {
            if (errno == ENOSYS)
            {
                sMissingGetRandom = TRUE;
                return FALSE;
            }
            else if (errno == EINTR)
            {
                continue;
            }
            else
            {
                return FALSE;
            }
        }

        length -= res;
        lpBuffer = (char *)lpBuffer + res;
    }
    while (length);

    return TRUE;
}
#endif // HAVE_SYS_GETRANDOM

VOID
PALAPI
PAL_Random(
        IN OUT LPVOID lpBuffer,
        IN DWORD dwLength)
{
    static BOOL sInitializedMRand;
    BOOL needSrand = TRUE;

    PERF_ENTRY(PAL_Random);
    ENTRY("PAL_Random(lpBuffer=%p, dwLength=%d)\n", lpBuffer, dwLength);

#if HAVE_SYS_GETRANDOM
    if (PAL_RandomFromGetRandom(lpBuffer, dwLength))
    {
        needSrand = FALSE;
    }
    else
#endif // HAVE_SYS_GETRANDOM
    if (PAL_RandomFromUrandom(lpBuffer, dwLength))
    {
        needSrand = FALSE;
    }

    if (needSrand)
    {
        long num;

        if (!sInitializedMRand)
        {
            // FIXME: There's not enough entropy in time(NULL) to
            // initialize the PRNG.  Might be a good idea to leverage
            // other sources of entropy at this point.
            srand48(time(NULL));
            sInitializedMRand = TRUE;
        }

        for (DWORD i = 0; i < dwLength; i++)
        {
            if (i % sizeof(long) == 0) {
                num = mrand48();
            }

            *(((BYTE*)lpBuffer) + i) ^= num;
            num >>= 8;
        }
    }

    LOGEXIT("PAL_Random\n");
    PERF_EXIT(PAL_Random);
}

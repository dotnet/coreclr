// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: ArrayNative.cpp
//

//
// This file contains the native methods that support the Array class
//

#ifndef _ARRAYNATIVE_INL_
#define _ARRAYNATIVE_INL_

#include "gchelpers.inl"

FORCEINLINE void InlinedForwardGCSafeCopyHelper(void *dest, const void *src, size_t len)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(CheckPointer(dest));
        PRECONDITION(CheckPointer(src));
        SO_TOLERANT;
    }
    CONTRACTL_END;

    _ASSERTE(dest != nullptr);
    _ASSERTE(src != nullptr);
    _ASSERTE(dest != src);
    _ASSERTE(len != 0);

    // To be able to copy forwards, the destination buffer cannot start inside the source buffer
    _ASSERTE((SIZE_T)dest - (SIZE_T)src >= len);

    // Make sure everything is pointer aligned
    _ASSERTE(IS_ALIGNED(dest, sizeof(SIZE_T)));
    _ASSERTE(IS_ALIGNED(src, sizeof(SIZE_T)));
    _ASSERTE(IS_ALIGNED(len, sizeof(SIZE_T)));

    _ASSERTE(CheckPointer(dest));
    _ASSERTE(CheckPointer(src));

    SIZE_T *dptr = (SIZE_T *)dest;
    SIZE_T *sptr = (SIZE_T *)src;

    if ((len & sizeof(SIZE_T)) != 0)
    {
        *dptr = *sptr;
        len ^= sizeof(SIZE_T);
        if (len == 0)
        {
            return;
        }
        ++sptr;
        ++dptr;
    }

    if ((len & (2 * sizeof(SIZE_T))) != 0)
    {
#if defined(_AMD64_) && !defined(FEATURE_PAL) // TODO: Enable on Unix
        __m128 v = _mm_loadu_ps((float *)sptr);
        _mm_storeu_ps((float *)dptr, v);
#else // !_AMD64_ || FEATURE_PAL
        // Read two values and write two values to hint the use of wide loads and stores
        SIZE_T p[2];
        p[0] = sptr[0];
        p[1] = sptr[1];
        dptr[0] = p[0];
        dptr[1] = p[1];
#endif // _AMD64_ && !FEATURE_PAL

        len ^= 2 * sizeof(SIZE_T);
        if (len == 0)
        {
            return;
        }
        sptr += 2;
        dptr += 2;
    }

    // copy 16 (on 32-bit systems) or 32 (on 64-bit systems) bytes at a time
    while (true)
    {
#if defined(_AMD64_) && !defined(FEATURE_PAL) // TODO: Enable on Unix
        __m128 v = _mm_loadu_ps((float *)sptr);
        _mm_storeu_ps((float *)dptr, v);
        v = _mm_loadu_ps((float *)(sptr + 2));
        _mm_storeu_ps((float *)(dptr + 2), v);
#else // !_AMD64_ || FEATURE_PAL
        // Read two values and write two values to hint the use of wide loads and stores
        SIZE_T p[2];
        p[0] = sptr[0];
        p[1] = sptr[1];
        dptr[0] = p[0];
        dptr[1] = p[1];
        p[0] = sptr[2];
        p[1] = sptr[3];
        dptr[2] = p[0];
        dptr[3] = p[1];
#endif // _AMD64_ && !FEATURE_PAL

        len -= 4 * sizeof(SIZE_T);
        if (len == 0)
        {
            return;
        }
        sptr += 4;
        dptr += 4;
    }
}

FORCEINLINE void InlinedBackwardGCSafeCopyHelper(void *dest, const void *src, size_t len)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        SO_TOLERANT;
    }
    CONTRACTL_END;

    _ASSERTE(dest != nullptr);
    _ASSERTE(src != nullptr);
    _ASSERTE(dest != src);
    _ASSERTE(len != 0);

    // To be able to copy backwards, the source buffer cannot start inside the destination buffer
    _ASSERTE((SIZE_T)src - (SIZE_T)dest >= len);

    // Make sure everything is pointer aligned
    _ASSERTE(IS_ALIGNED(dest, sizeof(SIZE_T)));
    _ASSERTE(IS_ALIGNED(src, sizeof(SIZE_T)));
    _ASSERTE(IS_ALIGNED(len, sizeof(SIZE_T)));

    _ASSERTE(CheckPointer(dest));
    _ASSERTE(CheckPointer(src));

    SIZE_T *dptr = (SIZE_T *)((BYTE *)dest + len);
    SIZE_T *sptr = (SIZE_T *)((BYTE *)src + len);

    if ((len & sizeof(SIZE_T)) != 0)
    {
        --sptr;
        --dptr;
        *dptr = *sptr;
        len ^= sizeof(SIZE_T);
        if (len == 0)
        {
            return;
        }
    }

    if ((len & (2 * sizeof(SIZE_T))) != 0)
    {
        sptr -= 2;
        dptr -= 2;

#if defined(_AMD64_) && !defined(FEATURE_PAL) // TODO: Enable on Unix
        __m128 v = _mm_loadu_ps((float *)sptr);
        _mm_storeu_ps((float *)dptr, v);
#else // !_AMD64_ || FEATURE_PAL
        // Read two values and write two values to hint the use of wide loads and stores
        SIZE_T p[2];
        p[1] = sptr[1];
        p[0] = sptr[0];
        dptr[1] = p[1];
        dptr[0] = p[0];
#endif // _AMD64_ && !FEATURE_PAL

        len ^= 2 * sizeof(SIZE_T);
        if (len == 0)
        {
            return;
        }
    }

    // copy 16 (on 32-bit systems) or 32 (on 64-bit systems) bytes at a time
    do
    {
        sptr -= 4;
        dptr -= 4;

#if defined(_AMD64_) && !defined(FEATURE_PAL) // TODO: Enable on Unix
        __m128 v = _mm_loadu_ps((float *)(sptr + 2));
        _mm_storeu_ps((float *)(dptr + 2), v);
        v = _mm_loadu_ps((float *)sptr);
        _mm_storeu_ps((float *)dptr, v);
#else // !_AMD64_ || FEATURE_PAL
        // Read two values and write two values to hint the use of wide loads and stores
        SIZE_T p[2];
        p[0] = sptr[2];
        p[1] = sptr[3];
        dptr[2] = p[0];
        dptr[3] = p[1];
        p[0] = sptr[0];
        p[1] = sptr[1];
        dptr[0] = p[0];
        dptr[1] = p[1];
#endif // _AMD64_ && !FEATURE_PAL

        len -= 4 * sizeof(SIZE_T);
    } while (len != 0);
}

FORCEINLINE void InlinedMemmoveGCRefsHelper(void *dest, const void *src, size_t len)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        SO_TOLERANT;
    }
    CONTRACTL_END;

    _ASSERTE(dest != nullptr);
    _ASSERTE(src != nullptr);
    _ASSERTE(dest != src);
    _ASSERTE(len != 0);

    // Make sure everything is pointer aligned
    _ASSERTE(IS_ALIGNED(dest, sizeof(SIZE_T)));
    _ASSERTE(IS_ALIGNED(src, sizeof(SIZE_T)));
    _ASSERTE(IS_ALIGNED(len, sizeof(SIZE_T)));

    _ASSERTE(CheckPointer(dest));
    _ASSERTE(CheckPointer(src));

    GCHeapMemoryBarrier();

    // To be able to copy forwards, the destination buffer cannot start inside the source buffer
    if ((size_t)dest - (size_t)src >= len)
    {
        InlinedForwardGCSafeCopyHelper(dest, src, len);
    }
    else
    {
        InlinedBackwardGCSafeCopyHelper(dest, src, len);
    }

    InlinedSetCardsAfterBulkCopyHelper((Object**)dest, len);
}

#endif // !_ARRAYNATIVE_INL_

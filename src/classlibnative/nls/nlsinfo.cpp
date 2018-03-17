// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
////////////////////////////////////////////////////////////////////////////
//
//  Class:    NLSInfo
//

//
//  Purpose:  This module implements the methods of the COMNlsInfo
//            class.  These methods are the helper functions for the
//            Locale class.
//
//  Date:     August 12, 1998
//
////////////////////////////////////////////////////////////////////////////

//
//  Include Files.
//
#include "common.h"
#include "object.h"
#include "excep.h"
#include "vars.hpp"
#include "interoputil.h"
#include "corhost.h"

#include <winnls.h>

#include "utilcode.h"
#include "frames.h"
#include "field.h"
#include "metasig.h"
#include "nls.h"
#include "nlsinfo.h"

#include "newapis.h"

//
//  Constant Declarations.
//

// TODO: NLS Arrowhead -Be nice if we could depend more on the OS for this
// Language ID for CHT (Taiwan)
#define LANGID_ZH_TW            0x0404
// Language ID for CHT (Hong-Kong)
#define LANGID_ZH_HK            0x0c04


INT32 COMNlsInfo::CallGetUserDefaultUILanguage()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
        SO_TOLERANT;
    } CONTRACTL_END;

    static INT32 s_lcid = 0;

    // The user UI language cannot change within a process (in fact it cannot change within a logon session),
    // so we cache it. We dont take a lock while initializing s_lcid for the same reason. If two threads are
    // racing to initialize s_lcid, the worst thing that'll happen is that one thread will call
    // GetUserDefaultUILanguage needlessly, but the final result is going to be the same.
    if (s_lcid == 0)
    {
        INT32 s_lcidTemp = GetUserDefaultUILanguage();
        if (s_lcidTemp == LANGID_ZH_TW)
        {
            // If the UI language ID is 0x0404, we need to do extra check to decide
            // the real UI language, since MUI (in CHT)/HK/TW Windows SKU all uses 0x0404 as their CHT language ID.
            if (!NewApis::IsZhTwSku())
            {
                s_lcidTemp = LANGID_ZH_HK;
            }
        }
        s_lcid = s_lcidTemp;
    }

    return s_lcid;
}

/**
 * This function returns a pointer to this table that we use in System.Globalization.EncodingTable.
 * No error checking of any sort is performed.  Range checking is entirely the responsibility of the managed
 * code.
 */
FCIMPL0(EncodingDataItem *, COMNlsInfo::nativeGetEncodingTableDataPointer)
{
    LIMITED_METHOD_CONTRACT;
    STATIC_CONTRACT_SO_TOLERANT;

    return (EncodingDataItem *)EncodingDataTable;
}
FCIMPLEND

/**
 * This function returns a pointer to this table that we use in System.Globalization.EncodingTable.
 * No error checking of any sort is performed.  Range checking is entirely the responsibility of the managed
 * code.
 */
FCIMPL0(CodePageDataItem *, COMNlsInfo::nativeGetCodePageTableDataPointer)
{
    LIMITED_METHOD_CONTRACT;

    STATIC_CONTRACT_SO_TOLERANT;

    return ((CodePageDataItem*) CodePageDataTable);
}
FCIMPLEND

/**
 * This function returns the number of items in EncodingDataTable.
 */
FCIMPL0(INT32, COMNlsInfo::nativeGetNumEncodingItems)
{
    LIMITED_METHOD_CONTRACT;
    STATIC_CONTRACT_SO_TOLERANT;

    return (m_nEncodingDataTableItems);
}
FCIMPLEND

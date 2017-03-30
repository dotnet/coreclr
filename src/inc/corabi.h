// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*****************************************************************************\
*                                                                             *
* corabi.h -    EE / JIT Commons to Sync                                      *
*                                                                             *
\*****************************************************************************/

#ifndef _CORABI_H_
#define _CORABI_H_

#ifdef UNIX_X86_ABI

//#define UNIX_X86_ABI_FOA
// UNIX_X86_ABI_FOA:
//      This will set FEAURE_FIXED_OUT_ARGS to 1 or else to 0 for JIT and
//      tell unwinder not to use padding added for 16 byte stack alignment.
//      We may remove 'UNIX_X86_ABI_FOA' and use 'UNIX_X86_ABI' when
//      UNIX_X86_ABI and FEATURE_FIXED_OUT_ARGS are stable.

#endif // UNIX_X86_ABI

#endif // _CORABI_H_

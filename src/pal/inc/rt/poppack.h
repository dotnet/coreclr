// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

//
// ===========================================================================
// File:  poppack.h
// 
// ===========================================================================
/*
Abstract:

    This file turns packing of structures off.  (That is, it enables
    automatic alignment of structure fields.)  An include file is needed
    because various compilers do this in different ways.

    poppack.h is the complement to pshpack?.h.  An inclusion of poppack.h
    MUST ALWAYS be preceded by an inclusion of one of pshpack?.h, in one-to-one
    correspondence.

    For Microsoft compatible compilers, this file uses the pop option
    to the pack pragma so that it can restore the previous saved by the
    pshpack?.h include file.

*/

#if ! (defined(lint) || defined(RC_INVOKED))
#include "poppack_impl.h"
#pragma clang diagnostic pop
#endif // ! (defined(lint) || defined(RC_INVOKED))

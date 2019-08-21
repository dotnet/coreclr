// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef TAILCALL_HELP_H
#define TAILCALL_HELP_H

class TailCallHelp
{
private:
    static void CreateStoreArgsStubSig(MethodDesc* pCalleeMD, MetaSig& callSiteSig, SigBuilder* sig);
public:
    static MethodDesc* CreateStoreArgsStub(MethodDesc* pCallerMD,
                                           MethodDesc* pCalleeMD,
                                           MetaSig& callSiteSig);
};

#endif
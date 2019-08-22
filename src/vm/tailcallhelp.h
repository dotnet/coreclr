// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef TAILCALL_HELP_H
#define TAILCALL_HELP_H

#include "fcall.h"

struct TailCallInfo;
struct ArgBufferOrigArg;
struct ArgBufferLayout;
struct NewTailCallFrame; // todo; drop new prefix

class TailCallHelp
{
private:
    static bool GenerateGCDescriptor(const SArray<ArgBufferOrigArg>& args, GCRefMapBuilder* builder);
    static TypeHandle NormalizeSigType(TypeHandle tyHnd);
    static void LayOutArgBuffer(MetaSig& callSiteSig, ArgBufferLayout* layout);
    static void AppendConvertedSigType(const MetaSig& msig, SigPointer sigType, SigBuilder* sig);
    static void CreateStoreArgsStubSig(const TailCallInfo& layout, SigBuilder* sig);
    static void CreateCallTargetStubSig(const TailCallInfo& info, SigBuilder* sig);
    static MethodDesc* CreateCallTargetStub(const TailCallInfo& info);
public:
    static MethodDesc* CreateStoreArgsStub(MethodDesc* pCallerMD,
                                           MethodDesc* pCalleeMD,
                                           MetaSig& callSiteSig);

    static FCDECL1(void*, AllocTailCallArgBuffer, INT32);
    static FCDECL0(void*, GetTailCallArgBuffer);
    static FCDECL0(void,  FreeTailCallArgBuffer);
    static FCDECL1(void,  PushNewTailCallFrame, NewTailCallFrame*);
    static FCDECL0(void,  PopTailCallFrame);
};

#endif
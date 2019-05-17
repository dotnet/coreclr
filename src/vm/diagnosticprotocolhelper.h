// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __DIAGNOSTIC_PROTOCOL_HELPER_H__
#define __DIAGNOSTIC_PROTOCOL_HELPER_H__

#ifdef FEATURE_PERFTRACING

#include "common.h"
#include <diagnosticsprotocol.h>


class IpcStream;

// The Miscellaneous command set is 0x01
enum class MiscellaneousCommandId : uint8_t
{
    OK = 0x00,
    GenerateCoreDump = 0x01,
    // future

    Error =0xFF,
};

const DiagnosticsIpc::IpcHeader MiscellaneousErrorHeader =
{
    DiagnosticsIpc::DotnetIpcMagic_V1,
    (uint16_t)sizeof(DiagnosticsIpc::IpcHeader),
    (uint8_t)DiagnosticsIpc::DiagnosticServerCommandSet::Miscellaneous,
    (uint8_t)MiscellaneousCommandId::Error,
    (uint16_t)0x0000
};

const DiagnosticsIpc::IpcHeader MiscellaneousSuccessHeader =
{
    DiagnosticsIpc::DotnetIpcMagic_V1,
    (uint16_t)sizeof(DiagnosticsIpc::IpcHeader),
    (uint8_t)DiagnosticsIpc::DiagnosticServerCommandSet::Miscellaneous,
    (uint8_t)MiscellaneousCommandId::OK,
    (uint16_t)0x0000
};

struct GenerateCoreDumpCommandPayload
{
    // The protocol buffer is defined as:
    //   string - dumpName (UTF16)
    //   int - dumpType
    //   int - diagnostics
    // returns
    //   ulong - status
    LPCWSTR dumpName;
    INT dumpType;
    INT diagnostics;
    static const GenerateCoreDumpCommandPayload* TryParse(BYTE* lpBuffer, uint16_t& BufferSize);
};

class DiagnosticProtocolHelper
{
public:
    // IPC event handlers.
#ifdef FEATURE_PAL
    static void GenerateCoreDump(DiagnosticsIpc::IpcMessage& message, IpcStream *pStream); // `dotnet-dump collect`
    static void HandleIpcMessage(DiagnosticsIpc::IpcMessage& message, IpcStream* pStream);
#endif

#ifdef FEATURE_PROFAPI_ATTACH_DETACH
    static void AttachProfiler(IpcStream *pStream);
#endif // FEATURE_PROFAPI_ATTACH_DETACH

private:
    const static uint32_t IpcStreamReadBufferSize = 8192;
};

#endif // FEATURE_PERFTRACING

#endif // __DIAGNOSTIC_PROTOCOL_HELPER_H__

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if defined(FEATURE_PROFAPI_ATTACH_DETACH) && !defined(DACCESS_COMPILE)
#include "common.h"
#include "fastserializer.h"
#include "profilerdiagnosticprotocolhelper.h"
#include "diagnosticsipc.h"
#include "diagnosticsprotocol.h"
#include "profilinghelper.h"
#include "profilinghelper.inl"

void ProfilerDiagnosticProtocolHelper::HandleIpcMessage(DiagnosticsIpc::IpcMessage& message, IpcStream* pStream)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
        PRECONDITION(pStream != nullptr);
    }
    CONTRACTL_END;

    switch ((ProfilferCommandId)message.GetHeader().CommandId)
    {
    case ProfilferCommandId::AttachProfiler:
        ProfilferDiagnosticProtocolHelper::AttachProfiler(message, pStream);
        break;

    default:
        STRESS_LOG1(LF_DIAGNOSTICS_PORT, LL_WARNING, "Received unknown request type (%d)\n", message.GetHeader().CommandSet);
        DiagnosticsIpc::IpcMessage::SendErrorMessage(pStream, DiagnosticsIpc::DiagnosticServerErrorCode::UnknownCommand);
        delete pStream;
        break;
    }
}

const ProfilerAttachCommandPayload* ProfilerAttachCommandPayload::TryParse(BYTE* lpBuffer, uint16_t& BufferSize)
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
        PRECONDITION(lpBuffer != nullptr);
    }
    CONTRACTL_END;

    ProfilerAttachCommandPayload* payload = new (nothrow) ProfilerAttachCommandPayload;
    if (payload == nullptr)
    {
        // OOM
        return nullptr;
    }

    payload->incomingBuffer = lpBuffer;
    uint8_t* pBufferCursor = payload->incomingBuffer;
    uint32_t bufferLen = BufferSize;
    if (!::TryParse(pBufferCursor, bufferLen, payload->dwAttachTimeout) ||
        !::TryParse(pBufferCursor, bufferLen, payload->profilerGuid) ||
        !TryParseString(pBufferCursor, bufferLen, payload->pwszProfilerPath) ||
        !::TryParse(pBufferCursor, bufferLen, payload->cbClientData) ||
        !(bufferLen < payload->cbClientData))
    {
        delete payload;
        return nullptr;
    }

    payload->pClientData = pBufferCursor;

    return payload;
}

void ProfilerDiagnosticProtocolHelper::AttachProfiler(DiagnosticsIpc::IpcMessage& message, IpcStream *pStream)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
        PRECONDITION(pStream != nullptr);
    }
    CONTRACTL_END;

    if (pStream == nullptr)
    {
        return;
    }

    HRESULT hr = S_OK;
    NewHolder<const AttachProfilerCommandPayload> payload = message.TryParsePayload<AttachProfilerCommandPayload>();
    if (payload == nullptr)
    {
        DiagnosticsIpc::IpcMessage::SendErrorMessage(pStream, DiagnosticsIpc::DiagnosticServerErrorCode::BadEncoding);
        delete pStream;
        return;
    }

    if (!g_profControlBlock.fProfControlBlockInitialized)
    {
        hr = CORPROF_E_RUNTIME_UNINITIALIZED;
        goto ErrExit;
    }

    hr = ProfilingAPIUtility::LoadProfilerForAttach(&payload->profilerGuid,
                                                    payload->pwszProfilerPath,
                                                    payload->pClientData,
                                                    payload->cbClientData,
                                                    payload->dwAttachTimeout);
ErrExit:
    DiagnosticsIpc::IpcMessage profilerAttachResponse;
    DiagnosticsIpc::IpcHeader header = hr == S_OK ? DiagnosticsIpc::GenericSuccessHeader : DiagnosticsIpc::GenericErrorHeader;
    if (profilerAttachResponse.Initialize(header, (uint32_t)hr))
        profilerAttachResponse.Send(pStream)
    delete pStream;
}

#endif // defined(FEATURE_PROFAPI_ATTACH_DETACH) && !defined(DACCESS_COMPILE)
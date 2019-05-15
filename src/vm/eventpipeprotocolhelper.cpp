// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "fastserializer.h"
#include "eventpipefile.h"
#include "eventpipeprotocolhelper.h"
#include "eventpipesession.h"
#include "diagnosticsipc.h"
#include "diagnosticsprotocol.h"

#ifdef FEATURE_PERFTRACING

static bool IsNullOrWhiteSpace(LPCWSTR value)
{
    if (value == nullptr)
        return true;

    while (*value)
    {
        if (!iswspace(*value))
            return false;
        ++value;
    }
    return true;
}

constexpr DiagnosticsIpc::IpcHeader EventPipeSuccessHeader =
{
    DOTNET_IPC_V1_MAGIC,
    (uint16_t)20,
    (uint8_t)DiagnosticsIpc::DiagnosticServerCommandSet::EventPipe,
    (uint8_t)EventPipeCommandId::OK,
    (uint16_t)0x0000
};

constexpr DiagnosticsIpc::IpcHeader EventPipeErrorHeader =
{
    DOTNET_IPC_V1_MAGIC,
    (uint16_t)20,
    (uint8_t)DiagnosticsIpc::DiagnosticServerCommandSet::EventPipe,
    (uint8_t)EventPipeCommandId::Error,
    (uint16_t)0x0000
};

static bool TryParseCircularBufferSize(uint8_t*& bufferCursor, uint32_t& bufferLen, uint32_t& circularBufferSizeInMB)
{
    const bool CanParse = TryParse(bufferCursor, bufferLen, circularBufferSizeInMB);
    return CanParse && (circularBufferSizeInMB > 0);
}

const EventPipeCollectTracingCommandPayload* EventPipeCollectTracingCommandPayload::TryParse(BYTE* lpBuffer, uint16_t& BufferSize)
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
        PRECONDITION(lpBuffer != nullptr);
    }
    CONTRACTL_END;

    EventPipeCollectTracingCommandPayload *payload = new (nothrow) EventPipeCollectTracingCommandPayload;
    uint8_t* pBufferCursor = lpBuffer;
    uint32_t bufferLen = BufferSize;
    if (!TryParseCircularBufferSize(pBufferCursor, bufferLen, payload->circularBufferSizeInMB) ||
        !TryParseString(pBufferCursor, bufferLen, payload->outputPath) ||
        !EventPipeProtocolHelper::TryParseProviderConfiguration(pBufferCursor, bufferLen, payload->providerConfigs))
        return nullptr;

    return payload;
}

void EventPipeProtocolHelper::HandleIpcMessage(DiagnosticsIpc::IpcMessage& message, IpcStream* pStream)
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(pStream != nullptr);
    }
    CONTRACTL_END;

    switch ((EventPipeCommandId)message.GetHeader().CommandId)
    {
    case EventPipeCommandId::CollectTracing:
        EventPipeProtocolHelper::CollectTracing(message, pStream);
        break;

    case EventPipeCommandId::StopTracing:
        EventPipeProtocolHelper::StopTracing(message, pStream);
        break;

    default:
        STRESS_LOG1(LF_DIAGNOSTICS_PORT, LL_WARNING, "Received unknown request type (%d)\n", message.GetHeader().CommandSet);
        DiagnosticsIpc::IpcMessage errorResponse(DiagnosticsIpc::DiagnosticServerErrorCode::UnknownCommandId);
        errorResponse.Send(pStream);
        delete pStream;
        break;
    }
}

bool EventPipeProtocolHelper::TryParseProviderConfiguration(uint8_t *&bufferCursor, uint32_t &bufferLen, CQuickArray<EventPipeProviderConfiguration> &result)
{
    // Picking an arbitrary upper bound,
    // This should be larger than any reasonable client request.
    const uint32_t MaxCountConfigs = 1000; // TODO: This might be too large.

    uint32_t countConfigs = 0;
    if (!TryParse(bufferCursor, bufferLen, countConfigs))
        return false;
    if (countConfigs > MaxCountConfigs)
        return false;
    EventPipeProviderConfiguration *pConfigs = result.AllocNoThrow(countConfigs);
    if (pConfigs == nullptr)
        return false;

    for (uint32_t i = 0; i < countConfigs; i++)
    {
        uint64_t keywords = 0;
        if (!TryParse(bufferCursor, bufferLen, keywords))
            return false;

        uint32_t logLevel = 0;
        if (!TryParse(bufferCursor, bufferLen, logLevel))
            return false;
        if (logLevel > 5) // (logLevel > EventPipeEventLevel::Verbose)
            return false;

        LPCWSTR pProviderName = nullptr;
        if (!TryParseString(bufferCursor, bufferLen, pProviderName))
            return false;
        if (IsNullOrWhiteSpace(pProviderName))
            return false;

        LPCWSTR pFilterData = nullptr; // This parameter is optional.
        TryParseString(bufferCursor, bufferLen, pFilterData);

        pConfigs[i] = EventPipeProviderConfiguration(pProviderName, keywords, logLevel, pFilterData);
    }
    return (countConfigs > 0);
}

void EventPipeProtocolHelper::StopTracing(DiagnosticsIpc::IpcMessage& message, IpcStream *pStream)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
        PRECONDITION(pStream != nullptr);
    }
    CONTRACTL_END;

    const EventPipeStopTracingCommandPayload* payload = message.TryParsePayload<EventPipeStopTracingCommandPayload>();
    if (payload == nullptr)
    {
        DiagnosticsIpc::IpcMessage errorMessage(EventPipeErrorHeader, DiagnosticsIpc::ServerErrorPayload{ DiagnosticsIpc::DiagnosticServerErrorCode::BadEncoding });
        errorMessage.Send(pStream);
        delete pStream;
        return;
    }

    EventPipe::Disable(payload->sessionId);

    DiagnosticsIpc::IpcMessage stopTracingResponse(EventPipeSuccessHeader, payload->sessionId);
    stopTracingResponse.Send(pStream);

    bool fSuccess = pStream->Flush();
    if (!fSuccess)
    {
        // TODO: Add error handling.
    }
    delete pStream;
}

void EventPipeProtocolHelper::CollectTracing(DiagnosticsIpc::IpcMessage& message, IpcStream *pStream)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_PREEMPTIVE;
        PRECONDITION(pStream != nullptr);
    }
    CONTRACTL_END;

    const EventPipeCollectTracingCommandPayload* payload = message.TryParsePayload<EventPipeCollectTracingCommandPayload>();
    if (payload == nullptr)
    {
        DiagnosticsIpc::IpcMessage errorResponse(EventPipeErrorHeader, DiagnosticsIpc::ServerErrorPayload{ DiagnosticsIpc::DiagnosticServerErrorCode::BadEncoding });
        errorResponse.Send(pStream);
        delete pStream;
        return;
    }

    auto sessionId = EventPipe::Enable(
        nullptr,                                        // strOutputPath (ignored in this scenario)
        payload->circularBufferSizeInMB,                         // circularBufferSizeInMB
        DefaultProfilerSamplingRateInNanoseconds,       // ProfilerSamplingRateInNanoseconds
        payload->providerConfigs.Ptr(),                          // pConfigs
        static_cast<uint32_t>(payload->providerConfigs.Size()),  // numConfigs
        EventPipeSessionType::IpcStream,                // EventPipeSessionType
        pStream);                                       // IpcStream

    if (sessionId == 0)
    {
        DiagnosticsIpc::IpcMessage errorMessage(EventPipeErrorHeader, DiagnosticsIpc::ServerErrorPayload{ DiagnosticsIpc::DiagnosticServerErrorCode::BAD });
        errorMessage.Send(pStream);
        delete pStream;
    }
}

#endif // FEATURE_PERFTRACING

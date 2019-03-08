// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "diagnosticserver.h"
#include "diagnosticsipc.h"
#include "eventpipe.h"
#include "eventpipeconfiguration.h"
#include "sampleprofiler.h"

#ifdef FEATURE_PAL
#include "pal.h"
#endif // FEATURE_PAL

#ifdef FEATURE_PERFTRACING

static DWORD WINAPI DiagnosticsServerThread(LPVOID lpThreadParameter)
{
    CONTRACTL
    {
        // TODO: Maybe this should not throw.
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(lpThreadParameter != nullptr);
    }
    CONTRACTL_END;

    auto pIpc = reinterpret_cast<IpcStream::DiagnosticsIpc *>(lpThreadParameter);
    if (pIpc == nullptr)
    {
        STRESS_LOG0(
            LF_STARTUP,                                     // facility
            LL_ERROR,                                       // level
            "Diagnostics IPC listener was undefined\n");    // msg
        return 1;
    }

    while (true)
    {
        IpcStream *pStream = pIpc->Accept();  // FIXME: Ideally this would be something like a std::shared_ptr
        assert(pStream != nullptr);
        if (pStream == nullptr)
            continue;

        // TODO: Read operation should happen in a loop.
        uint32_t nNumberOfBytesRead = 0;
        MessageHeader header;
        bool fSuccess = pStream->Read(&header, sizeof(header), nNumberOfBytesRead);
        if (!fSuccess || nNumberOfBytesRead != sizeof(header))
        {
            delete pStream;
            continue;
        }

        // TODO: Dispatch thread worker.
        switch (header.RequestType)
        {
        case DiagnosticMessageType::EnableEventPipe:
            EventPipe::EnableFileTracingEventHandler(pStream);
            break;

        case DiagnosticMessageType::DisableEventPipe:
            EventPipe::DisableTracingEventHandler(pStream);
            break;

        default:
            // TODO: Add error handling?
            break;
        }
    }

    return 0;
}

bool DiagnosticServer::Initialize()
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    bool fSuccess = false;

    EX_TRY
    {
        auto ErrorCallback = [](const char *szMessage, uint32_t code) {
            STRESS_LOG2(
                LF_STARTUP,                                           // facility
                LL_ERROR,                                             // level
                "Failed to create diagnostic IPC: error (%d): %s.\n", // msg
                code,                                                 // data1
                szMessage);                                           // data2
        };
        IpcStream::DiagnosticsIpc *pIpc = IpcStream::DiagnosticsIpc::Create(
            "dotnetcore-diagnostic", ::GetCurrentProcessId(), ErrorCallback);

        if (pIpc != nullptr)
        {
            DWORD dwThreadId = 0;
            HANDLE hThread = ::CreateThread( // TODO: Is it correct to have this "lower" level call here?
                nullptr,                     // no security attribute
                0,                           // default stack size
                DiagnosticsServerThread,     // thread proc
                (LPVOID)pIpc,                // thread parameter
                0,                           // not suspended
                &dwThreadId);                // returns thread ID

            if (hThread == nullptr)
            {
                // Failed to create IPC thread.
                STRESS_LOG1(
                    LF_STARTUP,                                          // facility
                    LL_ERROR,                                            // level
                    "Failed to create diagnostic server thread (%d).\n", // msg
                    ::GetLastError());                                   // data1
            }
            else
            {
                // FIXME: Maybe hold on to the thread to abort/cleanup at exit?
                ::CloseHandle(hThread);

                // TODO: Add error handling?
                fSuccess = true;
            }
        }
    }
    EX_CATCH {}
    EX_END_CATCH(SwallowAllExceptions);

    return fSuccess;
}

bool DiagnosticServer::Shutdown()
{
    CONTRACTL
    {
        NOTHROW;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    bool fSuccess = false;

    EX_TRY
    {
        // FIXME: Stop IPC server thread?
        fSuccess = true;
    }
    EX_CATCH
    {
        fSuccess = false;
    }
    EX_END_CATCH(SwallowAllExceptions);

    return fSuccess;
}

#endif // FEATURE_PERFTRACING

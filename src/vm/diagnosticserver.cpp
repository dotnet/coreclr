// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "diagnosticserver.h"
#include "diagnosticsipc.h"
#include "eventpipe.h"
#include "eventpipeconfiguration.h"
#include "processdescriptor.h"
#include "sampleprofiler.h"

#ifdef FEATURE_PAL
#include "pal.h"
#endif // FEATURE_PAL

#ifdef FEATURE_PERFTRACING

static DWORD WINAPI DiagnosticsServerThread(LPVOID /*lpThreadParameter*/)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    const ProcessDescriptor pd = ProcessDescriptor::FromCurrentProcess();
    IpcStream::DiagnosticsIpc ipc("dotnetcore-diagnostic", pd.m_Pid);

    while (true)
    {
        IpcStream *pStream = ipc.Accept();  // FIXME: Ideally this would be something like a std::shared_ptr
        assert(pStream != nullptr);
        if (pStream == nullptr)
        {
            continue;
        }

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
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    } CONTRACTL_END;

    static bool fInitialized = false;
    assert(!fInitialized);

    DWORD dwThreadId = 0;
    HANDLE hThread = ::CreateThread(
        nullptr,                    // no security attribute
        0,                          // default stack size
        DiagnosticsServerThread,    // thread proc
        nullptr,                    // thread parameter
        0,                          // not suspended
        &dwThreadId);               // returns thread ID

    if (hThread == nullptr)
    {
        // Failed to create IPC thread. Add error to STRESS_LOG.
        STRESS_LOG1(
            LF_STARTUP,                                             // facility
            LL_ERROR,                                               // level
            "Failed to create diagnostic server thread (%d).\n",    // msg
            ::GetLastError());                                      // data1
    }
    else
    {
         // FIXME: Maybe hold on to the thread to abort/cleanup atexit?
        ::CloseHandle(hThread);
        fInitialized = true;
    }

    return fInitialized;
}

bool DiagnosticServer::Shutdown()
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    } CONTRACTL_END;

    static bool fSuccess = false;
    assert(!fSuccess);

    // FIXME: Stop IPC server thread?

    fSuccess = true;
    return fSuccess;
}

#endif // FEATURE_PERFTRACING

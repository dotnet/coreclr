// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <assert.h>
#include <new>
#include <stdio.h>
#include "diagnosticsipc.h"

IpcStream::DiagnosticsIpc::DiagnosticsIpc(const char *const pIpcName, const uint32_t pid)
{
    assert(pIpcName != nullptr);

    memset(_pNamedPipeName, 0, sizeof(_pNamedPipeName));
    const int nCharactersWritten = sprintf_s(
        _pNamedPipeName,
        sizeof(_pNamedPipeName),
        "\\\\.\\pipe\\%s-%d",
        pIpcName,
        pid);
    assert(nCharactersWritten > 0);
}

IpcStream::DiagnosticsIpc::~DiagnosticsIpc()
{
}

IpcStream *IpcStream::DiagnosticsIpc::Accept() const
{
    const uint32_t nInBufferSize = 1024;
    const uint32_t nOutBufferSize = 16 * 1024 * 1024;
    HANDLE hPipe = ::CreateNamedPipeA(
        _pNamedPipeName,                                            // pipe name
        PIPE_ACCESS_DUPLEX/* | FILE_FLAG_OVERLAPPED*/,              // read/write access
        PIPE_TYPE_BYTE | PIPE_WAIT | PIPE_REJECT_REMOTE_CLIENTS,    // message type pipe, message-read and blocking mode
        PIPE_UNLIMITED_INSTANCES,                                   // max. instances
        nOutBufferSize,                                             // output buffer size
        nInBufferSize,                                              // input buffer size
        0,                                                          // default client time-out
        NULL);                                                      // default security attribute
    if (hPipe == INVALID_HANDLE_VALUE)
        return nullptr;
    return (::ConnectNamedPipe(hPipe, NULL) != 0) || (::GetLastError() == ERROR_PIPE_CONNECTED) ? new (std::nothrow) IpcStream(hPipe) : nullptr;
}

IpcStream::~IpcStream()
{
    if (_hPipe != INVALID_HANDLE_VALUE)
    {
        BOOL fSuccess = ::DisconnectNamedPipe(_hPipe);
        assert(fSuccess != 0);

        fSuccess = ::CloseHandle(_hPipe);
        assert(fSuccess != 0);
    }
}

bool IpcStream::Read(void *lpBuffer, const uint32_t nBytesToRead, uint32_t &nBytesRead) const
{
    assert(lpBuffer != nullptr);

    DWORD nNumberOfBytesRead = 0;
    const bool fSuccess = ::ReadFile(
        _hPipe,                 // handle to pipe
        lpBuffer,               // buffer to receive data
        nBytesToRead,           // size of buffer
        &nNumberOfBytesRead,    // number of bytes read
        NULL) != 0;             // not overlapped I/O

    if (!fSuccess)
    {
        // TODO: Add error handling.
    }

    nBytesRead = static_cast<std::remove_reference<decltype(nBytesRead)>::type>(nNumberOfBytesRead);
    return fSuccess;
}

bool IpcStream::Write(const void *lpBuffer, const uint32_t nBytesToWrite, uint32_t &nBytesWritten) const
{
    assert(lpBuffer != nullptr);

    DWORD nNumberOfBytesWritten = 0;
    const bool fSuccess = ::WriteFile(
        _hPipe,                 // handle to pipe
        lpBuffer,               // buffer to write from
        nBytesToWrite,          // number of bytes to write
        &nNumberOfBytesWritten, // number of bytes written
        NULL) != 0;             // not overlapped I/O

    if (!fSuccess)
    {
        // TODO: Add error handling.
    }

    nBytesWritten = static_cast<std::remove_reference<decltype(nBytesWritten)>::type>(nNumberOfBytesWritten);
    return fSuccess;
}

bool IpcStream::Flush() const
{
    const bool fSuccess = ::FlushFileBuffers(_hPipe) != 0;
    if (!fSuccess)
    {
        // TODO: Add error handling.
    }
    return fSuccess;
}

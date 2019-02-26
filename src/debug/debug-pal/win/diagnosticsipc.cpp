// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <assert.h>
#include <stdio.h>
#include <type_traits>
#include "diagnosticsipc.h"

#define _ASSERTE assert

DiagnosticsIpc::DiagnosticsIpc(const char *const pIpcName, const uint32_t pid)
{
    _ASSERTE(pIpcName != nullptr);

    memset(_pNamedPipeName, 0, sizeof(_pNamedPipeName));
    const int nCharactersWritten = sprintf_s(
        _pNamedPipeName,
        sizeof(_pNamedPipeName),
        "\\\\.\\pipe\\%s-%d",
        pIpcName,
        pid);
    _ASSERTE(nCharactersWritten > 0);
}

DiagnosticsIpc::~DiagnosticsIpc()
{
    if (IsValidStatus())
    {
        bool fSuccess = Close();
        if (!fSuccess)
        {
            // TODO: Add error handling.
        }
    }
}

bool DiagnosticsIpc::Open()
{
    const uint32_t BufferSize = 8192;
    _hPipe = ::CreateNamedPipeA(
        _pNamedPipeName,                                            // pipe name
        PIPE_ACCESS_DUPLEX,                                         // read/write access
        PIPE_TYPE_BYTE | PIPE_WAIT | PIPE_REJECT_REMOTE_CLIENTS,    // message type pipe, message-read and blocking mode
        PIPE_UNLIMITED_INSTANCES,                                   // max. instances
        BufferSize,                                                 // output buffer size
        BufferSize,                                                 // input buffer size
        0,                                                          // default client time-out
        NULL);                                                      // default security attribute

    return (_hPipe != INVALID_HANDLE_VALUE);
}

bool DiagnosticsIpc::Close()
{
    if (IsValidStatus())
    {
        HANDLE hPipe = _hPipe;
        _hPipe = INVALID_HANDLE_VALUE;

        BOOL fSuccess;
        fSuccess = ::DisconnectNamedPipe(hPipe);
        if (fSuccess == 0)
            return false;

        fSuccess = ::CloseHandle(hPipe);
        if (fSuccess == 0)
            return false;
        return true;
    }

    return false;
}

bool DiagnosticsIpc::IsValidStatus() const
{
    return _hPipe != INVALID_HANDLE_VALUE;
}

bool DiagnosticsIpc::Accept()
{
    const BOOL fSuccess = ::ConnectNamedPipe(_hPipe, NULL);
    return fSuccess != 0 ? true : (GetLastError() == ERROR_PIPE_CONNECTED);
}

bool DiagnosticsIpc::Read(void *lpBuffer, const uint32_t nBytesToRead, uint32_t &nBytesRead) const
{
    _ASSERTE(lpBuffer != nullptr);

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

    nBytesRead = fSuccess ? static_cast<std::remove_reference<decltype(nBytesRead)>::type>(nNumberOfBytesRead) : 0;
    return fSuccess;
}

bool DiagnosticsIpc::Write(const void *lpBuffer, const uint32_t nBytesToWrite, uint32_t &nBytesWritten) const
{
    _ASSERTE(lpBuffer != nullptr);

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

    nBytesWritten = fSuccess ? static_cast<std::remove_reference<decltype(nBytesWritten)>::type>(nNumberOfBytesWritten) : 0;
    return fSuccess;
}

bool DiagnosticsIpc::Flush() const
{
    const bool fSuccess = ::FlushFileBuffers(_hPipe) != 0;
    if (!fSuccess)
    {
        // TODO: Add error handling.
    }
    return fSuccess;
}

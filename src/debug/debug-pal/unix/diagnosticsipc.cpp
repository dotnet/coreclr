// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <pal.h>
#include <pal_assert.h>
#include "diagnosticsipc.h"
#include <new>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/un.h>
#include <type_traits>

DiagnosticsIpc::DiagnosticsIpc(const char *const pIpcName, const uint32_t pid)
    : _pServerAddress(new (std::nothrow) sockaddr_un)
{
    _ASSERTE(_pServerAddress != nullptr);
    _ASSERTE(pIpcName != nullptr);

    _serverSocket = ::socket(PF_UNIX, SOCK_STREAM, 0);
    _ASSERTE(_serverSocket != -1); // TODO: Add error handling.

    memset(_pServerAddress, 0, sizeof(sockaddr_un));
    _pServerAddress->sun_family = AF_UNIX;
    const int nCharactersWritten = sprintf_s(
        _pServerAddress->sun_path,
        sizeof(_pServerAddress->sun_path),
        "/tmp/%s-%d.socket",
        pIpcName,
        pid);
    _ASSERTE(nCharactersWritten > 0);

    const int fSuccessBind = ::bind(_serverSocket, (sockaddr *)_pServerAddress, sizeof(*_pServerAddress));
    _ASSERTE(fSuccessBind != -1);
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

        const int fSuccessClose = ::close(_serverSocket);
        _ASSERTE(fSuccessClose != -1); // TODO: Add error handling.

        const int fSuccessUnlink = ::unlink(_pServerAddress->sun_path);
        _ASSERTE(fSuccessUnlink != -1); // TODO: Add error handling.

        delete _pServerAddress;
    }
}

bool DiagnosticsIpc::Open()
{
    return true;
}

bool DiagnosticsIpc::Close()
{
    if (_clientSocket != -1)
    {
        const int fSuccessClose = ::close(_clientSocket);
        _clientSocket = -1;
        _ASSERTE(fSuccessClose != -1);
        if (fSuccessClose == -1)
        {
            // TODO: Add error handling.
            return false;
        }
        return true;
    }
    return false;
}

bool DiagnosticsIpc::IsValidStatus() const
{
    return _serverSocket != -1;
}

bool DiagnosticsIpc::Accept()
{
    const int fSuccessListen = ::listen(_serverSocket, /* backlog */ 255); // TODO: Unlimited here?
    _ASSERTE(fSuccessListen != -1);
    if (fSuccessListen == -1)
    {
        // TODO: Add error handling.
        return false;
    }

    sockaddr_un from;
    socklen_t fromlen = sizeof(from);
    _clientSocket = ::accept(_serverSocket, (sockaddr *)&from, &fromlen);
    _ASSERTE(_clientSocket != -1);
    if (_clientSocket == -1)
    {
        // TODO: Add error handling.
        return false;
    }
    return true;
}

bool DiagnosticsIpc::Read(void *lpBuffer, const uint32_t nBytesToRead, uint32_t &nBytesRead) const
{
    _ASSERTE(lpBuffer != nullptr);

    const ssize_t ssize = ::recv(_clientSocket, lpBuffer, nBytesToRead, 0);
    const bool fSuccess = ssize != -1;

    if (!fSuccess)
    {
        // TODO: Add error handling.
    }

    nBytesRead = fSuccess ? static_cast<std::remove_reference<decltype(nBytesRead)>::type>(ssize) : 0;
    return fSuccess;
}

bool DiagnosticsIpc::Write(const void *lpBuffer, const uint32_t nBytesToWrite, uint32_t &nBytesWritten) const
{
    _ASSERTE(lpBuffer != nullptr);

    const ssize_t ssize = ::send(_clientSocket, lpBuffer, nBytesToWrite, 0);
    const bool fSuccess = ssize != -1;

    if (!fSuccess)
    {
        // TODO: Add error handling.
    }

    nBytesWritten = fSuccess ? static_cast<std::remove_reference<decltype(nBytesWritten)>::type>(ssize) : 0;
    return fSuccess;
}

bool DiagnosticsIpc::Flush() const
{
    return true;
}

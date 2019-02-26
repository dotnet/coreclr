// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __DIAGNOSTICS_IPC_H__
#define __DIAGNOSTICS_IPC_H__


#ifdef FEATURE_PAL
  struct sockaddr_un;
#else
  #include <stdint.h>
  #include <Windows.h>
#endif /* FEATURE_PAL */

class DiagnosticsIpc final
{
public:
    DiagnosticsIpc(const char *const pIpcName, const uint32_t pid);
    ~DiagnosticsIpc();

    bool IsValidStatus() const;

    bool Open();
    bool Close();

    bool Accept();
    bool Read(void *lpBuffer, const uint32_t nBytesToRead, uint32_t &nBytesRead) const;
    bool Write(const void *lpBuffer, const uint32_t nBytesToWrite, uint32_t &nBytesWritten) const;
    bool Flush() const;

    /*
     * Bind -> bool
     * (Listen/Accept)(Server)/Connect(Client) -> bool
     * Send/Receive -> bool
     */

    DiagnosticsIpc() = delete;
    DiagnosticsIpc(const DiagnosticsIpc &src) = delete;
    DiagnosticsIpc(DiagnosticsIpc &&src) = delete;
    DiagnosticsIpc &operator=(const DiagnosticsIpc &rhs) = delete;
    DiagnosticsIpc &&operator=(DiagnosticsIpc &&rhs) = delete;

private:
#ifdef FEATURE_PAL
    int _serverSocket = -1;
    int _clientSocket = -1;
    sockaddr_un *const _pServerAddress;
#else
    char _pNamedPipeName[256]; // https://docs.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-createnamedpipea
    HANDLE _hPipe = INVALID_HANDLE_VALUE;
#endif /* FEATURE_PAL */
};

#endif // __DIAGNOSTICS_IPC_H__

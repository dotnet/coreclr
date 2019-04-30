// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __DIAGNOSTICS_IPC_H__
#define __DIAGNOSTICS_IPC_H__

#include <stdint.h>

#ifdef FEATURE_PAL
  struct sockaddr_un;
#else
  #include <Windows.h>
#endif /* FEATURE_PAL */

typedef void (*ErrorCallback)(const char *szMessage, uint32_t code);

namespace Ipc
{

    enum IpcVersion : uint8_t
    {
        DOTNET_IPC_V1 = 0x01,
    };

    // "DOTNET_IPC_V1\0\0\0"
    static const char *DOTNET_IPC_V1 = { 0x68, 0x79, 0x84, 0x78, 0x69, 0x84, 0x95, 0x73, 0x80, 0x67, 0x95, 0x86, 0x49, 0x00, 0x00, 0x00 }

    const char *IpcVersionString(IpcVersion versionEnum)
    {
        switch (versionEnum)
        {
            case IpcVersion::DOTNET_IPC_V1:
                return DOTNET_IPC_V1;
            default:
                ASSERT("Unkown IPC Version")
        }
    };

    // The header to be associated with every command and response
    // to/from the diagnostics server
    struct IpcHeader
    {
        char     Magic[16]; // Magic Version number; a 0 terminated char array
        uint16_t Size;      // The size of the incoming packet, size = header + payload size
        uint16_t Command;   // top two bytes are the command_set; bottom two bytes are the command
        uint16_t Reserved;  // reserved for future use
    };

    // TODO: use a shared pointer
    typedef void *pIpcPacketData;

    class IpcPackable
    {
    public:
        // Flatten  class into given buffer at cursor
        virtual bool Flatten(BYTE *lpBufferCursor) = 0;

        // Calculate size of thing
        virtual const uint16_t GetSize() = 0;
    };

    class NullIpcPayload : IpcPackable
    {
        bool Flatten(BYTE *lpBufferCursor) override {};
        const uint16_t GetSize() override { return 0; };
    };

    template <class T>
    class IpcPacket
    {
    public:
        IpcPacket(struct IpcHeader header, const T &payload)
            : m_Header(header), m_Payload(payload)
        {
            static_assert(std::is_base_of<IpcPackable, T>::value, "Type parameter must derive from IpcPackable");
        };

        // Attempt to populate header and payload from a buffer.
        // Payload is left opaque as a flattened buffer in m_pData
        bool TryParse(void *lpBuffer, uint32_t &nBytesRead);

        // Given a buffer, attempt to parse out a given payload type
        template <typename U>
        bool TryParsePayload(void *lpBuffer, uint32_t &nBytesRead, const U *&result);

        // Create a buffer of the correct size filled with
        // header + payload
        bool Flatten()
        {
            if (IsFlattened())
                return true;

            S_UINT16 temp_size = S_UINT16(0);
            temp_size += sizeof(struct IpcHeader) + m_Payload.GetSize();
            if (temp_size.Overflow())
            {
                // TODO: what should this do?
            }

            m_size = temp_size;

            BYTE *temp_buffer = new (no_throw) BYTE[m_Size];
            BYTE *temp_buffer_cursor = temp_buffer;

            if (temp_buffer == NULL)
            {
                // TODO: Error
            }

            memcpy(temp_buffer_cursor, m_Header, sizeof(struct IpcHeader));
            temp_buffer_cursor += sizeof(struct IpcHeader);

            m_Payload.Flatten(temp_buffer_cursor);
        };

        BYTE *GetFlatData() const 
        {
            if (!IsFlattened() && !Flatten())
                return NULL; // TODO: Error
            return m_pData;
        };

        // TODO: ensure ownership of pointer isn't transfered
        const IpcHeader *TryParseHeader()
        {
            return reinterpret_cast<const struct IpcHeader *>(GetFlatData());
        };
    private:
        // Pointer to flattened buffer filled with packet
        BYTE *m_pData;
        T m_Payload;
        struct IpcHeader m_Header;
        uint16_t m_Size;

        bool IsFlattened() const
        {
            return m_pData != NULL;
        };
    };
}

class IpcStream final
{
public:
    ~IpcStream();
    bool Read(void *lpBuffer, const uint32_t nBytesToRead, uint32_t &nBytesRead) const;
    bool Write(const void *lpBuffer, const uint32_t nBytesToWrite, uint32_t &nBytesWritten) const;
    bool Flush() const;

    class DiagnosticsIpc final
    {
    public:
        ~DiagnosticsIpc();

        //! Creates an IPC object
        static DiagnosticsIpc *Create(const char *const pIpcName, ErrorCallback callback = nullptr);

        //! Enables the underlaying IPC implementation to accept connection.
        IpcStream *Accept(ErrorCallback callback = nullptr) const;

        //! Used to unlink the socket so it can be removed from the filesystem
        //! when the last reference to it is closed.
        void Unlink(ErrorCallback callback = nullptr);

    private:

#ifdef FEATURE_PAL
        const int _serverSocket;
        sockaddr_un *const _pServerAddress;
        bool _isUnlinked = false;

        DiagnosticsIpc(const int serverSocket, sockaddr_un *const pServerAddress);
#else
        static const uint32_t MaxNamedPipeNameLength = 256;
        char _pNamedPipeName[MaxNamedPipeNameLength]; // https://docs.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-createnamedpipea

        DiagnosticsIpc(const char(&namedPipeName)[MaxNamedPipeNameLength]);
#endif /* FEATURE_PAL */

        DiagnosticsIpc() = delete;
        DiagnosticsIpc(const DiagnosticsIpc &src) = delete;
        DiagnosticsIpc(DiagnosticsIpc &&src) = delete;
        DiagnosticsIpc &operator=(const DiagnosticsIpc &rhs) = delete;
        DiagnosticsIpc &&operator=(DiagnosticsIpc &&rhs) = delete;
    };

private:
#ifdef FEATURE_PAL
    int _clientSocket = -1;
    IpcStream(int clientSocket) : _clientSocket(clientSocket) {}
#else
    HANDLE _hPipe = INVALID_HANDLE_VALUE;
    IpcStream(HANDLE hPipe) : _hPipe(hPipe) {}
#endif /* FEATURE_PAL */

    IpcStream() = delete;
    IpcStream(const IpcStream &src) = delete;
    IpcStream(IpcStream &&src) = delete;
    IpcStream &operator=(const IpcStream &rhs) = delete;
    IpcStream &&operator=(IpcStream &&rhs) = delete;
};

#endif // __DIAGNOSTICS_IPC_H__

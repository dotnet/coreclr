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

// "DOTNET_IPC_V1\0\0\0"
constexpr const char DOTNET_IPC_V1_MAGIC[14] = "DOTNET_IPC_V1";

namespace DiagnosticsIpc
{
    enum IpcVersion : uint8_t
    {
        DOTNET_IPC_V1 = 0x01,
        // FUTURE
    };

    // The header to be associated with every command and response
    // to/from the diagnostics server
    struct IpcHeader
    {
        char     Magic[14];  // Magic Version number; a 0 terminated char array
        uint16_t Size;       // The size of the incoming packet, size = header + payload size
        uint8_t  CommandSet; // The scope of the Command.
        uint8_t  Command;    // The command being sent
        uint16_t Reserved;   // reserved for future use
    };

    // The Following structs are template, meta-programming to enable
    // users of the IpcMessage class to get free serialization for fixed-size structures.

    // recreates the functionality of the same named helper in std::type_traits
    template <bool B, class T = void>
    using enable_if_t = typename std::enable_if<B, T>::type;

    // template meta-programming to check for bool(Flatten)(void*) member function
    template <typename T>
    struct HasFlatten
    {
        using yes = uint16_t;
        using no  = uint8_t;
        template <typename U, U u> struct Has;
        template <typename U> static constexpr yes& test(Has<bool (U::*)(void*), &U::Flatten>*);
        template <typename U> static constexpr no& test(...);
        static constexpr bool value = sizeof(test<T>(int())) == sizeof(yes);
    };

    // template meta-programming to check for uint16_t(GetSize)() member function
    template <typename T>
    struct HasGetSize
    {
        using yes = uint16_t;
        using no  = uint8_t;
        template <typename U, U u> struct Has;
        template <typename U> static constexpr yes& test(Has<uint16_t (U::*)(), &U::GetSize>*);
        template <typename U> static constexpr no& test(...);
        static constexpr bool value = sizeof(test<T>(int())) == sizeof(yes);
    };

    // Encodes the messages sent and received by the Diagnostics Server.
    //
    // Payloads that are fixed-size structs don't require any custom functionality.
    //
    // Payloads that are NOT fixed-size simply need to implement the following methods:
    //  * uint16_t GetSize()           -> should return the flattened size of the payload
    //  * bool Flatten(BYTE *lpBuffer) -> Should serialize and write the payload to the provided buffer
    template <class T>
    class IpcMessage
    {
    public:
        // Create an outgoing IpcMessage from a header and a payload
        IpcMessage(struct IpcHeader header, const T &payload)
            : m_Header(header), m_Payload(payload)
        {
            Flatten(); // TODO: Error checking
        };

        // Create an incoming IpcMessage from an incoming buffer
        // TODO: deal with ownership of pointer here? Don't want to needlesly copy data...
        IpcMessage(BYTE *lpBuffer)
            : m_pData(lpBuffer)
        {
            uint32_t bytesRead;
            TryParse(m_pData, bytesRead);
        };

        // Attempt to populate header and payload from a buffer.
        // Payload is left opaque as a flattened buffer in m_pData
        static bool TryParse(BYTE* lpBuffer, uint32_t& nBytesRead)
        {
            // TODO
        };

        // Given a buffer, attempt to parse out a given payload type
        template <typename U>
        bool TryParsePayload(BYTE *lpBuffer, uint32_t &nBytesRead, const U *&result);

        // Create a buffer of the correct size filled with
        // header + payload. Correctly handles flattening of
        // trivial structures, but uses a bool(Flatten)(void*)
        // and uint16_t(GetSize)() when available.
        bool Flatten()
        {
            return FlattenImpl<T>();
        };

        BYTE *GetFlatData()
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

        ~IpcMessage()
        {
            // TODO: ensure flattened buffer is dealt with
            delete m_pData;
        };

        bool IsFlattened() const
        {
            return m_pData != NULL;
        };

        // Handles the case where the payload structure exposes Flatten
        // and GetSize methods
        template <typename U>
        enable_if_t<HasFlatten<U>::value && HasGetSize<U>::value, bool>
        FlattenImpl()
        {
            if (IsFlattened())
                return true;

            S_UINT16 temp_size = S_UINT16(0);
            temp_size += sizeof(struct IpcHeader) + m_Payload.GetSize();
            if (temp_size.IsOverflow())
            {
                // TODO: what should happen here?
                return false;
            }

            m_Size = temp_size.Value;

            BYTE* temp_buffer = new (nothrow) BYTE[m_Size];
            BYTE* temp_buffer_cursor = temp_buffer;

            if (temp_buffer == NULL)
            {
                // TODO: Error
                return false;
            }

            m_Header.Size = m_Size;

            memcpy(temp_buffer_cursor, &m_Header, sizeof(struct IpcHeader));
            temp_buffer_cursor += sizeof(struct IpcHeader);

            m_Payload.Flatten(temp_buffer_cursor);

            m_pData = temp_buffer;

            return true;
        }

        // handles the case where we were handed a struct with no Flatten or GetSize method
        template <typename U>
        enable_if_t<!HasFlatten<U>::value && !HasGetSize<U>::value, bool>
        FlattenImpl()
        {
            if (IsFlattened())
                return true;

            S_UINT16 temp_size = S_UINT16(0);
            temp_size += sizeof(struct IpcHeader) + sizeof(m_Payload);
            if (temp_size.IsOverflow())
            {
                // TODO: what should happen here?
                return false;
            }

            m_Size = temp_size.Value;

            BYTE* temp_buffer = new (nothrow) BYTE[m_Size];
            BYTE* temp_buffer_cursor = temp_buffer;

            if (temp_buffer == NULL)
            {
                // TODO: Error
                return false;
            }

            m_Header.Size = m_Size;

            memcpy(temp_buffer_cursor, &m_Header, sizeof(struct IpcHeader));
            temp_buffer_cursor += sizeof(struct IpcHeader);

            memcpy(temp_buffer_cursor, &m_Payload, sizeof(m_Payload));

            m_pData = temp_buffer;

            return true;
        }
    };

    template <class T>
    class IpcCommand : private IpcMessage<T>
    {
    public:
        IpcCommand(uint16_t);
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

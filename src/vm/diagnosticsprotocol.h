// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __DIAGNOSTICS_PROTOCOL_H__
#define __DIAGNOSTICS_PROTOCOL_H__

#ifdef FEATURE_PERFTRACING

#include "clr_std/type_traits"
#include "new.hpp"
//#include "common.h"

#define DOTNET_IPC_V1_MAGIC "DOTNET_IPC_V1"

template <typename T>
bool TryParse(uint8_t *&bufferCursor, uint32_t &bufferLen, T &result)
{
    static_assert(
        std::is_integral<T>::value || std::is_same<T, float>::value ||
        std::is_same<T, double>::value || std::is_same<T, CLSID>::value,
        "Can only be instantiated with integral and floating point types.");

    if (bufferLen < sizeof(T))
        return false;
    result = *(reinterpret_cast<T *>(bufferCursor));
    bufferCursor += sizeof(T);
    bufferLen -= sizeof(T);
    return true;
}

template <typename T>
bool TryParseString(uint8_t *&bufferCursor, uint32_t &bufferLen, const T *&result)
{
    static_assert(
        std::is_same<T, char>::value || std::is_same<T, wchar_t>::value,
        "Can only be instantiated with char and wchar_t types.");

    uint32_t stringLen = 0;
    if (!TryParse(bufferCursor, bufferLen, stringLen))
        return false;
    if (stringLen == 0)
    {
        result = nullptr;
        return true;
    }
    if (stringLen > (bufferLen / sizeof(T)))
        return false;
    if ((reinterpret_cast<const T *>(bufferCursor))[stringLen - 1] != 0)
        return false;
    result = reinterpret_cast<const T *>(bufferCursor);

    const uint32_t TotalStringLength = stringLen * sizeof(T);
    bufferCursor += TotalStringLength;
    bufferLen -= TotalStringLength;
    return true;
}

namespace DiagnosticsIpc
{
    enum class IpcVersion : uint8_t
    {
        DOTNET_IPC_V1 = 0x01,
        // FUTURE
    };

    enum class DiagnosticServerCommandSet : uint8_t
    {
        // Debug         = 0x00,
        Miscellaneous = 0x01,
        EventPipe = 0x02,

        Server = 0xFF,
    };

    enum class DiagnosticServerCommandId : uint8_t
    {
        OK = 0x00,
        Error = 0xFF,
    };

    enum class DiagnosticServerErrorCode : uint32_t
    {
        OK = 0x00000000,
        BadEncoding = 0x00000001,
        UnknownCommandSet = 0x00000002,
        UnknownCommandId = 0x00000003,
        UnknownVersion = 0x00000004,
        // future

        BAD = 0xFFFFFFFF,
    };

    // The header to be associated with every command and response
    // to/from the diagnostics server
    struct IpcHeader
    {
        uint8_t  Magic[14];  // Magic Version number; a 0 terminated char array
        uint16_t Size;       // The size of the incoming packet, size = header + payload size
        uint8_t  CommandSet; // The scope of the Command.
        uint8_t  CommandId;  // The command being sent
        uint16_t Reserved;   // reserved for future use
    };

    struct ServerErrorPayload
    {
        DiagnosticServerErrorCode code;
    };

    constexpr IpcHeader GenericSuccessHeader =
    {
        DOTNET_IPC_V1_MAGIC,
        (uint16_t)20,
        (uint8_t)DiagnosticServerCommandSet::Server,
        (uint8_t)DiagnosticServerCommandId::OK,
        (uint16_t)0x0000
    };

    constexpr IpcHeader GenericErrorHeader =
    {
        DOTNET_IPC_V1_MAGIC,
        (uint16_t)20,
        (uint8_t)DiagnosticServerCommandSet::Server,
        (uint8_t)DiagnosticServerCommandId::Error,
        (uint16_t)0x0000
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
        template <typename U> static constexpr yes& test(Has<uint16_t(U::*)(), &U::GetSize>*);
        template <typename U> static constexpr no& test(...);
        static constexpr bool value = sizeof(test<T>(int())) == sizeof(yes);
    };

    // template meta-programming to check for const T*(TryParse)(BYTE*,uint16_t&) member function
    template <typename T>
    struct HasTryParse
    {
        using yes = uint16_t;
        using no  = uint8_t;
        template <typename U, U u> struct Has;
        template <typename U> static constexpr yes& test(Has<const U* (U::*)(BYTE*, uint16_t&), &U::TryParse>*);
        template <typename U> static constexpr no& test(...);
        static constexpr bool value = sizeof(test<T>(int())) == sizeof(yes);
    };

    // Encodes the messages sent and received by the Diagnostics Server.
    //
    // Payloads that are fixed-size structs don't require any custom functionality.
    //
    // Payloads that are NOT fixed-size simply need to implement the following methods:
    //  * uint16_t GetSize()                                     -> should return the flattened size of the payload
    //  * bool Flatten(BYTE *lpBuffer)                           -> Should serialize and write the payload to the provided buffer
    //  * const T *TryParse(BYTE *lpBuffer, uint16_t& bufferLen) -> should decode payload or return nullptr
    class IpcMessage
    {
    public:
        // Create an outgoing IpcMessage from a header and a payload
        template <typename T>
        IpcMessage(IpcHeader header, T& payload)
            : m_Header(header), m_Size(0), m_pData(nullptr)
        {
            FlattenImpl<T>(payload);
        };

        // Create an incoming IpcMessage from an incoming buffer
        IpcMessage(::IpcStream* pStream)
            : m_Header(), m_Size(0), m_pData(nullptr)
        {
            TryParse(pStream);
        };

        IpcMessage(DiagnosticServerErrorCode errorCode)
            : IpcMessage(GenericErrorHeader, errorCode)
        {};

        ~IpcMessage()
        {
            delete m_pData;
        };

        // Attempt to populate header and payload from a buffer.
        // Payload is left opaque as a flattened buffer in m_pData
        bool TryParse(::IpcStream* pStream)
        {
            // Read out header first
            uint32_t nBytesRead;
            bool success = pStream->Read(&m_Header, sizeof(IpcHeader), nBytesRead);
            if (nBytesRead < sizeof(IpcHeader) || !success)
            {
                return false;
            }

            // Then read out payload to buffer
            uint16_t payloadSize = m_Header.Size - sizeof(IpcHeader);
            BYTE* temp_buffer = new (nothrow) BYTE[payloadSize];
            success = pStream->Read(temp_buffer, payloadSize, nBytesRead);
            if (nBytesRead < payloadSize)
            {
                delete[] temp_buffer;
                return false;
            }

            m_pData = temp_buffer;
            m_Size = m_Header.Size;

            return true;
        };

        // Given a buffer, attempt to parse out a given payload type
        // If a payload type is fixed-size, this will simply return
        // a pointer to the buffer of data reinterpreted as a const pointer.
        // Otherwise, your payload type should implment the following static method:
        // * const T *TryParse(BYTE *lpBuffer)
        // which this will call if it exists.
        template <typename T>
        const T* TryParsePayload()
        {
            ASSERT(IsFlattened());
            return TryParsePayloadImpl<T>();
        };

        BYTE* GetFlatData()
        {
            return m_pData;
        };

        const IpcHeader GetHeader()
        {
            return m_Header;
        };

        bool Send(IpcStream* pStream)
        {
            ASSERT(IsFlattened());
            uint32_t nBytesWritten;
            pStream->Write(m_pData, m_Size, nBytesWritten);

            return nBytesWritten == m_Size;
        };
    private:
        // Pointer to flattened buffer filled with packet
        BYTE* m_pData;
        struct IpcHeader m_Header;
        uint16_t m_Size;

        bool IsFlattened() const
        {
            return m_pData != NULL;
        };

        // Create a buffer of the correct size filled with
        // header + payload. Correctly handles flattening of
        // trivial structures, but uses a bool(Flatten)(void*)
        // and uint16_t(GetSize)() when available.
        template <class T>
        bool Flatten(T& payload)
        {
            return FlattenImpl<T>(payload);
        };

        // Handles the case where the payload structure exposes Flatten
        // and GetSize methods
        template <typename U>
        enable_if_t<HasFlatten<U>::value&& HasGetSize<U>::value, bool>
            FlattenImpl(U& payload)
        {
            if (IsFlattened())
                return true;

            S_UINT16 temp_size = S_UINT16(0);
            temp_size += sizeof(struct IpcHeader) + payload.GetSize();
            ASSERT(!temp_size.IsOverflow());

            m_Size = temp_size.Value();

            BYTE* temp_buffer = new (nothrow) BYTE[m_Size];
            BYTE* temp_buffer_cursor = temp_buffer;

            if (temp_buffer == NULL)
            {
                return false;
            }

            m_Header.Size = m_Size;

            memcpy(temp_buffer_cursor, &m_Header, sizeof(struct IpcHeader));
            temp_buffer_cursor += sizeof(struct IpcHeader);

            payload.Flatten(temp_buffer_cursor);

            m_pData = temp_buffer;

            return true;
        };

        // handles the case where we were handed a struct with no Flatten or GetSize method
        template <typename U>
        // enable_if_t<!HasFlatten<U>::value && !HasGetSize<U>::value, bool>
        bool FlattenImpl(U& payload)
        {
            if (IsFlattened())
                return true;

            S_UINT16 temp_size = S_UINT16(0);
            temp_size += sizeof(struct IpcHeader) + sizeof(payload);
            ASSERT(!temp_size.IsOverflow());

            m_Size = temp_size.Value();

            BYTE* temp_buffer = new (nothrow) BYTE[m_Size];
            BYTE* temp_buffer_cursor = temp_buffer;

            if (temp_buffer == NULL)
            {
                return false;
            }

            m_Header.Size = m_Size;

            memcpy(temp_buffer_cursor, &m_Header, sizeof(struct IpcHeader));
            temp_buffer_cursor += sizeof(struct IpcHeader);

            memcpy(temp_buffer_cursor, &payload, sizeof(payload));

            m_pData = temp_buffer;

            return true;
        };

        template <typename U>
        enable_if_t<HasTryParse<U>::value, const U*>
            TryParsePayloadImpl()
        {
            return U::TryParse(m_pData, m_Size - sizeof(IpcHeader));
        };

        template <typename U>
        // enable_if_t<!HasTryParse<U>::value, const U*>
        const U* TryParsePayloadImpl()
        {
            return reinterpret_cast<const U*>(m_pData);
        };
    };
};

#endif // FEATURE_PERFTRACING

#endif // __DIAGNOSTICS_PROTOCOL_H__

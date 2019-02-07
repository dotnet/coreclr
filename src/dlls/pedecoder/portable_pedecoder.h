// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#ifndef PORTABLE_PEDECODER_H
#define PORTABLE_PEDECODER_H

#include <cstdint>

class PEDecoder;

namespace portable_pedecoder
{
    using hresult_t = std::int32_t;

    struct IMAGE_COR_VTABLEFIXUP
    {
        std::uint32_t RVA;
        std::uint16_t Count;
        std::uint16_t Type;
    };

    class PEDecoder
    {
    public:
        PEDecoder();
        ~PEDecoder();
        PEDecoder(PEDecoder&& other) = default;
        PEDecoder& operator=(PEDecoder&& other) = default;

        hresult_t Init(void* mappedBase);
        bool CheckCORFormat() const;
        bool IsILOnly() const;
        bool HasManagedEntryPoint() const;
        bool HasNativeEntryPoint() const;
        void* GetNativeEntryPoint() const;
        IMAGE_COR_VTABLEFIXUP* GetVTableFixups(std::uint32_t* numFixupRecords) const;
        void* GetBase() const;
        std::uintptr_t GetRvaData(std::int32_t rva) const;

    private:
        ::PEDecoder* m_impl;
    };
}

#endif

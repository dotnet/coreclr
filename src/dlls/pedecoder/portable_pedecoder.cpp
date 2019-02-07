// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "portable_pedecoder.h"
#include "pedecoder.h"
#include "pedecoder.inl"

namespace portable_pedecoder
{

PEDecoder::PEDecoder()
    :m_impl(new ::PEDecoder())
{
}

PEDecoder::~PEDecoder()
{
    delete m_impl;
}

hresult_t PEDecoder::Init(void* mappedBase)
{
    return m_impl->Init(mappedBase);
}

bool PEDecoder::CheckCORFormat() const
{
    return !!m_impl->CheckCORFormat();
}

bool PEDecoder::IsILOnly() const
{
    return !!m_impl->IsILOnly();
}

bool PEDecoder::HasManagedEntryPoint() const
{
    return !!m_impl->HasManagedEntryPoint();
}

bool PEDecoder::HasNativeEntryPoint() const
{
    return !!m_impl->HasNativeEntryPoint();
}

void* PEDecoder::GetNativeEntryPoint() const
{
    return m_impl->GetNativeEntryPoint();
}

IMAGE_COR_VTABLEFIXUP* PEDecoder::GetVTableFixups(std::uint32_t* numFixupRecords) const
{
    // Verify that we have the exact same layout as the offical copy of the IMAGE_COR_VTABLEFIXUP type.
    static_assert_no_msg(sizeof(IMAGE_COR_VTABLEFIXUP) == sizeof(::IMAGE_COR_VTABLEFIXUP));
    static_assert_no_msg(offsetof(IMAGE_COR_VTABLEFIXUP, RVA) == offsetof(::IMAGE_COR_VTABLEFIXUP, RVA));
    static_assert_no_msg(offsetof(IMAGE_COR_VTABLEFIXUP, Count) == offsetof(::IMAGE_COR_VTABLEFIXUP, Count));
    static_assert_no_msg(offsetof(IMAGE_COR_VTABLEFIXUP, Type) == offsetof(::IMAGE_COR_VTABLEFIXUP, Type));

    return reinterpret_cast<IMAGE_COR_VTABLEFIXUP*>(m_impl->GetVTableFixups(numFixupRecords));
}

void* PEDecoder::GetBase() const
{
    return m_impl->GetBase();
}

std::uintptr_t PEDecoder::GetRvaData(std::int32_t rva) const
{
    return m_impl->GetRvaData(rva);
}

}



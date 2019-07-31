// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <platformdefines.h>
#include <type_traits>
#include <algorithm>

template<typename StringT>
struct StringTraits
{
    using type = StringT;
    using CharT = typename std::remove_pointer<StringT>::type;
    using ConstStringT = CharT const *;
};

struct BSTRTraits
{
    using type = BSTR;
    using ConstStringT = BSTR;
};

template<typename StringT>
using VerifyReversedCallback = BOOL (__cdecl*)(StringT original, StringT reversed);

template<typename StringT>
using ReverseCallback = void(__cdecl*)(StringT original, StringT* reversed);

template<typename StringT>
using ReverseCallback = void(__cdecl*)(StringT original, StringT* reversed);

template<typename StringT>
using ReverseCallbackReturned = StringT(__cdecl*)(StringT original);

template<typename StringT>
using ReverseInplaceCallback = void(__cdecl*)(StringT str);

template<typename StringTraitsT, size_t LengthFunction(typename StringTraitsT::ConstStringT), typename CharT = typename StringTraitsT::CharT>
struct StringMarshalingTestsBase
{
    using StringT = typename StringTraitsT::type;
    using ConstStringT = typename StringTraitsT::ConstStringT;

    static BOOL Compare(ConstStringT expected, const ConstStringT actual)
    {
        if (LengthFunction(expected) != LengthFunction(actual))
        {
            return FALSE;
        }

        size_t length = LengthFunction(expected);
        ConstStringT currE = expected;
        ConstStringT endE = expected + length;
        ConstStringT currA = actual;
        ConstStringT endA = actual + length;
        for (; currE != endE; ++currE, ++currA)
        {
            if (((CharT)*currE) != ((CharT)*currA))
                return FALSE;
        }
        
        return TRUE;
    }

    static void ReverseInplace(StringT str)
    {
        std::reverse((CharT*)str, (CharT*)str + LengthFunction(str));
    }
};

template<typename StringT, size_t LengthFunction(typename StringTraits<StringT>::ConstStringT)>
struct StringMarshalingTests : StringMarshalingTestsBase<StringTraits<StringT>, LengthFunction>
{
    static void Reverse(StringT str, StringT* result)
    {
        size_t length = LengthFunction(str);
        size_t byteSize = sizeof(typename StringTraits<StringT>::CharT) * (length + 1);
        StringT buffer = (StringT)CoreClrAlloc(byteSize);
        
        memcpy(buffer, str, byteSize);

        ReverseInplace(buffer);
        *result = buffer;
    }

    static void FreeString(StringT str)
    {
        CoreClrFree(str);
    }
};

template<size_t LengthFunction(BSTR), typename CharT, BSTR Alloc(CharT const*, size_t)>
struct BStrMarshalingTests : StringMarshalingTestsBase<BSTRTraits, LengthFunction, CharT>
{
    static void Reverse(BSTR str, StringT* result)
    {
        size_t length = LengthFunction(str);
        StringT buffer = Alloc((CharT const*)str, length);

        ReverseInplace(buffer);
        *result = buffer;
    }

    static void FreeString(StringT str)
    {
        SysFreeString(str);
    }
};

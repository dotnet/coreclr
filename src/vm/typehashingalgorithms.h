// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// ---------------------------------------------------------------------------
// Generic functions to compute the hashcode value of types
// ---------------------------------------------------------------------------

#pragma once
#include <stdlib.h>

class HashCodeBuilder
{
    int _hash1;
    int _hash2;
    int _numCharactersHashed;
    
public:
    HashCodeBuilder(LPCUTF8 seed)
    {
        _hash1 = 0x6DA3B944;
        _hash2 = 0;
        _numCharactersHashed = 0;

        Append(seed);
    }

    void Append(LPCUTF8 src)
    {
        if (src == NULL || *src == '\0')
            return;

        LPWSTR srcWide;
        COUNT_T lsrcWide = WszMultiByteToWideChar(CP_UTF8, 0, src, -1, 0, 0);
        srcWide = (LPWSTR)alloca(lsrcWide * sizeof(WCHAR));
        WszMultiByteToWideChar(CP_UTF8, 0, src, -1, srcWide, lsrcWide);
        lsrcWide--;

        int startIndex = 0;
        if ((_numCharactersHashed & 1) == 1)
        {
            _hash2 = (_hash2 + _rotl(_hash2, 5)) ^ srcWide[0];
            startIndex = 1;
        }

        for (COUNT_T i = startIndex; i < lsrcWide; i += 2)
        {
            _hash1 = (_hash1 + _rotl(_hash1, 5)) ^ srcWide[i];
            if (srcWide[i + 1] != '\0')
                _hash2 = (_hash2 + _rotl(_hash2, 5)) ^ srcWide[i + 1];
        }

        _numCharactersHashed += lsrcWide;
    }

    int ToHashCode()
    {
        int hash1 = _hash1 + _rotl(_hash1, 8);
        int hash2 = _hash2 + _rotl(_hash2, 8);

        return hash1 ^ hash2;
    }
};

//
// Returns the hashcode value of the 'src' string
//
inline static int ComputeNameHashCode(LPCUTF8 src)
{
    LIMITED_METHOD_CONTRACT;

    if (src == NULL || *src == '\0')
        return 0;

    int hash1 = 0x6DA3B944;
    int hash2 = 0;

    LPWSTR srcWide;
    COUNT_T lsrcWide = WszMultiByteToWideChar(CP_UTF8, 0, src, -1, 0, 0);
    srcWide = (LPWSTR)alloca(lsrcWide * sizeof(WCHAR));
    WszMultiByteToWideChar(CP_UTF8, 0, src, -1, srcWide, lsrcWide);
    lsrcWide--;

    for (COUNT_T i = 0; i < lsrcWide; i += 2)
    {
        hash1 = (hash1 + _rotl(hash1, 5)) ^ srcWide[i];
        if (srcWide[i + 1] != '\0')
            hash2 = (hash2 + _rotl(hash2, 5)) ^ srcWide[i + 1];
        else
            break;
    }

    hash1 += _rotl(hash1, 8);
    hash2 += _rotl(hash2, 8);

    return hash1 ^ hash2;
}

inline static int ComputeNameHashCode(LPCUTF8 pszNamespace, LPCUTF8 pszName)
{
    LIMITED_METHOD_CONTRACT;

    HashCodeBuilder hashCodeBuilder(pszNamespace);
    if (pszNamespace != NULL && *pszNamespace != '\0')
        hashCodeBuilder.Append(".");
    hashCodeBuilder.Append(pszName);
    return hashCodeBuilder.ToHashCode();
}

inline static int ComputeArrayTypeHashCode(int elementTypeHashcode, int rank)
{
    LIMITED_METHOD_CONTRACT;

    // DIFFERENT FROM CORERT: This is much simplified compared to CoreRT, to avoid converting.rank to string.
    // For single-dimensinal array, the result is identical to CoreRT.
    int hashCode = 0xd5313556 + rank;
    if (rank == 1)
        _ASSERTE(hashCode == ComputeNameHashCode("System.Array`1"));

    hashCode = (hashCode + _rotl(hashCode, 13)) ^ elementTypeHashcode;
    return (hashCode + _rotl(hashCode, 15));
}

inline static int ComputePointerTypeHashCode(int pointeeTypeHashcode)
{
    LIMITED_METHOD_CONTRACT;

    return (pointeeTypeHashcode + _rotl(pointeeTypeHashcode, 5)) ^ 0x12D0;
}

inline static int ComputeByrefTypeHashCode(int parameterTypeHashcode)
{
    LIMITED_METHOD_CONTRACT;

    return (parameterTypeHashcode + _rotl(parameterTypeHashcode, 7)) ^ 0x4C85;
}

inline static int ComputeNestedTypeHashCode(int enclosingTypeHashcode, int nestedTypeNameHash)
{
    LIMITED_METHOD_CONTRACT;

    return (enclosingTypeHashcode + _rotl(enclosingTypeHashcode, 11)) ^ nestedTypeNameHash;
}

template <typename TA, typename TB>
inline static int ComputeGenericInstanceHashCode(int definitionHashcode, int arity, const TA& genericTypeArguments, int (*getHashCode)(TB))
{
    LIMITED_METHOD_CONTRACT;

    int hashcode = definitionHashcode;
    for (int i = 0; i < arity; i++)
    {
        int argumentHashCode = getHashCode(genericTypeArguments[i]);
        hashcode = (hashcode + _rotl(hashcode, 13)) ^ argumentHashCode;
    }
    return (hashcode + _rotl(hashcode, 15));
}

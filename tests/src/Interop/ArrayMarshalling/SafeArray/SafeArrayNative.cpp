// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>
#include <oleauto.h>
#include <algorithm>
#include <platformdefines.h>
#include "Helpers.h"

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE XorBoolArray(SAFEARRAY* d, BOOL* result)
{
    *result = FALSE;
    VARTYPE elementType;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetVartype(d, &elementType));

    if (elementType != VT_BOOL)
    {
        return FALSE;
    }

    LONG lowerBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetLBound(d, 1, &lowerBoundIndex));
    LONG upperBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetUBound(d, 1, &upperBoundIndex));

    VARIANT_BOOL* values;
    RETURN_FALSE_IF_FAILED(::SafeArrayAccessData(d, (void**)&values));
    
    for(long i = lowerBoundIndex; i <= upperBoundIndex; i++)
    {
        *result ^= values[i] == VARIANT_TRUE ? TRUE : FALSE;
    }

    RETURN_FALSE_IF_FAILED(::SafeArrayUnaccessData(d));
    
    return TRUE;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE MeanDecimalArray(SAFEARRAY* d, DECIMAL* result)
{
    DECIMAL sum{};
    DECIMAL_SETZERO(sum);

    VARTYPE elementType;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetVartype(d, &elementType));

    if (elementType != VT_DECIMAL)
    {
        return FALSE;
    }

    LONG lowerBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetLBound(d, 1, &lowerBoundIndex));
    LONG upperBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetUBound(d, 1, &upperBoundIndex));

    DECIMAL* values;
    RETURN_FALSE_IF_FAILED(::SafeArrayAccessData(d, (void**)&values));
    
    for(long i = lowerBoundIndex; i <= upperBoundIndex; i++)
    {
        DECIMAL lhs = sum;
        VarDecAdd(&lhs, &values[i], &sum);
    }

    DECIMAL numElements;
    VarDecFromI4(upperBoundIndex - lowerBoundIndex + 1, &numElements);

    VarDecDiv(&sum, &numElements, result);

    RETURN_FALSE_IF_FAILED(::SafeArrayUnaccessData(d));
    
    return TRUE;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE SumCurrencyArray(SAFEARRAY* d, CY* result)
{
    CY sum{};
    VARTYPE elementType;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetVartype(d, &elementType));

    if (elementType != VT_CY)
    {
        return FALSE;
    }

    LONG lowerBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetLBound(d, 1, &lowerBoundIndex));
    LONG upperBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetUBound(d, 1, &upperBoundIndex));

    CY* values;
    RETURN_FALSE_IF_FAILED(::SafeArrayAccessData(d, (void**)&values));
    
    for(long i = lowerBoundIndex; i <= upperBoundIndex; i++)
    {
        CY lhs = sum;
        VarCyAdd(lhs, values[i], &sum);
    }

    *result = sum;

    RETURN_FALSE_IF_FAILED(::SafeArrayUnaccessData(d));
    
    return TRUE;
}

template <typename StringType>
StringType ReverseInplace(size_t len, StringType s)
{
    std::reverse(s, s + len);
    return s;
}

template<typename StringType>
bool Reverse(StringType str, StringType *res)
{
    StringType tmp = str;
    size_t len = 0;
    while (*tmp++)
        ++len;

    size_t strDataLen = (len + 1) * sizeof(str[0]);
    auto resLocal = (StringType)CoreClrAlloc(strDataLen);
    if (resLocal == nullptr)
        return false;

    memcpy(resLocal, str, strDataLen);
    *res = ReverseInplace(len, resLocal);

    return true;
}

bool ReverseBSTR(BSTR str, BSTR *res)
{
    size_t strDataLen = TP_SysStringByteLen(str);
    BSTR resLocal = TP_SysAllocStringByteLen(reinterpret_cast<LPCSTR>(str), strDataLen);
    if (resLocal == nullptr)
        return false;

    UINT len = TP_SysStringLen(str);
    *res = ReverseInplace(len, resLocal);

    return true;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE ReverseStrings(SAFEARRAY* d)
{
    VARTYPE elementType;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetVartype(d, &elementType));

    if (elementType != VT_LPSTR && elementType != VT_LPWSTR && elementType != VT_BSTR)
    {
        return FALSE;
    }

    LONG lowerBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetLBound(d, 1, &lowerBoundIndex));
    LONG upperBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetUBound(d, 1, &upperBoundIndex));

    void** values;
    RETURN_FALSE_IF_FAILED(::SafeArrayAccessData(d, (void**)&values));
    
    bool success = true;

    for(long i = lowerBoundIndex; i <= upperBoundIndex; i++)
    {
        if (elementType == VT_LPSTR)
        {
            LPSTR reversed;
            success &= Reverse((LPSTR)values[i], &reversed);
            values[i] = reversed;
        }
        else if (elementType == VT_LPWSTR)
        {
            LPWSTR reversed;
            success &= Reverse((LPWSTR)values[i], &reversed);
            values[i] = reversed;
        }
        else if (elementType == VT_BSTR)
        {
            BSTR reversed;
            success &= ReverseBSTR((BSTR)values[i], &reversed);
            values[i] = reversed;
        }

        if (!success)
        {
            ::SafeArrayUnaccessData(d);
            return FALSE;
        }
    }

    RETURN_FALSE_IF_FAILED(::SafeArrayUnaccessData(d));
    
    return TRUE;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE VerifyInterfaceArray(SAFEARRAY* d, VARTYPE expectedType)
{
    VARTYPE elementType;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetVartype(d, &elementType));

    if (elementType != expectedType)
    {
        return FALSE;
    }

    LONG lowerBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetLBound(d, 1, &lowerBoundIndex));
    LONG upperBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetUBound(d, 1, &upperBoundIndex));

    IUnknown** values;
    RETURN_FALSE_IF_FAILED(::SafeArrayAccessData(d, (void**)&values));
    
    for(long i = lowerBoundIndex; i <= upperBoundIndex; i++)
    {
        values[i]->AddRef();
        values[i]->Release();
    }

    RETURN_FALSE_IF_FAILED(::SafeArrayUnaccessData(d));
    
    return TRUE;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE MeanVariantIntArray(SAFEARRAY* d, int* result)
{
    *result = 0;
    VARTYPE elementType;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetVartype(d, &elementType));

    if (elementType != VT_VARIANT)
    {
        return FALSE;
    }

    LONG lowerBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetLBound(d, 1, &lowerBoundIndex));
    LONG upperBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetUBound(d, 1, &upperBoundIndex));

    VARIANT* values;
    RETURN_FALSE_IF_FAILED(::SafeArrayAccessData(d, (void**)&values));
    
    for(long i = lowerBoundIndex; i <= upperBoundIndex; i++)
    {
        if (values[i].vt != VT_I4)
        {
            ::SafeArrayUnaccessData(d);
            return FALSE;
        }
        
        *result += values[i].intVal;
    }

    *result /= upperBoundIndex - lowerBoundIndex + 1;

    RETURN_FALSE_IF_FAILED(::SafeArrayUnaccessData(d));
    
    return TRUE;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE DistanceBetweenDates(SAFEARRAY* d, double* result)
{
    *result = 0;
    VARTYPE elementType;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetVartype(d, &elementType));

    if (elementType != VT_DATE)
    {
        return FALSE;
    }

    LONG lowerBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetLBound(d, 1, &lowerBoundIndex));
    LONG upperBoundIndex;
    RETURN_FALSE_IF_FAILED(::SafeArrayGetUBound(d, 1, &upperBoundIndex));

    DATE* values;
    RETURN_FALSE_IF_FAILED(::SafeArrayAccessData(d, (void**)&values));
    
    bool haveLastValue = false;
    DATE lastValue;

    for(long i = lowerBoundIndex; i <= upperBoundIndex; i++)
    {
        if (haveLastValue)
        {
            *result += values[i] - lastValue;
        }

        lastValue = values[i];
        haveLastValue = true;
    }

    RETURN_FALSE_IF_FAILED(::SafeArrayUnaccessData(d));
    
    return TRUE;
}

struct StructWithSafeArray
{
    SAFEARRAY* array;
};

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE XorBoolArrayInStruct(StructWithSafeArray str, BOOL* result)
{
    return XorBoolArray(str.array, result);
}

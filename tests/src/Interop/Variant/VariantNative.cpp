// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>
#include "platformdefines.h"

#define LCID_ENGLISH MAKELCID(MAKELANGID(0x09, 0x01), SORT_DEFAULT)

const BYTE NumericValue = 15;
const WCHAR CharValue = W('z');
LPCOLESTR StringValue = W("Abcdefg");
// The reserved field of the DECIMAL struct overlaps with the vt field in the definition of VARIANT.
DECIMAL DecimalValue = { VT_DECIMAL, {{ 0, 0 }}, 0xffffffff, {{0xffffffff, 0xffffffff}} }; 

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Byte(VARIANT value)
{
    if (value.vt != VT_UI1)
    {
        printf("Invalid format. Expected VT_UI1.\n");
        return FALSE;
    }

    return value.cVal == NumericValue ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_SByte(VARIANT value)
{
    if (value.vt != VT_I1)
    {
        printf("Invalid format. Expected VT_I1.\n");
        return FALSE;
    }

    return value.bVal == NumericValue ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Int16(VARIANT value)
{
    if (value.vt != VT_I2)
    {
        printf("Invalid format. Expected VT_I2.\n");
        return FALSE;
    }

    return value.iVal == NumericValue ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_UInt16(VARIANT value)
{
    if (value.vt != VT_UI2)
    {
        printf("Invalid format. Expected VT_UI2.\n");
        return FALSE;
    }

    return value.uiVal == NumericValue ? TRUE : FALSE;
}
extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Int32(VARIANT value)
{
    if (value.vt != VT_I4)
    {
        printf("Invalid format. Expected VT_I4.\n");
        return FALSE;
    }

    return value.lVal == NumericValue ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_UInt32(VARIANT value)
{
    if (value.vt != VT_UI4)
    {
        printf("Invalid format. Expected VT_UI4.\n");
        return FALSE;
    }

    return value.ulVal == NumericValue ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Int64(VARIANT value)
{
    if (value.vt != VT_I8)
    {
        printf("Invalid format. Expected VT_I8.\n");
        return FALSE;
    }

    return value.llVal == NumericValue ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_UInt64(VARIANT value)
{
    if (value.vt != VT_UI8)
    {
        printf("Invalid format. Expected VT_UI8.\n");
        return FALSE;
    }

    return value.ullVal == NumericValue ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Single(VARIANT value)
{
    if (value.vt != VT_R4)
    {
        printf("Invalid format. Expected VT_R4.\n");
        return FALSE;
    }

    return value.fltVal == NumericValue ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Double(VARIANT value)
{
    if (value.vt != VT_R8)
    {
        printf("Invalid format. Expected VT_R8.\n");
        return FALSE;
    }

    return value.dblVal == NumericValue ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Char(VARIANT value)
{
    if (value.vt != VT_UI2)
    {
        printf("Invalid format. Expected VT_UI2.\n");
        return FALSE;
    }

    return value.uiVal == CharValue ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_String(VARIANT value)
{
    static BSTR expected = NULL;

    if (expected == NULL)
    {
        expected = SysAllocString(StringValue);
    }

    if (value.vt != VT_BSTR)
    {
        printf("Invalid format. Expected VT_BSTR.\n");
        return FALSE;
    }

    size_t len = TP_SysStringByteLen(value.bstrVal);

    return len == TP_SysStringByteLen(expected) && memcmp(value.bstrVal, expected, len) == 0;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Object(VARIANT value)
{
    
    if (value.vt != VT_DISPATCH)
    {
        printf("Invalid format. Expected VT_DISPATCH.\n");
        return FALSE;
    }
    

    IDispatch* obj = value.pdispVal;

    if (obj == NULL)
    {
        printf("Marshal_ByValue (Native side) recieved an invalid IDispatch pointer\n");
        return FALSE;
    }

    obj->AddRef();

    obj->Release();

    return TRUE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Missing(VARIANT value)
{
    if (value.vt != VT_ERROR)
    {
        printf("Invalid format. Expected VT_ERROR.\n");
        return FALSE;
    }

    return value.scode == DISP_E_PARAMNOTFOUND ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Empty(VARIANT value)
{
    if (value.vt != VT_EMPTY)
    {
        printf("Invalid format. Expected VT_EMPTY. \n");
        return FALSE;
    }
    
    return TRUE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Boolean(VARIANT value)
{
    if (value.vt != VT_BOOL)
    {
        printf("Invalid format. Expected VT_BOOL.\n");
        return FALSE;
    }

    return value.boolVal == VARIANT_TRUE ? TRUE : FALSE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_DateTime(VARIANT value)
{
    if (value.vt != VT_DATE)
    {
        printf("Invalid format. Expected VT_BYREF.\n");
        return FALSE;
    }

    BSTR str;
    
    // always use the ENGLISH locale so that the string comes out as 11/16/1977 as opposed to 
    // say 16/11/1977 for German locale; otherwise this test would fail on non-ENU locales

    VarBstrFromDate(value.date, LCID_ENGLISH, VAR_FOURDIGITYEARS, &str);

    if(wcscmp(L"11/6/2018", (wchar_t *)str) != 0 )
    {
        wprintf(L"FAILURE! InDATE expected '07/04/2008' but received: %s\n", str);
        return FALSE;
    }

    return TRUE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Decimal(VARIANT value)
{
    if (value.vt != VT_DECIMAL)
    {
        printf("Invalid format. Expected VT_DECIMAL.\n");
        return FALSE;
    }

    return memcmp(&value.decVal, &DecimalValue, sizeof(DECIMAL)) == 0;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Null(VARIANT value)
{
    if (value.vt != VT_NULL)
    {
        printf("Invalid format. Expected VT_NULL. \n");
        return FALSE;
    }
    
    return TRUE;
}

extern "C" BOOL DLL_EXPORT STDMETHODCALLTYPE Marshal_ByValue_Invalid(VARIANT value)
{
    return FALSE;
}

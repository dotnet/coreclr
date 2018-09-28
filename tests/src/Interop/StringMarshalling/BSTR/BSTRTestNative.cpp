// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "platformdefines.h"

WCHAR* strManaged = L("Managed\0String\0");
size_t   lenstrManaged = 14; // the byte length of strManaged

WCHAR* strReturn = L("a\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0");
WCHAR* strerrReturn = L("error");

WCHAR* strNative = L(" Native\0String\0");
size_t lenstrNative = 14; //the byte len of strNative

//Test Method1
extern "C" BSTR ReturnString()
{
    return TP_SysAllocString(strReturn);    
}

extern "C" BSTR ReturnErrorString()
{
    return TP_SysAllocString(strerrReturn);    
}

//Test Method2
extern "C" DLL_EXPORT BSTR Marshal_InOut(/*[In,Out]*/BSTR s)
{
    //Check the Input
    size_t len = TP_SysStringByteLen(s);

    if(memcmp(s,strManaged,lenstrManaged)!=0)
    {
        printf("Error in Function Marshal_InOut(Native Client)\n");        
	printf("Error: Actual: %d, Expected: %d\n",(int32_t) len, (int32_t)lenstrManaged);        
        for(size_t i = 0; i< lenstrManaged;++i)
            putchar(*(((char *)strManaged)+i));               
        
        for(size_t j = 0; j < len; ++j )
            putchar(*(((char *)s) + j));
        return ReturnErrorString();
    }

    //In-Place Change
    memcpy(s, strNative, len);

    //Return
    return ReturnString();
}


extern "C" DLL_EXPORT BSTR Marshal_Out(/*[Out]*/BSTR s)
{
    s = TP_SysAllocString(strNative);
    
    //In-Place Change
    memcpy(s,strNative, lenstrNative);

    //Return
    return ReturnString();
}


extern "C" DLL_EXPORT BSTR MarshalPointer_InOut(/*[in,out]*/BSTR *s)
{    
    //Check the Input
    size_t len = TP_SysStringByteLen(*s);

    if(memcmp(*s,strManaged,lenstrManaged)!=0)
    {
        printf("Error in Function MarshalPointer_InOut\n");
	printf("Error: Expected: %d, Actual: %d", (int32_t)lenstrManaged, (int32_t)len);        
        for(size_t i = 0; i< lenstrManaged;++i)
            putchar(*(((char *)strManaged)+i));
                
        for( size_t j = 0; j < len; ++j)
            putchar(*(((char *)*s) + j));
        
        return ReturnErrorString();
    }

    //Allocate New
    TP_SysFreeString(*s);
    *s = TP_SysAllocString(strNative);
    
    
    //Return
    return ReturnString();
}

extern "C" DLL_EXPORT BSTR MarshalPointer_Out(/*[out]*/ BSTR *s)
{
    *s = TP_SysAllocString(strNative);
    return ReturnString();
}

extern "C" DLL_EXPORT int Writeline(char * pFormat, int i, char c, double d, short s, unsigned u)
{
	int sum = i;
	for (size_t it = 0; it < strlen(pFormat); it++)
	{
		sum += (int)(*pFormat);
	}	
	sum += (int)c;
	sum += (int)d;
	sum += (int)s;
	sum += (int)u;
	return sum;
}


typedef BSTR (__stdcall * Test_DelMarshal_InOut)(/*[in]*/ BSTR s);
extern "C" DLL_EXPORT BOOL __cdecl RPinvoke_DelMarshal_InOut(Test_DelMarshal_InOut d, /*[in]*/ BSTR s)
{
    BSTR str = d(s);
    LPWSTR ret = (LPWSTR)L("Return\0Return\0");    

    size_t lenstr = TP_SysStringByteLen(str);
    size_t lenret = 28;

    if((lenret != lenstr)||(memcmp(str,ret,lenstr)!=0))
    {
        printf("Error in RPinvoke_DelMarshal_InOut, Returned value didn't match\n");
        
        return FALSE;
    }
    
    TP_SysFreeString(str);

    return TRUE;
}

//
// PInvokeDef.cs explicitly declares that RPinvoke_DelMarshalPointer_Out uses STDCALL
//
typedef BSTR (__cdecl * Test_DelMarshalPointer_Out)(/*[out]*/ BSTR * s);
extern "C" DLL_EXPORT BOOL __stdcall RPinvoke_DelMarshalPointer_Out(Test_DelMarshalPointer_Out d)
{
    BSTR str;
    BSTR ret = d(&str);

    LPWSTR changedstr = (LPWSTR)L("Native\0String\0");

    size_t lenstr = TP_SysStringByteLen(str);
    size_t lenchangedstr = 28; // byte length

    if((lenstr != lenchangedstr)||(memcmp(str,changedstr,lenstr)!=0))
    {
        printf("Error in RPinvoke_DelMarshalPointer_Out, Value didn't change\n");
        printf("%d, %d\n", (int32_t)lenchangedstr, (int32_t)lenstr);
        return FALSE;
    }

    LPWSTR expected = (LPWSTR)L("Return\0Return\0");
    size_t lenret = TP_SysStringByteLen(ret);
    size_t lenexpected = 28;

    if((lenret != lenexpected)||(memcmp(ret,expected,lenret)!=0))
    {
        printf("Error in RPinvoke_DelMarshalPointer_Out, Return vaue is different than expected\n");
        return FALSE;
    }

    return TRUE;
}

//
// PInvokeDef.cs explicitly declares that ReverseP_MarshalStrB_InOut uses STDCALL
//
typedef BSTR (__stdcall * Test_Del_MarshalStrB_InOut)(/*[in,out]*/ BSTR s);
extern "C" DLL_EXPORT  BOOL __stdcall ReverseP_MarshalStrB_InOut(Test_Del_MarshalStrB_InOut d, /*[in]*/ BSTR s)
{
    BSTR ret = d((BSTR)s);
    LPWSTR expected = (LPWSTR)L("Return");
    size_t lenret = TP_SysStringByteLen(ret);
    size_t lenexpected = TP_slen(expected) * 2;

    if((lenret != lenexpected)||(memcmp(ret,expected,lenret)!=0))
    {
        printf("Error in ReverseP_MarshalStrB_InOut, Return vaue is different than expected\n");
        return FALSE;
    }

    LPWSTR expectedchange = (LPWSTR)L("m");
    size_t lenstr = TP_SysStringByteLen(s);
    size_t lenexpectedchange = TP_slen(expectedchange) * 2;
    
    if((lenstr != lenexpectedchange)||(memcmp(s,expectedchange,lenstr)!=0))
    {
        printf("Error in ReverseP_MarshalStrB_InOut, Value didn't get change\n");
        return FALSE;
    }
    return TRUE;
}

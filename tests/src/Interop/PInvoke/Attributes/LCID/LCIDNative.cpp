// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <stdio.h>
#include <tchar.h>
#include <xplatform.h>

char* strManaged = "Managed\0String\0";
size_t lenstrManaged = 7; // the length of strManaged

char* strReturn = "a\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";
char* strFalseReturn = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";

char* strNative = " Native\0String\0";
size_t lenstrNative = 7; //the len of strNative

LPSTR ReturnString()
{
    return strReturn;
}

LPSTR ReturnFalseString()
{
    return strFalseReturn;
}

void PrintExpectedAndActual(LPSTR s, size_t len)
{
    //Expected
    printf("Expected:");
    for(size_t i = 0; i< lenstrManaged;++i)
        putchar(*(((char *)strManaged)+i));
    printf("\tThe length of Expected:%d\n",static_cast<int>(lenstrManaged));

    //Actual
    printf("Actual:");
    for(size_t j = 0; j < len; ++j )
        putchar(*(((char *)s) + j));
    printf("\tThe length of Actual:%d\n",static_cast<int>(len));
}

extern "C" DLL_EXPORT LPSTR STDMETHODCALLTYPE MarshalStringBuilder_LCID_As_First_Argument(int lcid, LPSTR s)
{
    printf("LCID:%d\n\n",lcid);

    //Check the Input
    size_t len = strlen(s);
    if((len != lenstrManaged)||(memcmp(s,strManaged,len)!=0))
    {
        printf("Error in Function MarshalStringBuilder_LCID_As_First_Argument(Native Client)\n");
        PrintExpectedAndActual(s, len);
        return ReturnFalseString();
    }

    //In-Place Change
    strncpy_s(s,len + 1,strNative,lenstrNative);

    //Return
    return ReturnString();
}

extern "C" DLL_EXPORT LPSTR STDMETHODCALLTYPE MarshalStringBuilder_LCID_As_Last_Argument_SetLastError(LPSTR s,int lcid)
{	
    //Check the Input
    size_t len = strlen(s);
    if((len != lenstrManaged)||(memcmp(s,strManaged,len)!=0))
    {
        printf("Error in Function MarshalStringBuilder_LCID_As_Last_Argument_SetLastError(Native Client)\n");
        PrintExpectedAndActual(s, len);
        return ReturnFalseString();
    }

    //In-Place Change
    strncpy_s(s,len + 1,strNative,lenstrNative);

    SetLastError(1090);
    //Return
    return ReturnString();
}

extern "C" DLL_EXPORT HRESULT STDMETHODCALLTYPE MarshalStringBuilder_LCID_PreserveSig_SetLastError(LPSTR s, int lcid, LPSTR * retVal)
{
    //Check the Input
    size_t len = strlen(s);
    if((len != lenstrManaged)||(memcmp(s,strManaged,len)!=0))
    {
        printf("Error in Function MarshalStringBuilder_LCID_PreserveSig_SetLastError\n");
        PrintExpectedAndActual(s, len);
        *retVal = ReturnFalseString();

        return S_FALSE;
    }

    //In-Place Change
    strncpy_s(s,len + 1,strNative,lenstrNative);

    //Set the error code.
    SetLastError(1090);

    *retVal = ReturnString();

    return S_OK;
}

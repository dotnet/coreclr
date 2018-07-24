// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <stdio.h>
#include <windows.h>
#include <tchar.h>
#include <xplatform.h>

char* strManaged = "Managed\0String\0";
int   lenstrManaged = 7; // the length of strManaged

char* strReturn = "a\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";
char* strFalseReturn = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";

char* strNative = " Native\0String\0";
int lenstrNative = 7; //the len of strNative

//Test Method1

extern "C" LPSTR ReturnString()
{
    size_t lenstrReturn = strlen(strReturn);
    LPSTR ret = (LPSTR)(CoTaskMemAlloc(sizeof(char)*(lenstrReturn+1)));
    memset(ret,'\0',lenstrReturn+1);
    strncpy_s(ret,lenstrReturn + 1,strReturn,1);
    return ret;
}

extern "C" LPSTR ReturnFalseString()
{
    size_t lenstrFalseReturn = strlen(strFalseReturn);
    LPSTR ret = (LPSTR)(CoTaskMemAlloc(sizeof(char)*(lenstrFalseReturn+1)));
    memset(ret,'\0',lenstrFalseReturn+1);
    strncpy_s(ret,lenstrFalseReturn + 1,strFalseReturn,1);
    return ret;
}

//Test Method2
extern "C" DLL_EXPORT LPSTR Marshal_InOut1(int lcid, LPSTR s)
{
    printf("LCID:%d\n\n",lcid);

    //Check the Input
    size_t len = strlen(s);
    if((len != lenstrManaged)||(memcmp(s,strManaged,len)!=0))
    {
        printf("Error in Function Marshal_InOut(Native Client)\n");

        //Expected
        printf("Expected:");
        for(int i = 0; i< lenstrManaged;++i)
            putchar(*(((char *)strManaged)+i));
        printf("\tThe length of Expected:%d\n",lenstrManaged);

        //Actual
        printf("Actual:");
        for( int j = 0; j < len; ++j )
            putchar(*(((char *)s) + j));
        printf("\tThe length of Actual:%d\n",len);
        return ReturnFalseString();
    }

    //In-Place Change
    strncpy_s(s,len + 1,strNative,lenstrNative);

    //Return
    return ReturnString();
}

extern "C" DLL_EXPORT LPSTR Marshal_InOut2(LPSTR s,int lcid)
{	
    //Check the Input
    size_t len = strlen(s);
    if((len != lenstrManaged)||(memcmp(s,strManaged,len)!=0))
    {
        printf("Error in Function Marshal_InOut(Native Client)\n");

        //Expected
        printf("Expected:");
        for(int i = 0; i< lenstrManaged;++i)
            putchar(*(((char *)strManaged)+i));
        printf("\tThe length of Expected:%d\n",lenstrManaged);

        //Actual
        printf("Actual:");
        for( int j = 0; j < len; ++j )
            putchar(*(((char *)s) + j));
        printf("\tThe length of Actual:%d\n",len);
        return ReturnFalseString();
    }

    //In-Place Change
    strncpy_s(s,len + 1,strNative,lenstrNative);

    SetLastError(1090);
    //Return
    return ReturnString();
}

extern "C" DLL_EXPORT HRESULT WINAPI Marshal_InOut4(LPSTR s, int lcid, LPSTR * retVal)
{
    //Check the Input
    size_t len = strlen(s);
    if((len != lenstrManaged)||(memcmp(s,strManaged,len)!=0))
    {
        printf("Error in Function MarshalPointer_InOut\n");

        //Expected
        printf("Expected:");
        for(int i = 0; i< lenstrManaged;++i)
            putchar(*(((char *)strManaged)+i));
        printf("\tThe length of Expected:%d\n",lenstrManaged);

        //Actual
        printf("Actual:");
        for( int j = 0; j < len; ++j)
            putchar(*(((char *)s) + j));
        printf("\tThe length of Actual:%d\n",len);

        size_t lenstrFalseReturn = strlen(ReturnFalseString());
        *retVal = (LPSTR)CoTaskMemAlloc(sizeof(char)*(lenstrFalseReturn+1));
        memset(*retVal,'\0',lenstrFalseReturn+1);
        strncpy_s(*retVal,lenstrFalseReturn,ReturnFalseString(),lenstrFalseReturn);

        return S_FALSE;
    }

    //Allocate New
    strncpy_s(s,len + 1,strNative,lenstrNative);

    //Set the error code.
    SetLastError(1090);

    size_t lenstrReturn = strlen(strReturn);
    *retVal = (LPSTR)(CoTaskMemAlloc(sizeof(char)*(lenstrReturn+1)));
    memset(*retVal,'\0',lenstrReturn+1);
    strncpy_s(*retVal,lenstrReturn + 1,strReturn,lenstrReturn);

    return S_OK;
}











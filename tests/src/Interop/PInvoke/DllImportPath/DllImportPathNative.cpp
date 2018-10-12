// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <stdio.h>
#include <xplatform.h>

LPWSTR strManaged = L"Managed\0String\0";
size_t lenstrManaged = 7; // the length of strManaged

LPWSTR strNative = L" Native\0String\0";
size_t lenstrNative = 7; //the len of strNative

extern "C" DLL_EXPORT bool STDMETHODCALLTYPE MarshalStringPointer_InOut(/*[in,out]*/LPWSTR *s)
{
    //Check the Input
    size_t len = wcslen(*s);
    if((len != lenstrManaged)||(wcscmp(*s,strManaged)!=0))
    {
        printf("Error in Function MarshalStringPointer_InOut\n");

        //Expected
        printf("Expected:");
        wprintf_s(L"%s",strManaged);
        printf("\tThe length of Expected:%d\n",static_cast<int>(lenstrManaged));

        //Actual
        printf("Actual:");
        wprintf_s(L"%s",*s);
        printf("\tThe length of Actual:%d\n",static_cast<int>(len));

        return false;
    }

    //Allocate New
    CoTaskMemFree(*s);

    //Alloc New
    size_t length = lenstrNative + 1;
    *s = (LPWSTR)CoTaskMemAlloc(length * sizeof(WCHAR));
    memset(*s,'\0',length * sizeof(WCHAR));
    wcsncpy_s(*s,length,strNative,lenstrNative);

    //Return
    return true;
}

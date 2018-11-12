// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>


struct test1 {
    LONGLONG a;
    LONGLONG b;
};

extern "C" __declspec (dllexport) LONGLONG __stdcall PassLayout(test1* i) {
    printf("PassLayout: i->a  = %I64d\n", i->a);
    printf("PassLayout: i->b = %I64d\n", i->b);
    return i->b;
}


struct Mix
{
    int * intArray;
};


extern "C" __declspec (dllexport) BOOL __stdcall PassUnicodeStr(LPCWSTR str)
{
	return (__int16)str[0] == 0x0030 && (__int16)str[1] == 0x2075 && (__int16)str[2] == 0x7777;		
}
extern "C" __declspec (dllexport) BOOL __stdcall PassAnsiStr(LPCSTR str , BOOL isIncludeUnMappableChar)
{
	if(isIncludeUnMappableChar)
		return (__int8)str[0] == 0x30 && (__int8)str[1] == 0x3f && (__int8)str[2] == 0x3f;
	else
		return (__int8)str[0] == 0x30 && (__int8)str[1] == 0x35 && (__int8)str[2] == 0x0A;
}

extern "C" __declspec (dllexport) BOOL __stdcall PassUnicodeStrbd(LPCWSTR str)
{
	return (__int16)str[0] == 0x0030 && (__int16)str[1] == 0x2075 && (__int16)str[2] == 0x7777;		
}
extern "C" __declspec (dllexport) BOOL __stdcall PassAnsiStrbd(LPCSTR str , BOOL isIncludeUnMappableChar)
{
	if(isIncludeUnMappableChar)
		return (__int8)str[0] == 0x30 && (__int8)str[1] == 0x3f && (__int8)str[2] == 0x3f;
	else
		return (__int8)str[0] == 0x30 && (__int8)str[1] == 0x35 && (__int8)str[2] == 0x0A;
}

extern "C" __declspec (dllexport) BOOL __stdcall PassUnicodeCharArray(WCHAR CharArray_In [], WCHAR CharArray_InOut [], WCHAR CharArray_Out [])
{
	BOOL ret = FALSE;
	ret = (__int16)CharArray_In[0] == 0x0030 && (__int16)CharArray_In[1] == 0x2075 && (__int16)CharArray_In[2] == 0x7777 
		&& (__int16)CharArray_InOut[0] == 0x0030 && (__int16)CharArray_InOut[1] == 0x2075 && (__int16)CharArray_InOut[2] == 0x7777;

	// revert the string for passing back
	CHAR temp = CharArray_InOut[0]; 

	CharArray_InOut[0] = CharArray_InOut[2];
	CharArray_Out[0] = CharArray_InOut[2];
	CharArray_Out[1] = CharArray_InOut[1];
	CharArray_InOut[2] = temp;
	CharArray_Out[2] = temp;
	return ret;
}
extern "C" __declspec (dllexport) BOOL __stdcall PassAnsiCharArray(CHAR CharArray_In [], CHAR CharArray_InOut [], CHAR CharArray_Out [] ,
        BOOL isIncludeUnMappableChar)
{
	BOOL ret = FALSE;
	if(isIncludeUnMappableChar)
		ret = (__int8)CharArray_In[0] == 0x30 && (__int8)CharArray_In[1] == 0x3f && (__int8)CharArray_In[2] == 0x3f 
			&& (__int8)CharArray_InOut[0] == 0x30 && (__int8)CharArray_InOut[1] == 0x3f && (__int8)CharArray_InOut[2] == 0x3f;
	else
		ret = (__int8)CharArray_In[0] == 0x30 && (__int8)CharArray_In[1] == 0x35 && (__int8)CharArray_In[2] == 0x0A 
			&& (__int8)CharArray_InOut[0] == 0x30 && (__int8)CharArray_InOut[1] == 0x35 && (__int8)CharArray_InOut[2] == 0x0A;

	// revert the string for passing back
	CHAR temp = CharArray_InOut[0]; 

	CharArray_InOut[0] = CharArray_InOut[2];
	CharArray_Out[0] = CharArray_InOut[2];
	CharArray_Out[1] = CharArray_InOut[1];
	CharArray_InOut[2] = temp;
	CharArray_Out[2] = temp;

	return ret;
}

extern "C" __declspec (dllexport) BOOL __stdcall PassArraySbyte(
			BYTE sbyteArray[], BYTE sbyteArray_In[], BYTE sbyteArray_InOut[], BYTE sbyteArray_Out[], int len){
		BYTE bs [3] = {-1, 0 ,1};
		for(int i = 0; i < len; i++)
		{
			if(sbyteArray[i] != bs[i] || sbyteArray_In[i] != bs[i] || sbyteArray_InOut[i] != bs[i])
			{
				printf("Not correct pass in paremeter in PassArraySbyte\n");
				return FALSE;
			}
			sbyteArray_InOut[i] = 10 + bs[i];
			sbyteArray_Out[i] = 10 + bs[i];
		}
		return TRUE;    
}


extern "C" __declspec (dllexport) BOOL __stdcall PassArrayByte(	
		BYTE byteArray[], BYTE byteArray_In[], BYTE byteArray_InOut[], BYTE byteArray_Out[], int len){
		BYTE arrs[3] = {0, 1 ,2};
		for(int i = 0; i < len; i++)
		{
			if(byteArray[i] != arrs[i] || byteArray_In[i] != arrs[i] || byteArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayByte\n");
				return FALSE;
			}
			byteArray_InOut[i] = 10 + arrs[i];
			byteArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;    
}
    
extern "C" __declspec (dllexport) BOOL __stdcall PassArrayShort(	
		SHORT shortArray[], SHORT shortArray_In[], SHORT shortArray_InOut[], SHORT shortArray_Out[], int len){
		SHORT arrs[3] = {-1, 0 ,1};
		for(int i = 0; i < len; i++)
		{
			if(shortArray[i] != arrs[i] || shortArray_In[i] != arrs[i] || shortArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayShort\n");
				return FALSE;
			}
			shortArray_InOut[i] = 10 + arrs[i];
			shortArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;    
}
    
extern "C" __declspec (dllexport) BOOL __stdcall PassArrayUshort(	
		USHORT ushortArray[], USHORT ushortArray_In[], USHORT ushortArray_InOut[], USHORT ushortArray_Out[], int len){
		USHORT arrs[3] = {0, 1 ,2};
		for(int i = 0; i < len; i++)
		{
			if(ushortArray[i] != arrs[i] || ushortArray_In[i] != arrs[i] || ushortArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayUshort\n");
				return FALSE;
			}
			ushortArray_InOut[i] = 10 + arrs[i];
			ushortArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;    
}

extern "C" __declspec (dllexport) BOOL __stdcall PassArrayInt(	
		int IntArray[], int IntArray_In[], int IntArray_InOut[], int IntArray_Out[], int len){
		int arrs[3] = {0, 1 ,2};
		for(int i = 0; i < len; i++)
		{
			if(IntArray[i] != i || IntArray_In[i] != i || IntArray_InOut[i] != i)
			{
				printf("Not correct pass in paremeter in PassArrayInt\n");
				return FALSE;
			}
			IntArray_InOut[i] = 10 + arrs[i];
			IntArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;    
}


extern "C" __declspec (dllexport) BOOL __stdcall PassArrayUint(	
		UINT uintArray[], UINT uintArray_In[], UINT uintArray_InOut[], UINT uintArray_Out[], int len){
		UINT arrs[3] = {0, 1 ,2};
		for(int i = 0; i < len; i++)
		{
			if(uintArray[i] != arrs[i] || uintArray_In[i] != arrs[i] || uintArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayUint\n");
				return FALSE;
			}
			uintArray_InOut[i] = 10 + arrs[i];
			uintArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;    
}

extern "C" __declspec (dllexport) BOOL __stdcall PassArrayLong(	
		__int64 longArray[], __int64 longArray_In[], __int64 longArray_InOut[], __int64 longArray_Out[], int len){
		__int64 arrs[3] = {0, 1 ,2};
		for(int i = 0; i < len; i++)
		{
			if(longArray[i] != arrs[i] || longArray_In[i] != arrs[i] || longArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayLong\n");
				return FALSE;
			}
			longArray_InOut[i] = 10 + arrs[i];
			longArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;    
}

extern "C" __declspec (dllexport) BOOL __stdcall PassArrayUlong(	
	__int64 ulongArray[], __int64 ulongArray_In[], __int64 ulongArray_InOut[], 
	__int64 ulongArray_Out[], int len){
		__int64 arrs[3] = {0, 1 ,2};
		for(int i = 0; i < len; i++)
		{
			if(ulongArray[i] != arrs[i] || ulongArray_In[i] != arrs[i] || ulongArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayUlong\n");
				return FALSE;
			}
			ulongArray_InOut[i] = 10 + arrs[i];
			ulongArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;  
}

extern "C" __declspec (dllexport) BOOL __stdcall PassArraySingle(	
	float singleArray[], float singleArray_In[], float singleArray_InOut[], 
	float singleArray_Out[], int len){
		float arrs[3] = {0, 1 ,2};
		for(int i = 0; i < len; i++)
		{
			if(singleArray[i] != arrs[i] || singleArray_In[i] != arrs[i] || singleArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArraySingle\n");
				return FALSE;
			}
			singleArray_InOut[i] = 10 + arrs[i];
			singleArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;  
}

extern "C" __declspec (dllexport) BOOL __stdcall PassArrayDouble(	
	double doubleArray[], double doubleArray_In[], double doubleArray_InOut[], double doubleArray_Out[], int len){
		double arrs[3] = {0.0, 1.1 ,2.2};
		for(int i = 0; i < len; i++)
		{
			if(doubleArray[i] != arrs[i] || doubleArray_In[i] != arrs[i] || doubleArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayDouble\n");
				return FALSE;
			}
			doubleArray_InOut[i] = 10 + arrs[i];
			doubleArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;  
}

extern "C" __declspec (dllexport) BOOL __stdcall PassArrayChar(	
	CHAR charArray[], CHAR charArray_In[], CHAR charArray_InOut[], CHAR charArray_Out[], int len){
		CHAR *arrs = "abc";
		for(int i = 0; i < len; i++)
		{
			if(charArray[i] != arrs[i] || charArray_In[i] != arrs[i] || charArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayChar\n");
				return FALSE;
			}			
		}
		
		charArray_InOut[0] = 100;
		charArray_Out[0] = 100;
		charArray_InOut[1] = 101;
		charArray_Out[1] = 101;
		charArray_InOut[2] = 102;
		charArray_Out[2] = 102;

		return TRUE;  
}

extern "C" __declspec (dllexport) BOOL __stdcall PassArrayBool(	
	BOOL boolArray[], BOOL boolArray_In[], BOOL boolArray_InOut[], BOOL boolArray_Out[], int len){
		BOOL arrs[3] = {TRUE, FALSE, FALSE};
		for(int i = 0; i < len; i++)
		{
			if(boolArray[i] != arrs[i] || boolArray_In[i] != arrs[i] || boolArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayBool\n");
				return FALSE;
			}			
		}
		
		boolArray_InOut[0] = FALSE;
		boolArray_Out[0] = FALSE;
		boolArray_InOut[1] = TRUE;
		boolArray_Out[1] = TRUE;
		boolArray_InOut[2] = TRUE;
		boolArray_Out[2] = TRUE;

		return TRUE;  
}

extern "C" __declspec (dllexport) BOOL __stdcall PassArrayIntPtr(	
	INT_PTR intPtrArray[], INT_PTR intPtrArray_In[], INT_PTR intPtrArray_InOut[], INT_PTR intPtrArray_Out[], int len){
		INT_PTR arrs[3] = {0, 1, 2};
		for(int i = 0; i < len; i++)
		{
			if(intPtrArray[i] != arrs[i] || intPtrArray_In[i] != arrs[i] || intPtrArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayIntPtr\n");
				return FALSE;
			}
			intPtrArray_InOut[i] = 10 + arrs[i];
			intPtrArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;  
}

extern "C" __declspec (dllexport) BOOL __stdcall PassArrayUIntPtr(	
	UINT_PTR uIntPtrArray[], UINT_PTR uIntPtrArray_In[], UINT_PTR uIntPtrArray_InOut[], UINT_PTR uIntPtrArray_Out[], int len){
		UINT_PTR arrs[3] = {0, 1, 2};
		for(int i = 0; i < len; i++)
		{
			if(uIntPtrArray[i] != arrs[i] || uIntPtrArray_In[i] != arrs[i] || uIntPtrArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayUIntPtr\n");
				return FALSE;
			}
			uIntPtrArray_InOut[i] = 10 + arrs[i];
			uIntPtrArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;  
}

extern "C" __declspec (dllexport) BOOL __stdcall PassMixStruct(Mix mix){
		return TRUE;  
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>

struct test1 {
    LONGLONG a;
    LONGLONG b;
};

extern "C" DLL_EXPORT LONGLONG STDMETHODCALLTYPE PassLayout(test1* i) {
    printf("PassLayout: i->a  = %I64d\n", i->a);
    printf("PassLayout: i->b = %I64d\n", i->b);
    return i->b;
}


struct AsAnyField
{
    int * intArray;
};


extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassUnicodeStr(LPCWSTR str)
{
	return (SHORT)str[0] == 0x0030 && (SHORT)str[1] == 0x7777 && (SHORT)str[2] == 0x000A;		
}
extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassAnsiStr(LPCSTR str , BOOL isIncludeUnMappableChar)
{
	if(isIncludeUnMappableChar)
		return (BYTE)str[0] == 0x30 && (BYTE)str[1] == 0x3f && (BYTE)str[2] == 0x0A;
	else
		return (BYTE)str[0] == 0x30 && (BYTE)str[1] == 0x35 && (BYTE)str[2] == 0x0A;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassUnicodeStrbd(LPCWSTR str)
{
	return (SHORT)str[0] == 0x0030 && (SHORT)str[1] == 0x7777 && (SHORT)str[2] == 0x000A;			
}
extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassAnsiStrbd(LPCSTR str , BOOL isIncludeUnMappableChar)
{
	if(isIncludeUnMappableChar)
		return (BYTE)str[0] == 0x30 && (BYTE)str[1] == 0x3f && (BYTE)str[2] == 0x0A;
	else
		return (BYTE)str[0] == 0x30 && (BYTE)str[1] == 0x35 && (BYTE)str[2] == 0x0A;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassUnicodeCharArray(WCHAR CharArray_In [], WCHAR CharArray_InOut [], WCHAR CharArray_Out [])
{
	BOOL ret = FALSE;
	ret = (SHORT)CharArray_In[0] == 0x0030 && (SHORT)CharArray_In[1] == 0x7777 && (SHORT)CharArray_In[2] == 0x000A 
		&& (SHORT)CharArray_InOut[0] == 0x0030 && (SHORT)CharArray_InOut[1] == 0x7777 && (SHORT)CharArray_InOut[2] == 0x000A ;

	// revese the string for passing back
	WCHAR temp = CharArray_InOut[0]; 

	CharArray_InOut[0] = CharArray_InOut[2];
	CharArray_Out[0] = CharArray_InOut[2];
	CharArray_Out[1] = CharArray_InOut[1];
	CharArray_InOut[2] = temp;
	CharArray_Out[2] = temp;
	return ret;
}
extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassAnsiCharArray(CHAR CharArray_In [], CHAR CharArray_InOut [], CHAR CharArray_Out [] ,
        BOOL isIncludeUnMappableChar)
{
	BOOL ret = FALSE;
	if(isIncludeUnMappableChar)
		ret = (BYTE)CharArray_In[0] == 0x30 && (BYTE)CharArray_In[1] == 0x3f && (BYTE)CharArray_In[2] == 0x0A 
			&& (BYTE)CharArray_InOut[0] == 0x30 && (BYTE)CharArray_InOut[1] == 0x3f && (BYTE)CharArray_InOut[2] == 0x0A;
	else
		ret = (BYTE)CharArray_In[0] == 0x30 && (BYTE)CharArray_In[1] == 0x35 && (BYTE)CharArray_In[2] == 0x0A 
			&& (BYTE)CharArray_InOut[0] == 0x30 && (BYTE)CharArray_InOut[1] == 0x35 && (BYTE)CharArray_InOut[2] == 0x0A;

	// reverse the string for passing back
	CHAR temp = CharArray_InOut[0]; 

	CharArray_InOut[0] = CharArray_InOut[2];
	CharArray_Out[0] = CharArray_InOut[2];
	CharArray_Out[1] = CharArray_InOut[1];
	CharArray_InOut[2] = temp;
	CharArray_Out[2] = temp;

	return ret;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArraySbyte(
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


extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayByte(	
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
    
extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayShort(	
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
    
extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayUshort(	
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

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayInt(	
		int IntArray[], int IntArray_In[], int IntArray_InOut[], int IntArray_Out[], int len){
		int arrs[3] = {0, 1 ,2};
		for(int i = 0; i < len; i++)
		{
			if(IntArray[i] != arrs[i] || IntArray_In[i] != arrs[i] || IntArray_InOut[i] != arrs[i])
			{
				printf("Not correct pass in paremeter in PassArrayInt\n");
				return FALSE;
			}
			IntArray_InOut[i] = 10 + arrs[i];
			IntArray_Out[i] = 10 + arrs[i];
		}
		return TRUE;    
}


extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayUint(	
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

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayLong(	
		LONGLONG longArray[], LONGLONG longArray_In[], LONGLONG longArray_InOut[], LONGLONG longArray_Out[], int len){
		LONGLONG arrs[3] = {0, 1 ,2};
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

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayUlong(	
	LONGLONG ulongArray[], LONGLONG ulongArray_In[], LONGLONG ulongArray_InOut[], 
	LONGLONG ulongArray_Out[], int len){
		LONGLONG arrs[3] = {0, 1 ,2};
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

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArraySingle(	
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

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayDouble(	
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

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayChar(	
	CHAR charArray[], CHAR charArray_In[], CHAR charArray_InOut[], CHAR charArray_Out[], int len){
		const CHAR *arrs = "abc";
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

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayBool(	
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

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayIntPtr(	
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

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassArrayUIntPtr(	
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

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE PassMixStruct(AsAnyField mix){
		return TRUE;  
}

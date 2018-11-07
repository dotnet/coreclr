// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include "platformdefines.h"

//--------------------------------
//	Start of SafeArray calls
//--------------------------------

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE SafeArray_In(SAFEARRAY* psa)
{
	long i, lUbound, lLbound;
	int  *pInt; //will point to the data
	HRESULT hr;

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(psa, (void **)&pInt);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayAccessData call in SafeArray_In failed!\n");
		return FALSE;
	}

	//get upperbound
	hr = SafeArrayGetUBound(psa, 1, &lUbound);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayGetUBound call in SafeArray_In failed!\n");
		return FALSE;
	}

	//check upperbound
	if(lUbound != 255) //since num of elems = lUBound - lLBound + 1
	{
		printf("\t\tlUbound not as expected in SafeArray_In!\n");
		printf("\t\t\tlUbound = %d",lUbound);
		return FALSE;
	}

	//get lowerbound
	hr = SafeArrayGetLBound(psa, 1, &lLbound);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayGetLBound call in SafeArray_In failed!\n");
		return FALSE;
	}

	//check lowerbound
	if(lLbound != 0) 
	{
		printf("\t\tlLbound not as expected in SafeArray_In!\n");
		printf("\t\t\tlLbound = %d",lLbound);
		return FALSE;
	}

	//check dimension
	if(SafeArrayGetDim(psa) != 1)
	{
		printf("\t\tDimension not as expected in SafeArray_In!\n");
		return FALSE;
	}

	//check element size
	if(SafeArrayGetElemsize(psa) != 4) //size of each element should be 4 bytes
	{
		printf("\t\tElement size not as expected in SafeArray_In!\n");
		return FALSE;
	}

	//validate data
	for(i = 0; i <= lUbound; i++)
	{
		if(pInt[i] != i)
		{
			printf("\t\tData not as expected in SafeArray_In!\n");
			return FALSE;
		}
	}

	SafeArrayUnaccessData(psa);
	return TRUE;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE SafeArray_InOut(SAFEARRAY* psa)
{
	long i, lUbound, lLbound;
	int  *pInt; //will point to the data
	HRESULT hr;

	/************************************************
	* Validate the safearray 
	************************************************/

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(psa, (void **)&pInt);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayAccessData call in SafeArray_InOut failed!\n");
		return FALSE;
	}

	//get upperbound
	hr = SafeArrayGetUBound(psa, 1, &lUbound);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayGetUBound call in SafeArray_InOut failed!\n");
		return FALSE;
	}

	//check upperbound
	if(lUbound != 255) //since num of elems = lUBound - lLBound + 1
	{
		printf("\t\tlUbound not as expected in SafeArray_InOut!\n");
		printf("\t\t\tlUbound = %d",lUbound);
		return FALSE;
	}

	//get lowerbound
	hr = SafeArrayGetLBound(psa, 1, &lLbound);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayGetLBound call in SafeArray_InOut failed!\n");
		return FALSE;
	}

	//check lowerbound
	if(lLbound != 0) 
	{
		printf("\t\tlLbound not as expected in SafeArray_InOut!\n");
		printf("\t\t\tlLbound = %d",lLbound);
		return FALSE;
	}

	//check dimension
	if(SafeArrayGetDim(psa) != 1)
	{
		printf("\t\tDimension not as expected in SafeArray_InOut!\n");
		return FALSE;
	}

	//check element size
	if(SafeArrayGetElemsize(psa) != 4) //size of each element should be 4 bytes
	{
		printf("\t\tElement size not as expected in SafeArray_InOut!\n");
		return FALSE;
	}

	//validate data
	for(i = 0; i <= lUbound; i++)
	{
		if(pInt[i] != i)
		{
			printf("\t\tData not as expected in SafeArray_InOut!\n");
			return FALSE;
		}
	}

	/************************************************
	* Change the safearray 
	************************************************/

	//reverse data
	for(i = 0; i <= lUbound; i++)	
		pInt[i] = lUbound - i;

	SafeArrayUnaccessData(psa);
	return TRUE;
}

extern "C" DLL_EXPORT SAFEARRAY* _stdcall SafeArray_Ret()
{
	SAFEARRAY * psa;
	int * pInt;
    HRESULT hr;

	psa = SafeArrayCreateVector(VT_I4, 0, 1024); //data is array of ints; size = 1024 elements
	if(psa == NULL)
	{
		printf("\t\tSafeArrayCreateVector call failed!\n");
		return NULL;
	}
	else
	{
		// Get a pointer to the elements of the array.
		hr = SafeArrayAccessData(psa, (void **)&pInt);
		if(FAILED(hr))
		{
			printf("\t\tSafeArrayAccessData call in SafeArray_Ret failed!\n");
			return NULL;
		}
		for(int i = 0; i < 1024; i++)		
			pInt[i] = -1; //each element set to -1
	}

	SafeArrayUnaccessData(psa);
	return psa;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE SafeArray_InByRef(SAFEARRAY** ppsa)
{
	long i, lUbound, lLbound;
	int  *pInt; //will point to the data
	HRESULT hr;

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(*ppsa, (void **)&pInt);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayAccessData call in SafeArray_InByRef failed!\n");
		return FALSE;
	}

	//get upperbound
	hr = SafeArrayGetUBound(*ppsa, 1, &lUbound);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayGetUBound call in SafeArray_InByRef failed!\n");
		return FALSE;
	}

	//check upperbound
	if(lUbound != 255) //since num of elems = lUBound - lLBound + 1
	{
		printf("\t\tlUbound not as expected in SafeArray_InByRef!\n");
		printf("\t\t\tlUbound = %d",lUbound);
		return FALSE;
	}

	//get lowerbound
	hr = SafeArrayGetLBound(*ppsa, 1, &lLbound);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayGetLBound call in SafeArray_InByRef failed!\n");
		return FALSE;
	}

	//check lowerbound
	if(lLbound != 0) 
	{
		printf("\t\tlLbound not as expected in SafeArray_InByRef!\n");
		printf("\t\t\tlLbound = %d",lLbound);
		return FALSE;
	}

	//check dimension
	if(SafeArrayGetDim(*ppsa) != 1)
	{
		printf("\t\tDimension not as expected in SafeArray_InByRef!\n");
		return FALSE;
	}

	//check element size
	if(SafeArrayGetElemsize(*ppsa) != 4) //size of each element should be 4 bytes
	{
		printf("\t\tElement size not as expected in SafeArray_InByRef!\n");
		return FALSE;
	}

	//validate data
	for(i = 0; i <= lUbound; i++)
	{
		//printf("\t\t\ti = %d ; pInt[i] = %d\n", i, pInt[i]);
		if(pInt[i] != i)
		{
			printf("\t\tData not as expected in SafeArray_InByRef!\n");
			return FALSE;
		}
	}

	SafeArrayUnaccessData(*ppsa);
	return TRUE;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE SafeArray_InOutByRef(SAFEARRAY** ppsa)
{
	long i, lUbound, lLbound;
	int  *pInt; //will point to the data
	HRESULT hr;
	
	// Validate the safearray 

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(*ppsa, (void **)&pInt);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayAccessData call in SafeArray_InOutByRef failed!\n");
		return FALSE;
	}

	//get upperbound
	hr = SafeArrayGetUBound(*ppsa, 1, &lUbound);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayGetUBound call in SafeArray_InOutByRef failed!\n");
		return FALSE;
	}

	//check upperbound
	if(lUbound != 255) //since num of elems = lUBound - lLBound + 1
	{
		printf("\t\tlUbound not as expected in SafeArray_InOutByRef!\n");
		printf("\t\t\tlUbound = %d",lUbound);
		return FALSE;
	}

	//get lowerbound
	hr = SafeArrayGetLBound(*ppsa, 1, &lLbound);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayGetLBound call in SafeArray_InOutByRef failed!\n");
		return FALSE;
	}

	//check lowerbound
	if(lLbound != 0) 
	{
		printf("\t\tlLbound not as expected in SafeArray_InOutByRef!\n");
		printf("\t\t\tlLbound = %d",lLbound);
		return FALSE;
	}

	//check dimension
	if(SafeArrayGetDim(*ppsa) != 1)
	{
		printf("\t\tDimension not as expected in SafeArray_InOutByRef!\n");
		return FALSE;
	}

	//check element size
	if(SafeArrayGetElemsize(*ppsa) != 4) //size of each element should be 4 bytes
	{
		printf("\t\tElement size not as expected in SafeArray_InOut!\n");
		return FALSE;
	}

	//validate data
	for(i = 0; i <= lUbound; i++)
	{
		if(pInt[i] != i)
		{
			printf("\t\tData not as expected in SafeArray_InOut!\n");
			return FALSE;
		}
	}

	// Change the safearray 

	//reverse data
	for(i = 0; i <= lUbound; i++)	
		pInt[i] = lUbound - i;

	SafeArrayUnaccessData(*ppsa);
	return TRUE;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE SafeArrayWithOutAttribute(SAFEARRAY* psa)
{
	long i, lUbound;
	int  *pInt; //will point to the data
	HRESULT hr;

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(psa, (void **)&pInt);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayAccessData call in SafeArray_InOut failed!\n");
		return FALSE;
	}

	//get upperbound
	hr = SafeArrayGetUBound(psa, 1, &lUbound);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayGetUBound call in SafeArray_InOut failed!\n");
		return FALSE;
	}

	//check upperbound
	if(lUbound != 255) //since num of elems = lUBound - lLBound + 1
	{
		printf("\t\tlUbound not as expected in SafeArray_InOut!\n");
		printf("\t\t\tlUbound = %d",lUbound);
		return FALSE;
	}

	//check dimension
	if(SafeArrayGetDim(psa) != 1)
	{
		printf("\t\tDimension not as expected in SafeArray_InOut!\n");
		return FALSE;
	}

	/************************************************
	* Change the safearray 
	************************************************/

	//change the elements to -1
	for(i = 0; i <= lUbound; i++)	
		pInt[i] = -1;

	SafeArrayUnaccessData(psa);
	return TRUE;
}

extern "C" DLL_EXPORT SAFEARRAY* _stdcall SafeArray_Ret_MismatchRank()
{
	SAFEARRAY * psa;

	const int rank = 2;
	SAFEARRAYBOUND rgsabound[rank];
	for(int i = 0; i < rank; i++)
	{
		rgsabound[i].lLbound = 0;
		rgsabound[i].cElements = 5;
	}
	psa = SafeArrayCreate(VT_I4, rank, rgsabound);
	if(psa == NULL)
	{
		printf("\t\tSafeArrayCreate call failed!\n");
		return NULL;
	}
	
	return psa;
}

extern "C" DLL_EXPORT SAFEARRAY* _stdcall SafeArray_Ret_InvalidLBound()
{
	SAFEARRAY * psa;

	int rank = 1;
	SAFEARRAYBOUND rgsabound[1];
	rgsabound[0].lLbound = 99;
	rgsabound[0].cElements = 5;

	psa = SafeArrayCreate(VT_I4, rank, rgsabound);
	if(psa == NULL)
	{
		printf("\t\tSafeArrayCreate call failed!\n");
		return NULL;
	}
	
	return psa;
}

struct StructWithSA
{
	int i32;
	SAFEARRAY* ptoArrOfInt32s;
};

const int NumArrElements = 256;

StructWithSA* NewStructWithSA()
{
	int* pInt;
	HRESULT hr;
	StructWithSA* ps = (StructWithSA*)CoreClrAlloc(sizeof(StructWithSA));
	(*ps).i32 = 77;

	//create new SAFEARRAY; 1 dimension
	(*ps).ptoArrOfInt32s = SafeArrayCreateVector(VT_I4, 0, NumArrElements);  
	if((*ps).ptoArrOfInt32s == NULL)
	{
		printf("\t\tSafeArrayCreateVector call failed!\n");
		exit(1);
	}
	else
	{
		// Get a pointer to the elements of the array.
		hr = SafeArrayAccessData((*ps).ptoArrOfInt32s, (void**)&pInt);
		if(FAILED(hr))
		{
			printf("\t\tSafeArrayAccessData call in NewStructWithSA failed!\n");
			exit(1);
		}
		for(int i = 0; i < NumArrElements; i++)		
			pInt[i] = 77; //each element set to 77
	}
	SafeArrayUnaccessData((*ps).ptoArrOfInt32s);
	return ps;
}

bool ValidateSafearray(SAFEARRAY* psa)
{
	int* pInt;
	HRESULT hr;

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(psa, (void**)&pInt);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayAccessData call in ValidateSafearray failed!\n");
		if(hr == E_INVALIDARG)
			printf("E_INVALIDARG\n");
		else
			printf("E_UNEXPECTED\n");
		return false;
	}
	for(int i = 0; i < NumArrElements; i++)		
		if(pInt[i] != 7) {
			printf("ERROR! pInt[i] != 7 in ValidateSafearray \n");
			return false;
		}
	SafeArrayUnaccessData(psa);

	return true;
}

bool ChangeStructWithSA(StructWithSA* ps)
{
	int *pInt; //will point to the data
	HRESULT hr;

	(*ps).i32 = 77; //non-array field
	
	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData((*ps).ptoArrOfInt32s, (void**)&pInt);
	if(FAILED(hr))
	{
		printf("\t\tSafeArrayAccessData call in ChangeStructWithSA failed!\n");
		return false;
	}
	for(int i = 0; i < NumArrElements; i++)		
		pInt[i] = 77; //each element set to -1
	SafeArrayUnaccessData((*ps).ptoArrOfInt32s);

	return true;
}

bool CheckStructWithSA(StructWithSA* ps)
{
	//checking non-array fields
	if((*ps).i32 != 1) {
		printf("\t\tError!  (*ps).i32 != 1 \n");
		return false;
	}

	//checking array field
	if(!ValidateSafearray((*ps).ptoArrOfInt32s)) 
	{
		printf("\t\tError!  (*ps).ptoArrOfInt32s not as expected \n");	 
		return false;
	}

	return true;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE StructWithSA_In(StructWithSA s)
{
	//Make sure the StructWithSA is what we expect
	if(!CheckStructWithSA(&s))
		return FALSE;

	//If it's what we expect, change the value and return.
	//NOTE: this changed should not be propagated back to the caller
	//	  since this is not a ref call
	if(!ChangeStructWithSA(&s))
		return FALSE;
	return TRUE;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE StructWithSA_Out2(StructWithSA s)
{
	//Make sure the StructWithSA is what we expect
	if(!CheckStructWithSA(&s))
		return FALSE;

	//If it's what we expect, change the value and return.
	//NOTE: this changed should not be propagated back to the caller
	//	  since this is not a ref call
	if(!ChangeStructWithSA(&s))
		return FALSE;
	return TRUE;
}

//behaves like a ref parameter
extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE StructWithSA_Out(StructWithSA* ps)
{
	//ps = NewStructWithSA();
	int* pInt;
	HRESULT hr;
	
	(*ps).i32 = 77; //change i32 field

	//create new SAFEARRAY; 1 dimension
	(*ps).ptoArrOfInt32s = SafeArrayCreateVector(VT_I4, 0, NumArrElements);  
	if((*ps).ptoArrOfInt32s == NULL)
	{
		printf("\t\tSafeArrayCreateVector call failed!\n");
		exit(1);
	}
	else
	{
		// Get a pointer to the elements of the array.
		hr = SafeArrayAccessData((*ps).ptoArrOfInt32s, (void**)&pInt);
		if(FAILED(hr))
		{
			printf("\t\tSafeArrayAccessData call in NewStructWithSA failed!\n");
			exit(1);
		}
		for(int i = 0; i < NumArrElements; i++)		
			pInt[i] = 77; //each element set to 77
	}
	SafeArrayUnaccessData((*ps).ptoArrOfInt32s);

	return TRUE;
}

//StructWithSA_InOut
extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE StructWithSA_InOut(StructWithSA s)
{
	//Make sure the StructWithSA is what we expect
	if(!CheckStructWithSA(&s))
		return FALSE;

	//If it's what we expect, change the value and return.
	//NOTE: this changed should not be propagated back to the caller
	//	  since this is not a ref call
	if(!ChangeStructWithSA(&s))
		return FALSE;
	return TRUE;
}

//StructWithSA_InOutRef
extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE StructWithSA_InOutRef(StructWithSA* ps)
{
	//Make sure the StructWithSA is what we expect
	if(!CheckStructWithSA(ps))
		return FALSE;

	//If it's what we expect, change the value and return.
	if(!ChangeStructWithSA(ps))
		return FALSE;
	return TRUE;
}

//StructWithSA_Ret
extern "C" DLL_EXPORT StructWithSA STDMETHODCALLTYPE StructWithSA_Ret()
{
	StructWithSA* ps;

	ps = NewStructWithSA();

	//return newly allocated StructWithSA
	return *ps;
}

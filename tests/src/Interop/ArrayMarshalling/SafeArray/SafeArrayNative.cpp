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

extern "C" __declspec(dllexport) BOOL __stdcall SafeArray_In(SAFEARRAY* psa)
{
	long i, lUbound, lLbound;
	int  *pInt; //will point to the data
	HRESULT hr;

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(psa, (void **)&pInt);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayAccessData call in SafeArray_In failed!\n");
		return FALSE;
	}

	//get upperbound
	hr = SafeArrayGetUBound(psa, 1, &lUbound);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayGetUBound call in SafeArray_In failed!\n");
		return FALSE;
	}

	//check upperbound
	if( lUbound != 255 ) //since num of elems = lUBound - lLBound + 1
	{
		printf("\t\tlUbound not as expected in SafeArray_In!\n");
		printf("\t\t\tlUbound = %d",lUbound);
		return FALSE;
	}

	//get lowerbound
	hr = SafeArrayGetLBound(psa, 1, &lLbound);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayGetLBound call in SafeArray_In failed!\n");
		return FALSE;
	}

	//check lowerbound
	if( lLbound != 0 ) 
	{
		printf("\t\tlLbound not as expected in SafeArray_In!\n");
		printf("\t\t\tlLbound = %d",lLbound);
		return FALSE;
	}

	//check dimension
	if( SafeArrayGetDim(psa) != 1 )
	{
		printf("\t\tDimension not as expected in SafeArray_In!\n");
		return FALSE;
	}

	//check element size
	if( SafeArrayGetElemsize(psa) != 4 ) //size of each element should be 4 bytes
	{
		printf("\t\tElement size not as expected in SafeArray_In!\n");
		return FALSE;
	}

	//validate data
	for(i = 0; i <= lUbound; i++)
	{
		if( pInt[i] != i )
		{
			printf("\t\tData not as expected in SafeArray_In!\n");
			return FALSE;
		}
	}

	SafeArrayUnaccessData(psa);
	return TRUE;
}

extern "C" __declspec(dllexport) BOOL __stdcall SafeArray_InOut(SAFEARRAY* psa)
{
	long i, lUbound, lLbound;
	int  *pInt; //will point to the data
	HRESULT hr;

	/************************************************
	* Validate the safearray 
	************************************************/

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(psa, (void **)&pInt);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayAccessData call in SafeArray_InOut failed!\n");
		return FALSE;
	}

	//get upperbound
	hr = SafeArrayGetUBound(psa, 1, &lUbound);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayGetUBound call in SafeArray_InOut failed!\n");
		return FALSE;
	}

	//check upperbound
	if( lUbound != 255 ) //since num of elems = lUBound - lLBound + 1
	{
		printf("\t\tlUbound not as expected in SafeArray_InOut!\n");
		printf("\t\t\tlUbound = %d",lUbound);
		return FALSE;
	}

	//get lowerbound
	hr = SafeArrayGetLBound(psa, 1, &lLbound);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayGetLBound call in SafeArray_InOut failed!\n");
		return FALSE;
	}

	//check lowerbound
	if( lLbound != 0 ) 
	{
		printf("\t\tlLbound not as expected in SafeArray_InOut!\n");
		printf("\t\t\tlLbound = %d",lLbound);
		return FALSE;
	}

	//check dimension
	if( SafeArrayGetDim(psa) != 1 )
	{
		printf("\t\tDimension not as expected in SafeArray_InOut!\n");
		return FALSE;
	}

	//check element size
	if( SafeArrayGetElemsize(psa) != 4 ) //size of each element should be 4 bytes
	{
		printf("\t\tElement size not as expected in SafeArray_InOut!\n");
		return FALSE;
	}

	//validate data
	for(i = 0; i <= lUbound; i++)
	{
		if( pInt[i] != i )
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

extern "C" __declspec(dllexport) SAFEARRAY* _stdcall SafeArray_Ret()
{
	SAFEARRAY * psa;
	int * pInt;
    HRESULT hr;

	psa = SafeArrayCreateVector(VT_I4, 0, 1024); //data is array of ints; size = 1024 elements
	if( psa == NULL )
	{
		printf("\t\tSafeArrayCreateVector call failed!\n");
		return NULL;
	}
	else
	{
		// Get a pointer to the elements of the array.
		hr = SafeArrayAccessData(psa, (void **)&pInt);
		if( FAILED(hr) )
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

extern "C" __declspec(dllexport) BOOL __stdcall SafeArray_InByRef(SAFEARRAY** ppsa)
{
	long i, lUbound, lLbound;
	int  *pInt; //will point to the data
	HRESULT hr;

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(*ppsa, (void **)&pInt);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayAccessData call in SafeArray_InByRef failed!\n");
		return FALSE;
	}

	//get upperbound
	hr = SafeArrayGetUBound(*ppsa, 1, &lUbound);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayGetUBound call in SafeArray_InByRef failed!\n");
		return FALSE;
	}

	//check upperbound
	if( lUbound != 255 ) //since num of elems = lUBound - lLBound + 1
	{
		printf("\t\tlUbound not as expected in SafeArray_InByRef!\n");
		printf("\t\t\tlUbound = %d",lUbound);
		return FALSE;
	}

	//get lowerbound
	hr = SafeArrayGetLBound(*ppsa, 1, &lLbound);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayGetLBound call in SafeArray_InByRef failed!\n");
		return FALSE;
	}

	//check lowerbound
	if( lLbound != 0 ) 
	{
		printf("\t\tlLbound not as expected in SafeArray_InByRef!\n");
		printf("\t\t\tlLbound = %d",lLbound);
		return FALSE;
	}

	//check dimension
	if( SafeArrayGetDim(*ppsa) != 1 )
	{
		printf("\t\tDimension not as expected in SafeArray_InByRef!\n");
		return FALSE;
	}

	//check element size
	if( SafeArrayGetElemsize(*ppsa) != 4 ) //size of each element should be 4 bytes
	{
		printf("\t\tElement size not as expected in SafeArray_InByRef!\n");
		return FALSE;
	}

	//validate data
	for(i = 0; i <= lUbound; i++)
	{
		//printf("\t\t\ti = %d ; pInt[i] = %d\n", i, pInt[i]);
		if( pInt[i] != i )
		{
			printf("\t\tData not as expected in SafeArray_InByRef!\n");
			return FALSE;
		}
	}

	SafeArrayUnaccessData(*ppsa);
	return TRUE;
}

extern "C" __declspec(dllexport) BOOL __stdcall SafeArray_InOutByRef(SAFEARRAY** ppsa)
{
	long i, lUbound, lLbound;
	int  *pInt; //will point to the data
	HRESULT hr;
	
	// Validate the safearray 

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(*ppsa, (void **)&pInt);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayAccessData call in SafeArray_InOutByRef failed!\n");
		return FALSE;
	}

	//get upperbound
	hr = SafeArrayGetUBound(*ppsa, 1, &lUbound);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayGetUBound call in SafeArray_InOutByRef failed!\n");
		return FALSE;
	}

	//check upperbound
	if( lUbound != 255 ) //since num of elems = lUBound - lLBound + 1
	{
		printf("\t\tlUbound not as expected in SafeArray_InOutByRef!\n");
		printf("\t\t\tlUbound = %d",lUbound);
		return FALSE;
	}

	//get lowerbound
	hr = SafeArrayGetLBound(*ppsa, 1, &lLbound);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayGetLBound call in SafeArray_InOutByRef failed!\n");
		return FALSE;
	}

	//check lowerbound
	if( lLbound != 0 ) 
	{
		printf("\t\tlLbound not as expected in SafeArray_InOutByRef!\n");
		printf("\t\t\tlLbound = %d",lLbound);
		return FALSE;
	}

	//check dimension
	if( SafeArrayGetDim(*ppsa) != 1 )
	{
		printf("\t\tDimension not as expected in SafeArray_InOutByRef!\n");
		return FALSE;
	}

	//check element size
	if( SafeArrayGetElemsize(*ppsa) != 4 ) //size of each element should be 4 bytes
	{
		printf("\t\tElement size not as expected in SafeArray_InOut!\n");
		return FALSE;
	}

	//validate data
	for(i = 0; i <= lUbound; i++)
	{
		if( pInt[i] != i )
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

extern "C" __declspec(dllexport) BOOL __stdcall SafeArrayWithOutAttribute(SAFEARRAY* psa)
{
	long i, lUbound;
	int  *pInt; //will point to the data
	HRESULT hr;

	// Get a pointer to the elements of the array.
	hr = SafeArrayAccessData(psa, (void **)&pInt);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayAccessData call in SafeArray_InOut failed!\n");
		return FALSE;
	}

	//get upperbound
	hr = SafeArrayGetUBound(psa, 1, &lUbound);
	if( FAILED(hr) )
	{
		printf("\t\tSafeArrayGetUBound call in SafeArray_InOut failed!\n");
		return FALSE;
	}

	//check upperbound
	if( lUbound != 255 ) //since num of elems = lUBound - lLBound + 1
	{
		printf("\t\tlUbound not as expected in SafeArray_InOut!\n");
		printf("\t\t\tlUbound = %d",lUbound);
		return FALSE;
	}

	//check dimension
	if( SafeArrayGetDim(psa) != 1 )
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

extern "C" __declspec(dllexport) SAFEARRAY* _stdcall SafeArray_Ret_MismatchRank()
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
	if( psa == NULL )
	{
		printf("\t\tSafeArrayCreate call failed!\n");
		return NULL;
	}
	
	return psa;
}

extern "C" __declspec(dllexport) SAFEARRAY* _stdcall SafeArray_Ret_InvalidLBound()
{
	SAFEARRAY * psa;

	int rank = 1;
	SAFEARRAYBOUND rgsabound[1];
	rgsabound[0].lLbound = 99;
	rgsabound[0].cElements = 5;

	psa = SafeArrayCreate(VT_I4, rank, rgsabound);
	if( psa == NULL )
	{
		printf("\t\tSafeArrayCreate call failed!\n");
		return NULL;
	}
	
	return psa;
}

//--------------------------------
//	End of SafeArray calls
//--------------------------------

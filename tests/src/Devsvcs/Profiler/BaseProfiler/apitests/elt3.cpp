#include "ProfilerCommon.h"

class EnterLeaveTail3
{
public:
	EnterLeaveTail3(IPrfCom * pPrfCom)
	{
		EnterLeaveTail3::This = this;
		m_pPrfCom = pPrfCom;
		//InitializeCriticalSection(&m_criticalSection);
	}
	~EnterLeaveTail3()
	{
		EnterLeaveTail3::This = NULL;
		//DeleteCriticalSection(&m_criticalSection);
	}
	IPrfCom *m_pPrfCom;
	CRITICAL_SECTION m_criticalSection;
    HRESULT FunctionEnter2(IPrfCom * pPrfCom, FunctionID funcId, UINT_PTR clientData, COR_PRF_FRAME_INFO func, COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo)
	{
        ClassID classId;
        ModuleID moduleId;
        mdToken token;
        ClassID typeArgs[SHORT_LENGTH];
        ULONG32 nTypeArgs;
        if (pPrfCom->m_FunctionEnter2 != 0)
        {
            //check the frame info by calling back to CLR with the frame info as an input parameter
            MUST_PASS(PINFO->GetFunctionInfo2(funcId,
                                    func,
                                    &classId,
                                    &moduleId,
                                    &token,
                                    SHORT_LENGTH,
                                    &nTypeArgs,
                                    typeArgs));
			return S_OK;
        }
		else
			return E_FAIL;
	}
	HRESULT FunctionLeave2(IPrfCom * pPrfCom, FunctionID funcId, UINT_PTR clientData, COR_PRF_FRAME_INFO func, COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo)
	{
        ClassID classId;
        ModuleID moduleId;
        mdToken token;
        ClassID typeArgs[SHORT_LENGTH];
        ULONG32 nTypeArgs;
        if (pPrfCom->m_FunctionLeave2 != 0)
        {
            //check the frame info by calling back to CLR with the frame info as an input parameter
            MUST_PASS(PINFO->GetFunctionInfo2(funcId,
                                    func,
                                    &classId,
                                    &moduleId,
                                    &token,
                                    SHORT_LENGTH,
                                    &nTypeArgs,
                                    typeArgs));
			return S_OK;
        }
		else
			return E_FAIL;
	}
	HRESULT FunctionTailCall2(IPrfCom * pPrfCom, FunctionID funcId, UINT_PTR clientData, COR_PRF_FRAME_INFO func)
	{
        ClassID classId;
        ModuleID moduleId;
        mdToken token;
        ClassID typeArgs[SHORT_LENGTH];
        ULONG32 nTypeArgs;
        if (pPrfCom->m_FunctionTailcall2 != 0)
        {
            //check the frame info by calling back to CLR with the frame info as an input parameter
            MUST_PASS(PINFO->GetFunctionInfo2(funcId,
                                    func,
                                    &classId,
                                    &moduleId,
                                    &token,
                                    SHORT_LENGTH,
                                    &nTypeArgs,
                                    typeArgs));
			return S_OK;
        }
		else
			return E_FAIL;
	}

    static HRESULT FunctionEnter2Wrapper(IPrfCom * pPrfCom, FunctionID funcId, UINT_PTR clientData, COR_PRF_FRAME_INFO func, COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo)
	{
		return Instance()->FunctionEnter2(pPrfCom, funcId, clientData, func, argumentInfo);
	}
	static HRESULT FunctionLeave2Wrapper(IPrfCom * pPrfCom, FunctionID funcId, UINT_PTR clientData, COR_PRF_FRAME_INFO func, COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo)
	{
		return Instance()->FunctionLeave2(pPrfCom, funcId, clientData, func, argumentInfo);
	}
	static HRESULT FunctionTailCall2Wrapper(IPrfCom * pPrfCom, FunctionID funcId, UINT_PTR clientData, COR_PRF_FRAME_INFO func)
	{
		return Instance()->FunctionTailCall2(pPrfCom, funcId, clientData, func);
	}
	

	static HRESULT FunctionEnter3Wrapper(IPrfCom * pPrfCom, FunctionIDOrClientID functionIdOrClientID)
	{
		return Instance()->FunctionEnter3(pPrfCom, functionIdOrClientID);
	}
	static HRESULT FunctionLeave3Wrapper(IPrfCom * pPrfCom, FunctionIDOrClientID functionIdOrClientID)
	{
		return Instance()->FunctionLeave3(pPrfCom, functionIdOrClientID);
	}
	static HRESULT FunctionTailCall3Wrapper(IPrfCom * pPrfCom, FunctionIDOrClientID functionIdOrClientID)
	{
		return Instance()->FunctionTailCall3(pPrfCom, functionIdOrClientID);
	}
	static UINT_PTR __stdcall FunctionIDMapperWrapper(FunctionID functionId, BOOL *pbHookFunction )
	{
		return Instance()->FunctionIDMapper(Instance()->m_pPrfCom, functionId, pbHookFunction);
	}
	static UINT_PTR __stdcall FunctionIDMapper2Wrapper(FunctionID functionId, void * clientData, BOOL *pbHookFunction )
	{
		return Instance()->FunctionIDMapper2(Instance()->m_pPrfCom, functionId, clientData, pbHookFunction);
	}
	static HRESULT FunctionEnter3WithInfoWrapper(IPrfCom * pPrfCom, FunctionIDOrClientID functionIdOrClientID, COR_PRF_ELT_INFO eltInfo)
	{
		return Instance()->FunctionEnter3WithInfo(pPrfCom, functionIdOrClientID, eltInfo);
	}
	static HRESULT FunctionLeave3WithInfoWrapper(IPrfCom * pPrfCom, FunctionIDOrClientID functionIdOrClientID, COR_PRF_ELT_INFO eltInfo)
	{
		return Instance()->FunctionLeave3WithInfo(pPrfCom, functionIdOrClientID, eltInfo);
	}
	static HRESULT FunctionTailCall3WithInfoWrapper(IPrfCom * pPrfCom, FunctionIDOrClientID functionIdOrClientID, COR_PRF_ELT_INFO eltInfo)
	{
		return Instance()->FunctionTailCall3WithInfo(pPrfCom, functionIdOrClientID, eltInfo);
	}
    
	
	HRESULT FunctionEnter3(IPrfCom * pPrfCom, FunctionIDOrClientID functionIdOrClientID)
	{
        if (pPrfCom->m_FunctionEnter3 != 0)
			return S_OK;
		else
			return E_FAIL;
	}
	HRESULT FunctionLeave3(IPrfCom * pPrfCom, FunctionIDOrClientID /*functionIdOrClientID*/)
	{
		if (pPrfCom->m_FunctionLeave3 != 0)
			return S_OK;
		else
			return E_FAIL;
	}
	HRESULT FunctionTailCall3(IPrfCom * pPrfCom, FunctionIDOrClientID /*functionIdOrClientID*/)
	{
		if (pPrfCom->m_FunctionTailCall3 != 0)
			return S_OK;
		else
			return E_FAIL;
	}
	UINT_PTR FunctionIDMapper(IPrfCom * pPrfCom, FunctionID functionId, BOOL *pbHookFunction )
	{
		//EnterCriticalSection(&m_criticalSection);
		*pbHookFunction = TRUE;
		pPrfCom->m_FunctionIDMapper++;
		//LeaveCriticalSection(&m_criticalSection);
        if (pPrfCom->m_FunctionIDMapper!= 0)
			return (UINT_PTR)functionId;
		else
			return (UINT_PTR)E_FAIL;
	}

	UINT_PTR FunctionIDMapper2(IPrfCom * pPrfCom, FunctionID functionId, void * clientData, BOOL *pbHookFunction)
	{
		//EnterCriticalSection(&m_criticalSection);
		*pbHookFunction = TRUE;
		pPrfCom->m_FunctionIDMapper2++;
		//LeaveCriticalSection(&m_criticalSection);
        if (pPrfCom->m_FunctionIDMapper2!= 0)
			return (UINT_PTR)functionId;
		else
			return (UINT_PTR)E_FAIL;
	}
	HRESULT FunctionEnter3WithInfo(IPrfCom * pPrfCom, FunctionIDOrClientID functionIdOrClientID, COR_PRF_ELT_INFO eltInfo)
	{
        COR_PRF_FRAME_INFO frameInfo;
		ULONG pcbArgumentInfo = sizeof(COR_PRF_FUNCTION_ARGUMENT_INFO);
		COR_PRF_FUNCTION_ARGUMENT_INFO * pArgumentInfo = new COR_PRF_FUNCTION_ARGUMENT_INFO;
		HRESULT hr = pPrfCom->m_pInfo->GetFunctionEnter3Info(functionIdOrClientID.functionID, eltInfo, &frameInfo, &pcbArgumentInfo, pArgumentInfo);
		if (hr != S_OK && hr != HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER))
		{
			delete pArgumentInfo;
			return E_FAIL;
		}
		else if (hr == HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER))
		{
			delete pArgumentInfo;
			pArgumentInfo = reinterpret_cast <COR_PRF_FUNCTION_ARGUMENT_INFO *>  (new BYTE[pcbArgumentInfo]); 
			hr = pPrfCom->m_pInfo->GetFunctionEnter3Info(functionIdOrClientID.functionID, eltInfo, &frameInfo, &pcbArgumentInfo, pArgumentInfo);
			if(hr != S_OK)
			{
				delete[] (BYTE *)pArgumentInfo;
				return E_FAIL;
			}
		}
		if(hr != S_OK)
		{
			DISPLAY("Did not encounter expected return value from GetFunctionEnter3Info");
			if(pArgumentInfo != NULL)
				delete pArgumentInfo;
			return hr;
		}
		//DISPLAY("\nRanges : " << pArgumentInfo->numRanges);
			ULONG totalArgumentSize = 0;
		//DISPLAY("\ntotalArgumentSize  : " <<  pArgumentInfo->totalArgumentSize);
			for(int k = 0 ; k < (int)pArgumentInfo->numRanges; k++)
			{
				//DISPLAY("\npArgumentInfo->ranges[" << k << "].length = " << pArgumentInfo->ranges[k].length);
				//DISPLAY("\npArgumentInfo->ranges[" << k << "].startAddress = " << pArgumentInfo->ranges[k].startAddress);
				totalArgumentSize  += pArgumentInfo->ranges[k].length;
			}
			if(totalArgumentSize  != pArgumentInfo->totalArgumentSize)
			{
				DISPLAY("\ntotalArgumentSize did not match up to pArgumentInfo->totalArgumentSize");
				return E_FAIL;
			}	
			else
			{
				//DISPLAY("\ntotalArgumentSize matched up to pArgumentInfo->totalArgumentSize");
			}
		
		WCHAR_STR( funcName );
		
		pPrfCom->GetFunctionIDName(functionIdOrClientID.functionID, funcName, frameInfo);

		ModuleID modID;
		ClassID classID;
		mdToken token;
		ClassID typeArgs[SHORT_LENGTH];
		ULONG32 nTypeArgs;
		pPrfCom->m_pInfo->GetFunctionInfo2(
			functionIdOrClientID.functionID,
			frameInfo,
			&classID,
			&modID,
			&token,
			SHORT_LENGTH,
			&nTypeArgs,
			typeArgs);

		for (ULONG32 i = 0; i < nTypeArgs; i++)
		{
			WCHAR_STR( className );
			pPrfCom->GetClassIDName(typeArgs[(INT)i], className);

			if (typeArgs[(INT)i] == 0)
			{
				DISPLAY("\nNull class ID returned as a Type Arg for a generic function.\n");
				return E_FAIL;
			}
		}
		if (NULL != pArgumentInfo)
		{
			delete[] (BYTE *)pArgumentInfo;
		}
		if (FAILED(hr))
		{
			return hr;
		}

		if (pPrfCom->m_FunctionEnter3WithInfo != 0)
			return S_OK;
		else
			return E_FAIL;
	}
	HRESULT FunctionLeave3WithInfo(IPrfCom * pPrfCom, FunctionIDOrClientID functionIdOrClientID, COR_PRF_ELT_INFO eltInfo)
	{
		COR_PRF_FRAME_INFO frameInfo;
		COR_PRF_FUNCTION_ARGUMENT_RANGE * pRetvalRange = new COR_PRF_FUNCTION_ARGUMENT_RANGE;
		HRESULT hr = pPrfCom->m_pInfo->GetFunctionLeave3Info(functionIdOrClientID.functionID, eltInfo, &frameInfo, pRetvalRange);
		//DISPLAY("\nGetFunctionLeave3Info_hr = " << hr);
		if (hr == HRESULT_FROM_WIN32(E_INVALIDARG))
		{
			DISPLAY("\nInvalid functionId FunctionLeave3WithInfo");
			delete pRetvalRange;
			return hr;
		}
		else if(hr != S_OK)
		{
			DISPLAY("\nOther error condition FunctionLeave3WithInfo");
			delete pRetvalRange;
			return hr;
		}
		else
		{
			WCHAR wszMethodName[STRING_LENGTH];
			ULONG cwName = NULL;
			PCCOR_SIGNATURE pSig = NULL;
			ULONG cbSig = NULL;
			ModuleID moduleId = NULL;
			mdToken  token    = NULL;
			ClassID  classId  = NULL;

			IMetaDataImport * pIMDImport = 0;
			MUST_PASS(pPrfCom->m_pInfo->GetFunctionInfo2(functionIdOrClientID.functionID, frameInfo, &classId, &moduleId, &token, 0, NULL, NULL));

			MUST_PASS(pPrfCom->m_pInfo->GetModuleMetaData(moduleId, 
				0x00000000 /*Open scope for read - enum CorOpenFlags in CorHdr*/, 
				IID_IMetaDataImport2, 
				(IUnknown **)&pIMDImport));
			pIMDImport->GetMethodProps(token, NULL, wszMethodName, STRING_LENGTH, &cwName, NULL, &pSig, &cbSig, NULL, NULL);
			//DISPLAY(L"\nFuncLeave3WithInfo : " << wszMethodName);;
		}
		if (pPrfCom->m_FunctionLeave3WithInfo != 0)
			return S_OK;
		else
			return E_FAIL;
	}
	HRESULT FunctionTailCall3WithInfo(IPrfCom * pPrfCom, FunctionIDOrClientID functionIdOrClientID, COR_PRF_ELT_INFO eltInfo)
	{
		COR_PRF_FRAME_INFO frameInfo;
		HRESULT hr = pPrfCom->m_pInfo->GetFunctionTailcall3Info(functionIdOrClientID.functionID, eltInfo, &frameInfo);
		if (hr == HRESULT_FROM_WIN32(E_INVALIDARG))
		{
			DISPLAY("\nInvalid functionId error received from GetFunctionTailCall3WithInfo");
			return hr;
		}
		else if(hr == S_OK)
		{
			WCHAR_STR( functionName );
			MUST_PASS(PPRFCOM->GetFunctionIDName(functionIdOrClientID.functionID, functionName, frameInfo, true));
			//DISPLAY(L"\nFunctionTailCall3WithInfo - GetFunctionIDName -" << functionName);
		}
		else
		{
			DISPLAY("\nUnknown HR received for GetFunctionTailCall3Info");
			return hr;
		}
		if (pPrfCom->m_FunctionTailCall3WithInfo != 0)
			return S_OK;
		else
			return E_FAIL;
	}

	HRESULT Verify(IPrfCom * pPrfCom)
	{
		if(pPrfCom->m_FunctionEnter3 != 0)
		{
			if(pPrfCom->m_FunctionEnter3 == pPrfCom->m_FunctionLeave3 + pPrfCom->m_FunctionTailCall3)
				return S_OK;
			else
			{
				DISPLAY("\nNumber of FunctionEnter3 does not match sum of FunctionLeave3 + FunctionTailCall3");
				return E_FAIL; // Number of FunctionEnter3 does not match sum of FunctionLeave3 + FunctionTailCall3
			}
		}
		if(pPrfCom->m_FunctionEnter3WithInfo != 0)
		{
			if(pPrfCom->m_FunctionEnter3WithInfo == pPrfCom->m_FunctionLeave3WithInfo + pPrfCom->m_FunctionTailCall3WithInfo)
				return S_OK;
			else
			{
				DISPLAY("\nNumber of FunctionEnter3WithInfo does not match sum of FunctionLeave3WithInfo + FunctionTailCall3WithInfo");
				return E_FAIL; // Number of FunctionEnter3WithInfo does not match sum of FunctionLeave3WithInfo + FunctionTailCall3WithInfo
			}
		}
		return S_OK;
	}
	static EnterLeaveTail3* This;
	static EnterLeaveTail3* Instance()
	{
		return This;
	}
};

EnterLeaveTail3* EnterLeaveTail3::This = new EnterLeaveTail3(NULL);
EnterLeaveTail3* global_EnterLeaveTail3;

HRESULT FastPathELT3_Verify(IPrfCom * pPrfCom)
{
	HRESULT hr = EnterLeaveTail3::Instance()->Verify(pPrfCom);
	DISPLAY(L"\nProfiler API Test: FastPathELT3_Verify");
	DISPLAY(  "\npPrfCom->m_FunctionIDMapper             = " << pPrfCom->m_FunctionIDMapper);
	DISPLAY(  "\npPrfCom->m_FunctionIDMapper2            = " << pPrfCom->m_FunctionIDMapper2);
    DISPLAY(  "\npPrfCom->m_FunctionEnter2               = " << pPrfCom->m_FunctionEnter2);
	DISPLAY(  "\npPrfCom->m_FunctionLeave2               = " << pPrfCom->m_FunctionLeave2);
	DISPLAY(  "\npPrfCom->m_FunctionTailCall2            = " << pPrfCom->m_FunctionTailcall2);
	DISPLAY(  "\npPrfCom->m_FunctionEnter3               = " << pPrfCom->m_FunctionEnter3);
	DISPLAY(  "\npPrfCom->m_FunctionLeave3               = " << pPrfCom->m_FunctionLeave3);
	DISPLAY(  "\npPrfCom->m_FunctionTailCall3            = " << pPrfCom->m_FunctionTailCall3);
	DISPLAY(  "\npPrfCom->m_FunctionEnter3WithInfo       = " << pPrfCom->m_FunctionEnter3WithInfo);
	DISPLAY(  "\npPrfCom->m_FunctionLeave3WithInfo       = " << pPrfCom->m_FunctionLeave3WithInfo);
	DISPLAY(  "\npPrfCom->m_FunctionTailCall3WithInfo    = " << pPrfCom->m_FunctionTailCall3WithInfo);
	DISPLAY(  "\n");
	delete global_EnterLeaveTail3;

	return hr;
}
void FastPathELT3_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable)
{
	DISPLAY(L"\nProfiler API Test: FastPathELT3_Initialize");
	global_EnterLeaveTail3 = new EnterLeaveTail3(pPrfCom);

	// Set the TEST POINTER to the instance of the EnterLeaveTail class so that we can pass it back when we are calling SetFunctionIDMapper2 for hooking up.
	// If we dont want to use the test pointer, the other way around would be to use the line below
	// // pPrfCom->m_pInfo->SetFunctionIDMapper2(pModuleMethodTable->FUNCTIONIDMAPPER2, this);
	// However that approach has the following drawbacks. It needs a valid Function pointer for FunctionIDMapper2 and it has to be implemented in every extension
	// dll which want to SetFunctionIDMapper2. A better way would be to do this in the BaseProfilerDriver as it is being done now
	pModuleMethodTable->TEST_POINTER = reinterpret_cast<void *>(global_EnterLeaveTail3);

	// Set only COR_PRF_MONITOR_ENTERLEAVE for FastPath. For Slowpath, you will also need to set one of the following flags
	// COR_PRF_ENABLE_FUNCTION_ARGS(for argument info)  
	// COR_PRF_ENABLE_FUNCTION_RETVAL (for retval info) 
	// COR_PRF_ENABLE_FRAME_INFO (for frame info)
	pModuleMethodTable->FLAGS = COR_PRF_MONITOR_ENTERLEAVE; // only this event mask for fast path

	pModuleMethodTable->VERIFY = (FC_VERIFY) &FastPathELT3_Verify;

	pModuleMethodTable->FUNCTIONENTER3 = (FC_FUNCTIONENTER3) &EnterLeaveTail3::FunctionEnter3Wrapper;
	pModuleMethodTable->FUNCTIONLEAVE3 = (FC_FUNCTIONLEAVE3) &EnterLeaveTail3::FunctionLeave3Wrapper;
	pModuleMethodTable->FUNCTIONTAILCALL3 = (FC_FUNCTIONTAILCALL3) &EnterLeaveTail3::FunctionTailCall3Wrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER = (FC_FUNCTIONIDMAPPER) &EnterLeaveTail3::FunctionIDMapperWrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER2 = (FC_FUNCTIONIDMAPPER2) &EnterLeaveTail3::FunctionIDMapper2Wrapper;
}

void SlowPathELT3_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable)
{

	DISPLAY(L"\nProfiler API Test: SlowPathELT3_Initialize");
	global_EnterLeaveTail3 = new EnterLeaveTail3(pPrfCom);

	// Set the TEST POINTER to the instance of the EnterLeaveTail class so that we can pass it back when we are calling SetFunctionIDMapper2 for hooking up.
	// If we dont want to use the test pointer, the other way around would be to use the line below
	// // pPrfCom->m_pInfo->SetFunctionIDMapper2(pModuleMethodTable->FUNCTIONIDMAPPER2, this);
	// However that approach has the following drawbacks. It needs a valid Function pointer for FunctionIDMapper2 and it has to be implemented in every extension
	// dll which want to SetFunctionIDMapper2. A better way would be to do this in the BaseProfilerDriver as it is being done now
	pModuleMethodTable->TEST_POINTER = reinterpret_cast<void *>(global_EnterLeaveTail3);

	// Set only COR_PRF_MONITOR_ENTERLEAVE for FastPath. For Slowpath, you will also need to set one of the following flags
	// COR_PRF_ENABLE_FUNCTION_ARGS(for argument info)  
	// COR_PRF_ENABLE_FUNCTION_RETVAL (for retval info) 
	// COR_PRF_ENABLE_FRAME_INFO (for frame info)
	pModuleMethodTable->FLAGS = COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_ENABLE_FUNCTION_ARGS | COR_PRF_ENABLE_FUNCTION_RETVAL | COR_PRF_ENABLE_FRAME_INFO; // set event mask for slow path

	pModuleMethodTable->VERIFY = (FC_VERIFY) &FastPathELT3_Verify;
	pModuleMethodTable->FUNCTIONENTER3WITHINFO = (FC_FUNCTIONENTER3WITHINFO) &EnterLeaveTail3::FunctionEnter3WithInfoWrapper;
	pModuleMethodTable->FUNCTIONLEAVE3WITHINFO = (FC_FUNCTIONLEAVE3WITHINFO) &EnterLeaveTail3::FunctionLeave3WithInfoWrapper;
	pModuleMethodTable->FUNCTIONTAILCALL3WITHINFO = (FC_FUNCTIONTAILCALL3WITHINFO) &EnterLeaveTail3::FunctionTailCall3WithInfoWrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER = (FC_FUNCTIONIDMAPPER) &EnterLeaveTail3::FunctionIDMapperWrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER2 = (FC_FUNCTIONIDMAPPER2) &EnterLeaveTail3::FunctionIDMapper2Wrapper;
}

void SlowPathELT3IncorrectFlags_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable)
{

	DISPLAY(L"\nProfiler API Test: SlowPathELT3IncorrectFlags_Initialize");
	global_EnterLeaveTail3 = new EnterLeaveTail3(pPrfCom);

	// Set the TEST POINTER to the instance of the EnterLeaveTail class so that we can pass it back when we are calling SetFunctionIDMapper2 for hooking up.
	// If we dont want to use the test pointer, the other way around would be to use the line below
	// // pPrfCom->m_pInfo->SetFunctionIDMapper2(pModuleMethodTable->FUNCTIONIDMAPPER2, this);
	// However that approach has the following drawbacks. It needs a valid Function pointer for FunctionIDMapper2 and it has to be implemented in every extension
	// dll which want to SetFunctionIDMapper2. A better way would be to do this in the BaseProfilerDriver as it is being done now
	pModuleMethodTable->TEST_POINTER = reinterpret_cast<void *>(global_EnterLeaveTail3);

	// set event mask for fast path and call slow path hooks
	pModuleMethodTable->FLAGS = COR_PRF_MONITOR_ENTERLEAVE; 
	pModuleMethodTable->VERIFY = (FC_VERIFY) &FastPathELT3_Verify;
	pModuleMethodTable->FUNCTIONENTER3WITHINFO = (FC_FUNCTIONENTER3WITHINFO) &EnterLeaveTail3::FunctionEnter3WithInfoWrapper;
	pModuleMethodTable->FUNCTIONLEAVE3WITHINFO = (FC_FUNCTIONLEAVE3WITHINFO) &EnterLeaveTail3::FunctionLeave3WithInfoWrapper;
	pModuleMethodTable->FUNCTIONTAILCALL3WITHINFO = (FC_FUNCTIONTAILCALL3WITHINFO) &EnterLeaveTail3::FunctionTailCall3WithInfoWrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER = (FC_FUNCTIONIDMAPPER) &EnterLeaveTail3::FunctionIDMapperWrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER2 = (FC_FUNCTIONIDMAPPER2) &EnterLeaveTail3::FunctionIDMapper2Wrapper;
	// This hookup should fail with the expected error code CORPROF_E_INCONSISTENT_WITH_FLAGS
	MUST_RETURN_VALUE(pPrfCom->m_pInfo->SetEnterLeaveFunctionHooks3WithInfo(reinterpret_cast<FunctionEnter3WithInfo *> (pModuleMethodTable->FUNCTIONENTER3WITHINFO), reinterpret_cast<FunctionLeave3WithInfo *>(pModuleMethodTable->FUNCTIONLEAVE3WITHINFO) , reinterpret_cast<FunctionTailcall3WithInfo *>(pModuleMethodTable->FUNCTIONTAILCALL3WITHINFO) ), CORPROF_E_INCONSISTENT_WITH_FLAGS);
	// Now set the correct flags and the test should work as good as SlowPathELT3.
	pModuleMethodTable->FLAGS = COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_ENABLE_FUNCTION_ARGS | COR_PRF_ENABLE_FUNCTION_RETVAL | COR_PRF_ENABLE_FRAME_INFO; // set event mask for slow path
}

void FastPathELT3IncorrectFlags_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable)
{

	DISPLAY(L"\nProfiler API Test: FastPathELT3IncorrectFlags_Initialize");
	global_EnterLeaveTail3 = new EnterLeaveTail3(pPrfCom);

	// Set the TEST POINTER to the instance of the EnterLeaveTail class so that we can pass it back when we are calling SetFunctionIDMapper2 for hooking up.
	// If we dont want to use the test pointer, the other way around would be to use the line below
	// // pPrfCom->m_pInfo->SetFunctionIDMapper2(pModuleMethodTable->FUNCTIONIDMAPPER2, this);
	// However that approach has the following drawbacks. It needs a valid Function pointer for FunctionIDMapper2 and it has to be implemented in every extension
	// dll which want to SetFunctionIDMapper2. A better way would be to do this in the BaseProfilerDriver as it is being done now
	pModuleMethodTable->TEST_POINTER = reinterpret_cast<void *>(global_EnterLeaveTail3);

	// set event mask for fast path and call slow path hooks
	pModuleMethodTable->FLAGS = COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_ENABLE_FUNCTION_ARGS | COR_PRF_ENABLE_FUNCTION_RETVAL | COR_PRF_ENABLE_FRAME_INFO;
	pModuleMethodTable->VERIFY = (FC_VERIFY) &FastPathELT3_Verify;
	pModuleMethodTable->FUNCTIONENTER3 = (FC_FUNCTIONENTER3) &EnterLeaveTail3::FunctionEnter3Wrapper;
	pModuleMethodTable->FUNCTIONLEAVE3 = (FC_FUNCTIONLEAVE3) &EnterLeaveTail3::FunctionLeave3Wrapper;
	pModuleMethodTable->FUNCTIONTAILCALL3 = (FC_FUNCTIONTAILCALL3) &EnterLeaveTail3::FunctionTailCall3Wrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER = (FC_FUNCTIONIDMAPPER) &EnterLeaveTail3::FunctionIDMapperWrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER2 = (FC_FUNCTIONIDMAPPER2) &EnterLeaveTail3::FunctionIDMapper2Wrapper;
	MUST_RETURN_VALUE(pPrfCom->m_pInfo->SetEnterLeaveFunctionHooks3(reinterpret_cast<FunctionEnter3 *> (pModuleMethodTable->FUNCTIONENTER3), reinterpret_cast<FunctionLeave3 *>(pModuleMethodTable->FUNCTIONLEAVE3) , reinterpret_cast<FunctionTailcall3 *>(pModuleMethodTable->FUNCTIONTAILCALL3) ), 0);
	// Now set the correct flags and the test should work as good as FastPathELT3.
	pModuleMethodTable->FLAGS = COR_PRF_MONITOR_ENTERLEAVE; // reset event mask for fast path
}
void SlowPathELT3IncorrectFlagsDSS_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable)
{

	DISPLAY(L"\nProfiler API Test: SlowPathELT3IncorrectFlagsDSS_Initialize");
	global_EnterLeaveTail3 = new EnterLeaveTail3(pPrfCom);

	// Set the TEST POINTER to the instance of the EnterLeaveTail class so that we can pass it back when we are calling SetFunctionIDMapper2 for hooking up.
	// If we dont want to use the test pointer, the other way around would be to use the line below
	// // pPrfCom->m_pInfo->SetFunctionIDMapper2(pModuleMethodTable->FUNCTIONIDMAPPER2, this);
	// However that approach has the following drawbacks. It needs a valid Function pointer for FunctionIDMapper2 and it has to be implemented in every extension
	// dll which want to SetFunctionIDMapper2. A better way would be to do this in the BaseProfilerDriver as it is being done now
	pModuleMethodTable->TEST_POINTER = reinterpret_cast<void *>(global_EnterLeaveTail3);

	// set event mask for fast path and call slow path hooks
	pModuleMethodTable->FLAGS = COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_ENABLE_STACK_SNAPSHOT ;
	pModuleMethodTable->VERIFY = (FC_VERIFY) &FastPathELT3_Verify;
	pModuleMethodTable->FUNCTIONENTER3WITHINFO = (FC_FUNCTIONENTER3WITHINFO) &EnterLeaveTail3::FunctionEnter3WithInfoWrapper;
	pModuleMethodTable->FUNCTIONLEAVE3WITHINFO = (FC_FUNCTIONLEAVE3WITHINFO) &EnterLeaveTail3::FunctionLeave3WithInfoWrapper;
	pModuleMethodTable->FUNCTIONTAILCALL3WITHINFO = (FC_FUNCTIONTAILCALL3WITHINFO) &EnterLeaveTail3::FunctionTailCall3WithInfoWrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER = (FC_FUNCTIONIDMAPPER) &EnterLeaveTail3::FunctionIDMapperWrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER2 = (FC_FUNCTIONIDMAPPER2) &EnterLeaveTail3::FunctionIDMapper2Wrapper;
	// This hookup should fail with the expected error code CORPROF_E_INCONSISTENT_WITH_FLAGS
	MUST_RETURN_VALUE(pPrfCom->m_pInfo->SetEnterLeaveFunctionHooks3WithInfo(reinterpret_cast<FunctionEnter3WithInfo *> (pModuleMethodTable->FUNCTIONENTER3WITHINFO), reinterpret_cast<FunctionLeave3WithInfo *>(pModuleMethodTable->FUNCTIONLEAVE3WITHINFO) , reinterpret_cast<FunctionTailcall3WithInfo *>(pModuleMethodTable->FUNCTIONTAILCALL3WITHINFO) ), CORPROF_E_INCONSISTENT_WITH_FLAGS);
	// Now set the correct flags and the test should work as good as SlowPathELT3.
	pModuleMethodTable->FLAGS = COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_ENABLE_FUNCTION_ARGS | COR_PRF_ENABLE_FUNCTION_RETVAL | COR_PRF_ENABLE_FRAME_INFO; // set event mask for slow path
}

void SlowPathELT2_Initialize(IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable)
{

	DISPLAY(L"\nProfiler API Test: SlowPathELT2_Initialize");
	global_EnterLeaveTail3 = new EnterLeaveTail3(pPrfCom);

	// Set the TEST POINTER to the instance of the EnterLeaveTail class so that we can pass it back when we are calling SetFunctionIDMapper2 for hooking up.
	// If we dont want to use the test pointer, the other way around would be to use the line below
	// // pPrfCom->m_pInfo->SetFunctionIDMapper2(pModuleMethodTable->FUNCTIONIDMAPPER2, this);
	// However that approach has the following drawbacks. It needs a valid Function pointer for FunctionIDMapper2 and it has to be implemented in every extension
	// dll which want to SetFunctionIDMapper2. A better way would be to do this in the BaseProfilerDriver as it is being done now
	pModuleMethodTable->TEST_POINTER = reinterpret_cast<void *>(global_EnterLeaveTail3);

	// Set only COR_PRF_MONITOR_ENTERLEAVE for FastPath. For Slowpath, you will also need to set one of the following flags
	// COR_PRF_ENABLE_FUNCTION_ARGS(for argument info)  
	// COR_PRF_ENABLE_FUNCTION_RETVAL (for retval info) 
	// COR_PRF_ENABLE_FRAME_INFO (for frame info)
	pModuleMethodTable->FLAGS = COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_ENABLE_FUNCTION_ARGS | COR_PRF_ENABLE_FUNCTION_RETVAL | COR_PRF_ENABLE_FRAME_INFO; // set event mask for slow path
                                
	pModuleMethodTable->VERIFY = (FC_VERIFY) &FastPathELT3_Verify;
	pModuleMethodTable->FUNCTIONENTER2 = (FC_FUNCTIONENTER2)&EnterLeaveTail3::FunctionEnter2Wrapper;
	pModuleMethodTable->FUNCTIONLEAVE2 =  (FC_FUNCTIONLEAVE2)&EnterLeaveTail3::FunctionLeave2Wrapper;
	pModuleMethodTable->FUNCTIONTAILCALL2 =  (FC_FUNCTIONTAILCALL2)&EnterLeaveTail3::FunctionTailCall2Wrapper;
	//pModuleMethodTable->FUNCTIONIDMAPPER =  &EnterLeaveTail3::FunctionIDMapperWrapper;
	pModuleMethodTable->FUNCTIONIDMAPPER2 = &EnterLeaveTail3::FunctionIDMapper2Wrapper;
}
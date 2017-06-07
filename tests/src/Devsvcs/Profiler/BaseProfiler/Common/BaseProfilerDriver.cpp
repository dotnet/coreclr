HRESULT BaseProfilerDriver::SetELTHooks()
{
    HRESULT hr = S_OK;

	// Set the FunctionIDMapper
	if (m_methodTable.FUNCTIONIDMAPPER != NULL)
	{
		MUST_PASS(PINFO->SetFunctionIDMapper(&FunctionIDMapperStub));
	}

	// Set the FunctionIDMapper2
	if (m_methodTable.FUNCTIONIDMAPPER2 != NULL)
	{
		// pass the pointer to the profiler instance for the second argument

		MUST_PASS(PINFO->SetFunctionIDMapper2(&FunctionIDMapper2Stub, m_methodTable.TEST_POINTER));
	}

	// Set Enter/Leave/Tailcall hooks if callbacks were requested
	if (m_methodTable.FUNCTIONENTER    != NULL ||
		m_methodTable.FUNCTIONLEAVE    != NULL ||
		m_methodTable.FUNCTIONTAILCALL != NULL)
	{
		MUST_PASS(PINFO->SetEnterLeaveFunctionHooks(m_methodTable.FUNCTIONENTER    == NULL ? NULL : &EnterStub,
			m_methodTable.FUNCTIONLEAVE    == NULL ? NULL : &LeaveStub,
			m_methodTable.FUNCTIONTAILCALL == NULL ? NULL : &TailcallStub));
	}

	if (m_methodTable.FUNCTIONENTER2    != NULL ||
		m_methodTable.FUNCTIONLEAVE2    != NULL ||
		m_methodTable.FUNCTIONTAILCALL2 != NULL)
	{
		bool fFastEnter2 = FALSE;
		bool fFastLeave2 = FALSE;
		bool fFastTail2  = FALSE;

		FunctionEnter2 * pFE2 = NULL;
		FunctionLeave2 * pFL2 = NULL;
		FunctionTailcall2 * pFTC2 = NULL;

		m_methodTable.FLAGS & (COR_PRF_ENABLE_STACK_SNAPSHOT | COR_PRF_ENABLE_FRAME_INFO | COR_PRF_ENABLE_FUNCTION_ARGS) ? fFastEnter2 = FALSE : fFastEnter2 = TRUE;
		m_methodTable.FLAGS & (COR_PRF_ENABLE_STACK_SNAPSHOT | COR_PRF_ENABLE_FRAME_INFO | COR_PRF_ENABLE_FUNCTION_RETVAL) ? fFastLeave2 = FALSE : fFastLeave2 = TRUE;
		m_methodTable.FLAGS & (COR_PRF_ENABLE_STACK_SNAPSHOT | COR_PRF_ENABLE_FRAME_INFO) ? fFastTail2 = FALSE : fFastTail2 = TRUE;

#if defined(_X86_)

		pFE2  = reinterpret_cast<FunctionEnter2 *>(fFastEnter2 ? &Enter2Naked : &Enter2Stub);
		pFL2  = reinterpret_cast<FunctionLeave2 *>(fFastLeave2 ? &Leave2Naked : &Leave2Stub);
		pFTC2 = reinterpret_cast<FunctionTailcall2 *>(fFastTail2  ? &Tailcall2Naked : &Tailcall2Stub);

#elif defined(_AMD64_) || defined(_ARM_)

		pFE2 = reinterpret_cast<FunctionEnter2 *>(&Enter2Stub);
		pFL2  = reinterpret_cast<FunctionLeave2 *>(fFastLeave2 ? &Leave2Naked : &Leave2Stub);
		pFTC2 = reinterpret_cast<FunctionTailcall2 *>(fFastTail2 ? &Tailcall2Naked : &Tailcall2Stub);

#elif defined(_IA64_)

		pFE2  = reinterpret_cast<FunctionEnter2 *>(&Enter2Stub);
		pFL2  = reinterpret_cast<FunctionLeave2 *>(&Leave2Stub);
		pFTC2 = reinterpret_cast<FunctionTailcall2 *>(&Tailcall2Stub);

#endif

		MUST_PASS(PINFO->SetEnterLeaveFunctionHooks2(m_methodTable.FUNCTIONENTER2    ? pFE2  : NULL,
			m_methodTable.FUNCTIONLEAVE2    ? pFL2  : NULL,
			m_methodTable.FUNCTIONTAILCALL2 ? pFTC2 : NULL));
	}
	if (m_methodTable.FUNCTIONENTER3    != NULL ||
		m_methodTable.FUNCTIONLEAVE3    != NULL ||
		m_methodTable.FUNCTIONTAILCALL3 != NULL)
	{
		FunctionEnter3 * pFE3		= reinterpret_cast<FunctionEnter3 *>(&FunctionEnter3Naked);
		FunctionLeave3 * pFL3		= reinterpret_cast<FunctionLeave3 *>(&FunctionLeave3Naked);
		FunctionTailcall3 * pFTC3	= reinterpret_cast<FunctionTailcall3 *>(&FunctionTailCall3Naked);
		MUST_PASS(PINFO->SetEnterLeaveFunctionHooks3(m_methodTable.FUNCTIONENTER3 ? pFE3 : NULL,
			m_methodTable.FUNCTIONLEAVE3 ? pFL3	: NULL,
			m_methodTable.FUNCTIONTAILCALL3 ? pFTC3 : NULL));
	}

	if (m_methodTable.FUNCTIONENTER3WITHINFO != NULL ||
		m_methodTable.FUNCTIONLEAVE3WITHINFO != NULL ||
		m_methodTable.FUNCTIONTAILCALL3WITHINFO != NULL)
	{
		FunctionEnter3WithInfo * pFEWI3		= reinterpret_cast<FunctionEnter3WithInfo *>(&FunctionEnter3WithInfoStub);
		FunctionLeave3WithInfo * pFLWI3		= reinterpret_cast<FunctionLeave3WithInfo *>(&FunctionLeave3WithInfoStub);
		FunctionTailcall3WithInfo * pFTCWI3	= reinterpret_cast<FunctionTailcall3WithInfo *>(&FunctionTailCall3WithInfoStub);
		MUST_PASS(PINFO->SetEnterLeaveFunctionHooks3WithInfo(m_methodTable.FUNCTIONENTER3WITHINFO ? pFEWI3 : NULL,
			m_methodTable.FUNCTIONLEAVE3WITHINFO ? pFLWI3	: NULL,
			m_methodTable.FUNCTIONTAILCALL3WITHINFO ? pFTCWI3 : NULL));
	}

    return S_OK;
}


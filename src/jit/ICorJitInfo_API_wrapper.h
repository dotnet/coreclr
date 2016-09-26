    //////////////////////////////////////////////////////////////////////////////////
    // class ICorStaticInfo
    //////////////////////////////////////////////////////////////////////////////////

    virtual DWORD getMethodAttribs (
            CORINFO_METHOD_HANDLE       ftn         /* IN */
            );

    virtual void setMethodAttribs (
            CORINFO_METHOD_HANDLE       ftn,        /* IN */
            CorInfoMethodRuntimeFlags   attribs     /* IN */
            );

    virtual void getMethodSig (
             CORINFO_METHOD_HANDLE      ftn,        /* IN  */
             CORINFO_SIG_INFO          *sig,        /* OUT */
             CORINFO_CLASS_HANDLE      memberParent = NULL /* IN */
             );

    virtual bool getMethodInfo (
            CORINFO_METHOD_HANDLE   ftn,            /* IN  */
            CORINFO_METHOD_INFO*    info            /* OUT */
            );

    virtual CorInfoInline canInline (
            CORINFO_METHOD_HANDLE       callerHnd,                  /* IN  */
            CORINFO_METHOD_HANDLE       calleeHnd,                  /* IN  */
            DWORD*                      pRestrictions               /* OUT */
            );

    virtual void reportInliningDecision (CORINFO_METHOD_HANDLE inlinerHnd,
                                                   CORINFO_METHOD_HANDLE inlineeHnd,
                                                   CorInfoInline inlineResult,
                                                   const char * reason);

    virtual bool canTailCall (
            CORINFO_METHOD_HANDLE   callerHnd,          /* IN */
            CORINFO_METHOD_HANDLE   declaredCalleeHnd,  /* IN */
            CORINFO_METHOD_HANDLE   exactCalleeHnd,     /* IN */
            bool fIsTailPrefix                          /* IN */
            );

    virtual void reportTailCallDecision (CORINFO_METHOD_HANDLE callerHnd,
                                                   CORINFO_METHOD_HANDLE calleeHnd,
                                                   bool fIsTailPrefix,
                                                   CorInfoTailCall tailCallResult,
                                                   const char * reason);

    virtual void getEHinfo(
            CORINFO_METHOD_HANDLE ftn,              /* IN  */
            unsigned          EHnumber,             /* IN */
            CORINFO_EH_CLAUSE* clause               /* OUT */
            );

    virtual CORINFO_CLASS_HANDLE getMethodClass (
            CORINFO_METHOD_HANDLE       method
            );

    virtual CORINFO_MODULE_HANDLE getMethodModule (
            CORINFO_METHOD_HANDLE       method
            );

    virtual void getMethodVTableOffset (
            CORINFO_METHOD_HANDLE       method,                 /* IN */
            unsigned*                   offsetOfIndirection,    /* OUT */
            unsigned*                   offsetAfterIndirection  /* OUT */
            );

#if COR_JIT_EE_VERSION > 460
    virtual CorInfoIntrinsics getIntrinsicID(
            CORINFO_METHOD_HANDLE       method,
            bool*                       pMustExpand = NULL      /* OUT */
            );
#else
    virtual CorInfoIntrinsics getIntrinsicID(
            CORINFO_METHOD_HANDLE       method
            );
#endif

    virtual bool isInSIMDModule(
            CORINFO_CLASS_HANDLE        classHnd
            );

    virtual CorInfoUnmanagedCallConv getUnmanagedCallConv(
            CORINFO_METHOD_HANDLE       method
            );

    virtual BOOL pInvokeMarshalingRequired(
            CORINFO_METHOD_HANDLE       method,
            CORINFO_SIG_INFO*           callSiteSig
            );

    virtual BOOL satisfiesMethodConstraints(
            CORINFO_CLASS_HANDLE        parent, // the exact parent of the method
            CORINFO_METHOD_HANDLE       method
            );

    virtual BOOL isCompatibleDelegate(
            CORINFO_CLASS_HANDLE        objCls,           /* type of the delegate target, if any */
            CORINFO_CLASS_HANDLE        methodParentCls,  /* exact parent of the target method, if any */
            CORINFO_METHOD_HANDLE       method,           /* (representative) target method, if any */
            CORINFO_CLASS_HANDLE        delegateCls,      /* exact type of the delegate */
            BOOL                        *pfIsOpenDelegate /* is the delegate open */
            );

    virtual BOOL isDelegateCreationAllowed (
            CORINFO_CLASS_HANDLE        delegateHnd,
            CORINFO_METHOD_HANDLE       calleeHnd
            );

    virtual CorInfoInstantiationVerification isInstantiationOfVerifiedGeneric (
            CORINFO_METHOD_HANDLE   method /* IN  */
            );

    virtual void initConstraintsForVerification(
            CORINFO_METHOD_HANDLE   method, /* IN */
            BOOL *pfHasCircularClassConstraints, /* OUT */
            BOOL *pfHasCircularMethodConstraint /* OUT */
            );

    virtual CorInfoCanSkipVerificationResult canSkipMethodVerification (
            CORINFO_METHOD_HANDLE       ftnHandle
            );

    virtual void methodMustBeLoadedBeforeCodeIsRun(
            CORINFO_METHOD_HANDLE       method
            );

    virtual CORINFO_METHOD_HANDLE mapMethodDeclToMethodImpl(
            CORINFO_METHOD_HANDLE       method
            );

    virtual void getGSCookie(
            GSCookie * pCookieVal,                     // OUT
            GSCookie ** ppCookieVal                    // OUT
            );

    /**********************************************************************************/
    // ICorModuleInfo
    /**********************************************************************************/

    virtual void resolveToken(/* IN, OUT */ CORINFO_RESOLVED_TOKEN * pResolvedToken);

#if COR_JIT_EE_VERSION > 460
    virtual bool tryResolveToken(/* IN, OUT */ CORINFO_RESOLVED_TOKEN * pResolvedToken);
#endif

    virtual void findSig (
            CORINFO_MODULE_HANDLE       module,     /* IN */
            unsigned                    sigTOK,     /* IN */
            CORINFO_CONTEXT_HANDLE      context,    /* IN */
            CORINFO_SIG_INFO           *sig         /* OUT */
            );

    virtual void findCallSiteSig (
            CORINFO_MODULE_HANDLE       module,     /* IN */
            unsigned                    methTOK,    /* IN */
            CORINFO_CONTEXT_HANDLE      context,    /* IN */
            CORINFO_SIG_INFO           *sig         /* OUT */
            );

    virtual CORINFO_CLASS_HANDLE getTokenTypeAsHandle (
            CORINFO_RESOLVED_TOKEN *    pResolvedToken /* IN  */);

    virtual CorInfoCanSkipVerificationResult canSkipVerification (
            CORINFO_MODULE_HANDLE       module     /* IN  */
            );

    virtual BOOL isValidToken (
            CORINFO_MODULE_HANDLE       module,     /* IN  */
            unsigned                    metaTOK     /* IN  */
            );

    virtual BOOL isValidStringRef (
            CORINFO_MODULE_HANDLE       module,     /* IN  */
            unsigned                    metaTOK     /* IN  */
            );

    virtual BOOL shouldEnforceCallvirtRestriction(
            CORINFO_MODULE_HANDLE   scope
            );

    /**********************************************************************************/
    // ICorClassInfo
    /**********************************************************************************/

    virtual CorInfoType asCorInfoType (
            CORINFO_CLASS_HANDLE    cls
            );

    virtual const char* getClassName (
            CORINFO_CLASS_HANDLE    cls
            );

    virtual int appendClassName(
            __deref_inout_ecount(*pnBufLen) WCHAR** ppBuf,
            int* pnBufLen,
            CORINFO_CLASS_HANDLE    cls,
            BOOL fNamespace,
            BOOL fFullInst,
            BOOL fAssembly
            );

    virtual BOOL isValueClass(CORINFO_CLASS_HANDLE cls);

    virtual BOOL canInlineTypeCheckWithObjectVTable(CORINFO_CLASS_HANDLE cls);

    virtual DWORD getClassAttribs (
            CORINFO_CLASS_HANDLE    cls
            );

    virtual BOOL isStructRequiringStackAllocRetBuf(CORINFO_CLASS_HANDLE cls);

    virtual CORINFO_MODULE_HANDLE getClassModule (
            CORINFO_CLASS_HANDLE    cls
            );

    virtual CORINFO_ASSEMBLY_HANDLE getModuleAssembly (
            CORINFO_MODULE_HANDLE   mod
            );

    virtual const char* getAssemblyName (
            CORINFO_ASSEMBLY_HANDLE assem
            );

    virtual void* LongLifetimeMalloc(size_t sz);
    virtual void LongLifetimeFree(void* obj);

    virtual size_t getClassModuleIdForStatics (
            CORINFO_CLASS_HANDLE    cls,
            CORINFO_MODULE_HANDLE *pModule,
            void **ppIndirection
            );

    virtual unsigned getClassSize (
            CORINFO_CLASS_HANDLE        cls
            );

    virtual unsigned getClassAlignmentRequirement (
            CORINFO_CLASS_HANDLE        cls,
            BOOL                        fDoubleAlignHint = FALSE
            );

    virtual unsigned getClassGClayout (
            CORINFO_CLASS_HANDLE        cls,        /* IN */
            BYTE                       *gcPtrs      /* OUT */
            );

    virtual unsigned getClassNumInstanceFields (
            CORINFO_CLASS_HANDLE        cls        /* IN */
            );

    virtual CORINFO_FIELD_HANDLE getFieldInClass(
            CORINFO_CLASS_HANDLE clsHnd,
            INT num
            );

    virtual BOOL checkMethodModifier(
            CORINFO_METHOD_HANDLE hMethod,
            LPCSTR modifier,
            BOOL fOptional
            );

    virtual CorInfoHelpFunc getNewHelper(
            CORINFO_RESOLVED_TOKEN * pResolvedToken,
            CORINFO_METHOD_HANDLE    callerHandle
            );

    virtual CorInfoHelpFunc getNewArrHelper(
            CORINFO_CLASS_HANDLE        arrayCls
            );

    virtual CorInfoHelpFunc getCastingHelper(
            CORINFO_RESOLVED_TOKEN * pResolvedToken,
            bool fThrowing
            );

    virtual CorInfoHelpFunc getSharedCCtorHelper(
            CORINFO_CLASS_HANDLE clsHnd
            );

    virtual CorInfoHelpFunc getSecurityPrologHelper(
            CORINFO_METHOD_HANDLE   ftn
            );

    virtual CORINFO_CLASS_HANDLE  getTypeForBox(
            CORINFO_CLASS_HANDLE        cls
            );

    virtual CorInfoHelpFunc getBoxHelper(
            CORINFO_CLASS_HANDLE        cls
            );

    virtual CorInfoHelpFunc getUnBoxHelper(
            CORINFO_CLASS_HANDLE        cls
            );

#if COR_JIT_EE_VERSION > 460
    virtual bool getReadyToRunHelper(
            CORINFO_RESOLVED_TOKEN *        pResolvedToken,
            CORINFO_LOOKUP_KIND *           pGenericLookupKind,
            CorInfoHelpFunc                 id,
            CORINFO_CONST_LOOKUP *          pLookup
            );

    virtual void getReadyToRunDelegateCtorHelper(
            CORINFO_RESOLVED_TOKEN * pTargetMethod,
            CORINFO_CLASS_HANDLE     delegateType,
            CORINFO_CONST_LOOKUP *   pLookup
            );
#else
    virtual void getReadyToRunHelper(
            CORINFO_RESOLVED_TOKEN * pResolvedToken,
            CorInfoHelpFunc          id,
            CORINFO_CONST_LOOKUP *   pLookup
            );
#endif

    virtual const char* getHelperName(
            CorInfoHelpFunc
            );

    virtual CorInfoInitClassResult initClass(
            CORINFO_FIELD_HANDLE    field,          // Non-NULL - inquire about cctor trigger before static field access
                                                    // NULL - inquire about cctor trigger in method prolog
            CORINFO_METHOD_HANDLE   method,         // Method referencing the field or prolog
            CORINFO_CONTEXT_HANDLE  context,        // Exact context of method
            BOOL                    speculative = FALSE     // TRUE means don't actually run it
            );

    virtual void classMustBeLoadedBeforeCodeIsRun(
            CORINFO_CLASS_HANDLE        cls
            );

    virtual CORINFO_CLASS_HANDLE getBuiltinClass (
            CorInfoClassId              classId
            );

    virtual CorInfoType getTypeForPrimitiveValueClass(
            CORINFO_CLASS_HANDLE        cls
            );

    virtual BOOL canCast(
            CORINFO_CLASS_HANDLE        child,  // subtype (extends parent)
            CORINFO_CLASS_HANDLE        parent  // base type
            );

    virtual BOOL areTypesEquivalent(
            CORINFO_CLASS_HANDLE        cls1,
            CORINFO_CLASS_HANDLE        cls2
            );

    virtual CORINFO_CLASS_HANDLE mergeClasses(
            CORINFO_CLASS_HANDLE        cls1,
            CORINFO_CLASS_HANDLE        cls2
            );

    virtual CORINFO_CLASS_HANDLE getParentType (
            CORINFO_CLASS_HANDLE        cls
            );

    virtual CorInfoType getChildType (
            CORINFO_CLASS_HANDLE       clsHnd,
            CORINFO_CLASS_HANDLE       *clsRet
            );

    virtual BOOL satisfiesClassConstraints(
            CORINFO_CLASS_HANDLE cls
            );

    virtual BOOL isSDArray(
            CORINFO_CLASS_HANDLE        cls
            );

    virtual unsigned getArrayRank(
            CORINFO_CLASS_HANDLE        cls
            );

    virtual void * getArrayInitializationData(
            CORINFO_FIELD_HANDLE        field,
            DWORD                       size
            );

    virtual CorInfoIsAccessAllowedResult canAccessClass(
                        CORINFO_RESOLVED_TOKEN * pResolvedToken,
                        CORINFO_METHOD_HANDLE   callerHandle,
                        CORINFO_HELPER_DESC    *pAccessHelper /* If canAccessMethod returns something other
                                                                 than ALLOWED, then this is filled in. */
                        );

    /**********************************************************************************/
    // ICorFieldInfo
    /**********************************************************************************/

    virtual const char* getFieldName (
                        CORINFO_FIELD_HANDLE        ftn,        /* IN */
                        const char                **moduleName  /* OUT */
                        );

    virtual CORINFO_CLASS_HANDLE getFieldClass (
                        CORINFO_FIELD_HANDLE    field
                        );

    virtual CorInfoType getFieldType(
                        CORINFO_FIELD_HANDLE    field,
                        CORINFO_CLASS_HANDLE   *structType,
                        CORINFO_CLASS_HANDLE    memberParent = NULL /* IN */
                        );

    virtual unsigned getFieldOffset(
                        CORINFO_FIELD_HANDLE    field
                        );

    virtual bool isWriteBarrierHelperRequired(
                        CORINFO_FIELD_HANDLE    field);

    virtual void getFieldInfo (CORINFO_RESOLVED_TOKEN * pResolvedToken,
                               CORINFO_METHOD_HANDLE  callerHandle,
                               CORINFO_ACCESS_FLAGS   flags,
                               CORINFO_FIELD_INFO    *pResult
                              );

    virtual bool isFieldStatic(CORINFO_FIELD_HANDLE fldHnd);

    /*********************************************************************************/
    // ICorDebugInfo
    /*********************************************************************************/

    virtual void getBoundaries(
                CORINFO_METHOD_HANDLE   ftn,                // [IN] method of interest
                unsigned int           *cILOffsets,         // [OUT] size of pILOffsets
                DWORD                 **pILOffsets,         // [OUT] IL offsets of interest
                                                            //       jit MUST free with freeArray!
                ICorDebugInfo::BoundaryTypes *implictBoundaries // [OUT] tell jit, all boundries of this type
                );

    virtual void setBoundaries(
                CORINFO_METHOD_HANDLE   ftn,            // [IN] method of interest
                ULONG32                 cMap,           // [IN] size of pMap
                ICorDebugInfo::OffsetMapping *pMap      // [IN] map including all points of interest.
                                                        //      jit allocated with allocateArray, EE frees
                );

    virtual void getVars(
            CORINFO_METHOD_HANDLE           ftn,            // [IN]  method of interest
            ULONG32                        *cVars,          // [OUT] size of 'vars'
            ICorDebugInfo::ILVarInfo       **vars,          // [OUT] scopes of variables of interest
                                                            //       jit MUST free with freeArray!
            bool                           *extendOthers    // [OUT] it TRUE, then assume the scope
                                                            //       of unmentioned vars is entire method
            );

    virtual void setVars(
            CORINFO_METHOD_HANDLE           ftn,            // [IN] method of interest
            ULONG32                         cVars,          // [IN] size of 'vars'
            ICorDebugInfo::NativeVarInfo   *vars            // [IN] map telling where local vars are stored at what points
                                                            //      jit allocated with allocateArray, EE frees
            );

    /*-------------------------- Misc ---------------------------------------*/

    virtual void * allocateArray(
                        ULONG              cBytes
                        );

    virtual void freeArray(
            void               *array
            );

/*********************************************************************************/
//
// ICorArgInfo
//
/*********************************************************************************/

    virtual CORINFO_ARG_LIST_HANDLE getArgNext (
            CORINFO_ARG_LIST_HANDLE     args            /* IN */
            );

    virtual CorInfoTypeWithMod getArgType (
            CORINFO_SIG_INFO*           sig,            /* IN */
            CORINFO_ARG_LIST_HANDLE     args,           /* IN */
            CORINFO_CLASS_HANDLE       *vcTypeRet       /* OUT */
            );

    virtual CORINFO_CLASS_HANDLE getArgClass (
            CORINFO_SIG_INFO*           sig,            /* IN */
            CORINFO_ARG_LIST_HANDLE     args            /* IN */
            );

    virtual CorInfoType getHFAType (
            CORINFO_CLASS_HANDLE hClass
            );

 /*****************************************************************************
 * ICorErrorInfo contains methods to deal with SEH exceptions being thrown
 * from the corinfo interface.  These methods may be called when an exception
 * with code EXCEPTION_COMPLUS is caught.
 *****************************************************************************/

    virtual HRESULT GetErrorHRESULT(
            struct _EXCEPTION_POINTERS *pExceptionPointers
            );

    virtual ULONG GetErrorMessage(
            __inout_ecount(bufferLength) LPWSTR buffer,
            ULONG bufferLength
            );

    virtual int FilterException(
            struct _EXCEPTION_POINTERS *pExceptionPointers
            );

    virtual void HandleException(
            struct _EXCEPTION_POINTERS *pExceptionPointers
            );

    virtual void ThrowExceptionForJitResult(
            HRESULT result);

    virtual void ThrowExceptionForHelper(
            const CORINFO_HELPER_DESC * throwHelper);

#if COR_JIT_EE_VERSION > 460
    virtual bool runWithErrorTrap(
        void (*function)(void*), // The function to run
        void* parameter          // The context parameter that will be passed to the function and the handler
        );
#endif

/*****************************************************************************
 * ICorStaticInfo contains EE interface methods which return values that are
 * constant from invocation to invocation.  Thus they may be embedded in
 * persisted information like statically generated code. (This is of course
 * assuming that all code versions are identical each time.)
 *****************************************************************************/

    virtual void getEEInfo(
                CORINFO_EE_INFO            *pEEInfoOut
                );

    virtual LPCWSTR getJitTimeLogFilename();

    /*********************************************************************************/
    /*********************************************************************************/

    virtual mdMethodDef getMethodDefFromMethod(
            CORINFO_METHOD_HANDLE hMethod
            );

    virtual const char* getMethodName (
            CORINFO_METHOD_HANDLE       ftn,        /* IN */
            const char                **moduleName  /* OUT */
            );

    virtual unsigned getMethodHash (
            CORINFO_METHOD_HANDLE       ftn         /* IN */
            );

    virtual size_t findNameOfToken (
            CORINFO_MODULE_HANDLE       module,     /* IN  */
            mdToken                     metaTOK,     /* IN  */
            __out_ecount (FQNameCapacity) char * szFQName, /* OUT */
            size_t FQNameCapacity  /* IN */
            );

#if COR_JIT_EE_VERSION > 460

    virtual bool getSystemVAmd64PassStructInRegisterDescriptor(
        /* IN */    CORINFO_CLASS_HANDLE        structHnd,
        /* OUT */   SYSTEMV_AMD64_CORINFO_STRUCT_REG_PASSING_DESCRIPTOR* structPassInRegDescPtr
        );

#endif // COR_JIT_EE_VERSION

    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////

    virtual DWORD getThreadTLSIndex(
                    void                  **ppIndirection = NULL
                    );

    virtual const void * getInlinedCallFrameVptr(
                    void                  **ppIndirection = NULL
                    );

    virtual LONG * getAddrOfCaptureThreadGlobal(
                    void                  **ppIndirection = NULL
                    );

    virtual SIZE_T*       getAddrModuleDomainID(CORINFO_MODULE_HANDLE   module);

    virtual void* getHelperFtn (
                    CorInfoHelpFunc         ftnNum,
                    void                  **ppIndirection = NULL
                    );

    virtual void getFunctionEntryPoint(
                              CORINFO_METHOD_HANDLE   ftn,                 /* IN  */
                              CORINFO_CONST_LOOKUP *  pResult,             /* OUT */
                              CORINFO_ACCESS_FLAGS    accessFlags = CORINFO_ACCESS_ANY);

    virtual void getFunctionFixedEntryPoint(
                              CORINFO_METHOD_HANDLE   ftn,
                              CORINFO_CONST_LOOKUP *  pResult);

    virtual void* getMethodSync(
                    CORINFO_METHOD_HANDLE               ftn,
                    void                  **ppIndirection = NULL
                    );

    virtual CorInfoHelpFunc getLazyStringLiteralHelper(
                    CORINFO_MODULE_HANDLE   handle
                    );

    virtual CORINFO_MODULE_HANDLE embedModuleHandle(
                    CORINFO_MODULE_HANDLE   handle,
                    void                  **ppIndirection = NULL
                    );

    virtual CORINFO_CLASS_HANDLE embedClassHandle(
                    CORINFO_CLASS_HANDLE    handle,
                    void                  **ppIndirection = NULL
                    );

    virtual CORINFO_METHOD_HANDLE embedMethodHandle(
                    CORINFO_METHOD_HANDLE   handle,
                    void                  **ppIndirection = NULL
                    );

    virtual CORINFO_FIELD_HANDLE embedFieldHandle(
                    CORINFO_FIELD_HANDLE    handle,
                    void                  **ppIndirection = NULL
                    );

    virtual void embedGenericHandle(
                        CORINFO_RESOLVED_TOKEN *        pResolvedToken,
                        BOOL                            fEmbedParent, // TRUE - embeds parent type handle of the field/method handle
                        CORINFO_GENERICHANDLE_RESULT *  pResult);

    virtual CORINFO_LOOKUP_KIND getLocationOfThisType(
                    CORINFO_METHOD_HANDLE context
                    );

    virtual void* getPInvokeUnmanagedTarget(
                    CORINFO_METHOD_HANDLE   method,
                    void                  **ppIndirection = NULL
                    );

    virtual void* getAddressOfPInvokeFixup(
                    CORINFO_METHOD_HANDLE   method,
                    void                  **ppIndirection = NULL
                    );

#if COR_JIT_EE_VERSION > 460
    virtual void getAddressOfPInvokeTarget(
                    CORINFO_METHOD_HANDLE  method,
                    CORINFO_CONST_LOOKUP  *pLookup
                    );
#endif

    virtual LPVOID GetCookieForPInvokeCalliSig(
            CORINFO_SIG_INFO* szMetaSig,
            void           ** ppIndirection = NULL
            );

    virtual bool canGetCookieForPInvokeCalliSig(
                    CORINFO_SIG_INFO* szMetaSig
                    );

    virtual CORINFO_JUST_MY_CODE_HANDLE getJustMyCodeHandle(
                    CORINFO_METHOD_HANDLE       method,
                    CORINFO_JUST_MY_CODE_HANDLE**ppIndirection = NULL
                    );

    virtual void GetProfilingHandle(
                    BOOL                      *pbHookFunction,
                    void                     **pProfilerHandle,
                    BOOL                      *pbIndirectedHandles
                    );

    virtual void getCallInfo(
                        // Token info
                        CORINFO_RESOLVED_TOKEN * pResolvedToken,

                        //Generics info
                        CORINFO_RESOLVED_TOKEN * pConstrainedResolvedToken,

                        //Security info
                        CORINFO_METHOD_HANDLE   callerHandle,

                        //Jit info
                        CORINFO_CALLINFO_FLAGS  flags,

                        //out params
                        CORINFO_CALL_INFO       *pResult
                        );

    virtual BOOL canAccessFamily(CORINFO_METHOD_HANDLE hCaller,
                                           CORINFO_CLASS_HANDLE hInstanceType);

    virtual BOOL isRIDClassDomainID(CORINFO_CLASS_HANDLE cls);

    virtual unsigned getClassDomainID (
                    CORINFO_CLASS_HANDLE    cls,
                    void                  **ppIndirection = NULL
                    );

    virtual void* getFieldAddress(
                    CORINFO_FIELD_HANDLE    field,
                    void                  **ppIndirection = NULL
                    );

    virtual CORINFO_VARARGS_HANDLE getVarArgsHandle(
                    CORINFO_SIG_INFO       *pSig,
                    void                  **ppIndirection = NULL
                    );

    virtual bool canGetVarArgsHandle(
                    CORINFO_SIG_INFO       *pSig
                    );

    virtual InfoAccessType constructStringLiteral(
                    CORINFO_MODULE_HANDLE   module,
                    mdToken                 metaTok,
                    void                  **ppValue
                    );

    virtual InfoAccessType emptyStringLiteral(
                    void                  **ppValue
                    );

    virtual DWORD getFieldThreadLocalStoreID (
                    CORINFO_FIELD_HANDLE    field,
                    void                  **ppIndirection = NULL
                    );

    virtual void setOverride(
                ICorDynamicInfo             *pOverride,
                CORINFO_METHOD_HANDLE       currentMethod
                );

    virtual void addActiveDependency(
               CORINFO_MODULE_HANDLE       moduleFrom,
               CORINFO_MODULE_HANDLE       moduleTo
                );

    virtual CORINFO_METHOD_HANDLE GetDelegateCtor(
            CORINFO_METHOD_HANDLE  methHnd,
            CORINFO_CLASS_HANDLE   clsHnd,
            CORINFO_METHOD_HANDLE  targetMethodHnd,
            DelegateCtorArgs *     pCtorData
            );

    virtual void MethodCompileComplete(
                CORINFO_METHOD_HANDLE methHnd
                );

    virtual void* getTailCallCopyArgsThunk (
                    CORINFO_SIG_INFO       *pSig,
                    CorInfoHelperTailCallSpecialHandling flags
                    );

    //////////////////////////////////////////////////////////////////////////////////
    // class ICorJitInfo : public ICorDynamicInfo
    //////////////////////////////////////////////////////////////////////////////////

    virtual IEEMemoryManager* getMemoryManager();

    virtual void allocMem (
            ULONG               hotCodeSize,    /* IN */
            ULONG               coldCodeSize,   /* IN */
            ULONG               roDataSize,     /* IN */
            ULONG               xcptnsCount,    /* IN */
            CorJitAllocMemFlag  flag,           /* IN */
            void **             hotCodeBlock,   /* OUT */
            void **             coldCodeBlock,  /* OUT */
            void **             roDataBlock     /* OUT */
            );

    virtual void reserveUnwindInfo (
            BOOL                isFunclet,             /* IN */
            BOOL                isColdCode,            /* IN */
            ULONG               unwindSize             /* IN */
            );

    virtual void allocUnwindInfo (
            BYTE *              pHotCode,              /* IN */
            BYTE *              pColdCode,             /* IN */
            ULONG               startOffset,           /* IN */
            ULONG               endOffset,             /* IN */
            ULONG               unwindSize,            /* IN */
            BYTE *              pUnwindBlock,          /* IN */
            CorJitFuncKind      funcKind               /* IN */
            );

        // Get a block of memory needed for the code manager information,
        // (the info for enumerating the GC pointers while crawling the
        // stack frame).
        // Note that allocMem must be called first
    virtual void * allocGCInfo (
            size_t                  size        /* IN */
            );

    virtual void yieldExecution();

    virtual void setEHcount (
            unsigned                cEH          /* IN */
            );

    virtual void setEHinfo (
            unsigned                 EHnumber,   /* IN  */
            const CORINFO_EH_CLAUSE *clause      /* IN */
            );

    virtual BOOL logMsg(unsigned level, const char* fmt, va_list args);

    virtual int doAssert(const char* szFile, int iLine, const char* szExpr);

    virtual void reportFatalError(CorJitResult result);

    virtual HRESULT allocBBProfileBuffer (
            ULONG                 count,           // The number of basic blocks that we have
            ProfileBuffer **      profileBuffer
            );

    virtual HRESULT getBBProfileData(
            CORINFO_METHOD_HANDLE ftnHnd,
            ULONG *               count,           // The number of basic blocks that we have
            ProfileBuffer **      profileBuffer,
            ULONG *               numRuns
            );

    virtual void recordCallSite(
            ULONG                 instrOffset,  /* IN */
            CORINFO_SIG_INFO *    callSig,      /* IN */
            CORINFO_METHOD_HANDLE methodHandle  /* IN */
            );

    virtual void recordRelocation(
            void *                 location,   /* IN  */
            void *                 target,     /* IN  */
            WORD                   fRelocType, /* IN  */
            WORD                   slotNum = 0,  /* IN  */
            INT32                  addlDelta = 0 /* IN  */
            );

    virtual WORD getRelocTypeHint(void * target);

    virtual void getModuleNativeEntryPointRange(
            void ** pStart, /* OUT */
            void ** pEnd    /* OUT */
            );

    virtual DWORD getExpectedTargetArchitecture();

#if COR_JIT_EE_VERSION > 460
    virtual DWORD getJitFlags(
        CORJIT_FLAGS* flags,       /* IN */
        DWORD        sizeInBytes   /* IN */

        );
#endif

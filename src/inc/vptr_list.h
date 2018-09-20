// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Any class with a vtable that needs to be instantiated
// during debugging data access must be listed here.

VPTR_CLASS(Thread)

VPTR_CLASS(EEJitManager)

#ifdef FEATURE_PREJIT 
VPTR_CLASS(NativeImageJitManager)
#endif // FEATURE_PREJIT
#ifdef FEATURE_READYTORUN
VPTR_CLASS(ReadyToRunJitManager)
#endif
VPTR_CLASS(EECodeManager)

VPTR_CLASS(RangeList)
VPTR_CLASS(LockedRangeList)

#ifdef EnC_SUPPORTED 
VPTR_CLASS(EditAndContinueModule)
#endif
VPTR_CLASS(Module)
VPTR_CLASS(ReflectionModule)

VPTR_CLASS(AppDomain)
VPTR_CLASS(SharedDomain)
VPTR_CLASS(SystemDomain)

VPTR_CLASS(DomainAssembly)
VPTR_CLASS(PrecodeStubManager)
VPTR_CLASS(StubLinkStubManager)
VPTR_CLASS(ThePreStubManager)
VPTR_CLASS(ThunkHeapStubManager)
VPTR_CLASS(VirtualCallStubManager)
VPTR_CLASS(VirtualCallStubManagerManager)
VPTR_CLASS(JumpStubStubManager)
VPTR_CLASS(RangeSectionStubManager)
VPTR_CLASS(ILStubManager)
VPTR_CLASS(InteropDispatchStubManager)
VPTR_CLASS(DelegateInvokeStubManager)
VPTR_CLASS(TailCallStubManager)
VPTR_CLASS(PEFile)
VPTR_CLASS(PEAssembly)
VPTR_CLASS(PEImageLayout)
VPTR_CLASS(RawImageLayout)
VPTR_CLASS(ConvertedImageLayout)
VPTR_CLASS(MappedImageLayout)
#if !defined(CROSSGEN_COMPILE) && !defined(FEATURE_PAL)
VPTR_CLASS(LoadedImageLayout)
#endif // !CROSSGEN_COMPILE && !FEATURE_PAL
VPTR_CLASS(FlatImageLayout)
#ifdef FEATURE_COMINTEROP 
VPTR_CLASS(ComMethodFrame)
VPTR_CLASS(ComPlusMethodFrame)
VPTR_CLASS(ComPrestubMethodFrame)
#endif // FEATURE_COMINTEROP
VPTR_CLASS(ContextTransitionFrame)
#ifdef FEATURE_INTERPRETER
VPTR_CLASS(InterpreterFrame)
#endif // FEATURE_INTERPRETER
VPTR_CLASS(DebuggerClassInitMarkFrame)
VPTR_CLASS(DebuggerSecurityCodeMarkFrame)
VPTR_CLASS(DebuggerExitFrame)
VPTR_CLASS(DebuggerU2MCatchHandlerFrame)
VPTR_CLASS(FaultingExceptionFrame)
VPTR_CLASS(FuncEvalFrame)
VPTR_CLASS(GCFrame)
VPTR_CLASS(HelperMethodFrame)
VPTR_CLASS(HelperMethodFrame_1OBJ)
VPTR_CLASS(HelperMethodFrame_2OBJ)
VPTR_CLASS(HelperMethodFrame_PROTECTOBJ)
#ifdef FEATURE_HIJACK
VPTR_CLASS(HijackFrame)
#endif
VPTR_CLASS(InlinedCallFrame)
VPTR_CLASS(SecureDelegateFrame)
VPTR_CLASS(SecurityContextFrame)
VPTR_CLASS(MulticastFrame)
VPTR_CLASS(PInvokeCalliFrame)
VPTR_CLASS(PrestubMethodFrame)
VPTR_CLASS(ProtectByRefsFrame)
VPTR_CLASS(ProtectValueClassFrame)
#ifdef FEATURE_HIJACK
VPTR_CLASS(ResumableFrame)
VPTR_CLASS(RedirectedThreadFrame)
#endif
VPTR_CLASS(StubDispatchFrame)
VPTR_CLASS(ExternalMethodFrame)
#ifdef FEATURE_READYTORUN
VPTR_CLASS(DynamicHelperFrame)
#endif
#if !defined(_TARGET_X86_)
VPTR_CLASS(StubHelperFrame)
#endif
#if defined(_TARGET_X86_)
VPTR_CLASS(UMThkCallFrame)
#endif
VPTR_CLASS(TailCallFrame)
VPTR_CLASS(ExceptionFilterFrame)

#ifdef _DEBUG
VPTR_CLASS(AssumeByrefFromJITStack)
#endif 

#ifdef DEBUGGING_SUPPORTED 
VPTR_CLASS(Debugger)
VPTR_CLASS(EEDbgInterfaceImpl)
#endif // DEBUGGING_SUPPORTED

VPTR_CLASS(DebuggerController)
VPTR_CLASS(DebuggerMethodInfoTable)
VPTR_CLASS(DebuggerPatchTable)

VPTR_CLASS(LoaderCodeHeap)
VPTR_CLASS(HostCodeHeap)

VPTR_CLASS(GlobalLoaderAllocator)
VPTR_CLASS(AppDomainLoaderAllocator)
VPTR_CLASS(AssemblyLoaderAllocator)

VPTR_CLASS(AssemblySecurityDescriptor)
VPTR_CLASS(ApplicationSecurityDescriptor)

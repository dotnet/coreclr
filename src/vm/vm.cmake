# Needed due to the cmunged files being in the binary folders, the set(CMAKE_INCLUDE_CURRENT_DIR ON) is not enough
include_directories(BEFORE ${VM_DIR}) 

include_directories(${CLR_DIR}/src/gc)

include_directories(${VM_DIR}/${ARCH_SOURCES_DIR})

add_definitions(-DFEATURE_LEAVE_RUNTIME_HOLDER=1)

add_definitions(-DUNICODE)
add_definitions(-D_UNICODE)

# Add the Merge flag here is needed
add_definitions(-DFEATURE_MERGE_JIT_AND_ENGINE)

if(CMAKE_CONFIGURATION_TYPES) # multi-configuration generator?
  foreach (Config DEBUG CHECKED)  
     set_property(DIRECTORY APPEND PROPERTY COMPILE_DEFINITIONS $<$<CONFIG:${Config}>:WRITE_BARRIER_CHECK=1>)
  endforeach (Config)
else()
  if(UPPERCASE_CMAKE_BUILD_TYPE STREQUAL DEBUG OR UPPERCASE_CMAKE_BUILD_TYPE STREQUAL CHECKED)
    add_definitions(-DWRITE_BARRIER_CHECK=1)
  endif(UPPERCASE_CMAKE_BUILD_TYPE STREQUAL DEBUG OR UPPERCASE_CMAKE_BUILD_TYPE STREQUAL CHECKED)
endif(CMAKE_CONFIGURATION_TYPES)

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

set(VM_SOURCES_DAC_AND_WKS_COMMON
    ${VM_DIR}/appdomain.cpp
    ${VM_DIR}/array.cpp
    ${VM_DIR}/assembly.cpp
    ${VM_DIR}/baseassemblyspec.cpp
    ${VM_DIR}/binder.cpp
    ${VM_DIR}/ceeload.cpp
    ${VM_DIR}/class.cpp
    ${VM_DIR}/classhash.cpp
    ${VM_DIR}/clsload.cpp
    ${VM_DIR}/codeman.cpp
    ${VM_DIR}/comdelegate.cpp
    ${VM_DIR}/contractimpl.cpp
    ${VM_DIR}/coreassemblyspec.cpp
    ${VM_DIR}/corebindresult.cpp
    ${VM_DIR}/corhost.cpp
    ${VM_DIR}/crst.cpp
    ${VM_DIR}/debugdebugger.cpp
    ${VM_DIR}/debughelp.cpp
    ${VM_DIR}/debuginfostore.cpp
    ${VM_DIR}/decodemd.cpp
    ${VM_DIR}/disassembler.cpp
    ${VM_DIR}/dllimport.cpp
    ${VM_DIR}/domainfile.cpp
    ${VM_DIR}/dynamicmethod.cpp
    ${VM_DIR}/ecall.cpp
    ${VM_DIR}/eedbginterfaceimpl.cpp
    ${VM_DIR}/eehash.cpp
    ${VM_DIR}/eetwain.cpp
    ${VM_DIR}/encee.cpp
    ${VM_DIR}/excep.cpp
    ${VM_DIR}/exstate.cpp
    ${VM_DIR}/field.cpp
    ${VM_DIR}/formattype.cpp
    ${VM_DIR}/fptrstubs.cpp
    ${VM_DIR}/frames.cpp
    ${VM_DIR}/../gc/gccommon.cpp
    ${VM_DIR}/../gc/gcscan.cpp
    ${VM_DIR}/../gc/gcsvr.cpp
    ${VM_DIR}/../gc/gcwks.cpp
    ${VM_DIR}/genericdict.cpp
    ${VM_DIR}/generics.cpp
    ${VM_DIR}/../gc/handletable.cpp
    ${VM_DIR}/../gc/handletablecore.cpp
    ${VM_DIR}/../gc/handletablescan.cpp
    ${VM_DIR}/hash.cpp
    ${VM_DIR}/hillclimbing.cpp
    ${VM_DIR}/ilstubcache.cpp
    ${VM_DIR}/ilstubresolver.cpp
    ${VM_DIR}/inlinetracking.cpp
    ${VM_DIR}/instmethhash.cpp
    ${VM_DIR}/jitinterface.cpp
    ${VM_DIR}/loaderallocator.cpp
    ${VM_DIR}/memberload.cpp
    ${VM_DIR}/method.cpp
    ${VM_DIR}/methodimpl.cpp
    ${VM_DIR}/methoditer.cpp
    ${VM_DIR}/methodtable.cpp
    ${VM_DIR}/object.cpp
    ${VM_DIR}/../gc/objecthandle.cpp
    ${VM_DIR}/pefile.cpp
    ${VM_DIR}/peimage.cpp
    ${VM_DIR}/peimagelayout.cpp
    ${VM_DIR}/perfmap.cpp
    ${VM_DIR}/precode.cpp
    ${VM_DIR}/prestub.cpp
    ${VM_DIR}/rejit.cpp
    ${VM_DIR}/securitydescriptor.cpp
    ${VM_DIR}/securitydescriptorassembly.cpp
    ${VM_DIR}/sigformat.cpp
    ${VM_DIR}/siginfo.cpp
    ${VM_DIR}/stackwalk.cpp
    ${VM_DIR}/stublink.cpp
    ${VM_DIR}/stubmgr.cpp
    ${VM_DIR}/syncblk.cpp
    ${VM_DIR}/threadpoolrequest.cpp
    ${VM_DIR}/threads.cpp
    ${VM_DIR}/threadstatics.cpp
    ${VM_DIR}/typectxt.cpp
    ${VM_DIR}/typedesc.cpp
    ${VM_DIR}/typehandle.cpp
    ${VM_DIR}/typehash.cpp
    ${VM_DIR}/typestring.cpp
    ${VM_DIR}/util.cpp
    ${VM_DIR}/vars.cpp
    ${VM_DIR}/virtualcallstub.cpp
    ${VM_DIR}/win32threadpool.cpp
    ${VM_DIR}/zapsig.cpp
)

if(FEATURE_READYTORUN)
    list(APPEND VM_SOURCES_DAC_AND_WKS_COMMON
        ${VM_DIR}/readytoruninfo.cpp
    )
endif(FEATURE_READYTORUN)

set(VM_SOURCES_DAC
    ${VM_SOURCES_DAC_AND_WKS_COMMON}
    ${VM_DIR}/contexts.cpp
    ${VM_DIR}/threaddebugblockinginfo.cpp
)

set(VM_SOURCES_WKS
    ${VM_SOURCES_DAC_AND_WKS_COMMON}
    ${VM_DIR}/appdomainnative.cpp
    ${VM_DIR}/appdomainstack.cpp
    ${VM_DIR}/assemblyname.cpp
    ${VM_DIR}/assemblynative.cpp
    ${VM_DIR}/assemblyspec.cpp
    ${VM_DIR}/cachelinealloc.cpp
    ${VM_DIR}/callhelpers.cpp
    ${VM_DIR}/ceemain.cpp
    ${VM_DIR}/clrex.cpp
    ${VM_DIR}/clrprivbinderutil.cpp
    ${VM_DIR}/clrvarargs.cpp
    ${VM_DIR}/comdatetime.cpp
    ${VM_DIR}/comdependenthandle.cpp
    ${VM_DIR}/comdynamic.cpp
    ${VM_DIR}/comisolatedstorage.cpp
    ${VM_DIR}/commemoryfailpoint.cpp
    ${VM_DIR}/commodule.cpp
    ${VM_DIR}/compatibilityswitch.cpp
    ${VM_DIR}/comsynchronizable.cpp
    ${VM_DIR}/comthreadpool.cpp
    ${VM_DIR}/comutilnative.cpp
    ${VM_DIR}/comwaithandle.cpp
    ${VM_DIR}/constrainedexecutionregion.cpp
    ${VM_DIR}/coverage.cpp
    ${VM_DIR}/customattribute.cpp
    ${VM_DIR}/custommarshalerinfo.cpp
    ${VM_DIR}/dbggcinfodecoder.cpp
    ${VM_DIR}/dllimportcallback.cpp
    ${VM_DIR}/eeconfig.cpp
    ${VM_DIR}/eecontract.cpp
    ${VM_DIR}/eemessagebox.cpp
    ${VM_DIR}/eepolicy.cpp
    ${VM_DIR}/eetoprofinterfaceimpl.cpp
    ${VM_DIR}/eventstore.cpp
    ${VM_DIR}/fcall.cpp
    ${VM_DIR}/fieldmarshaler.cpp
    ${VM_DIR}/finalizerthread.cpp
    ${VM_DIR}/frameworkexceptionloader.cpp
    ${VM_DIR}/gccover.cpp
    ${VM_DIR}/gcenv.ee.cpp
    ${VM_DIR}/gcenv.os.cpp
    ${VM_DIR}/gchelpers.cpp
    ${VM_DIR}/genmeth.cpp
    ${VM_DIR}/../gc/gceesvr.cpp
    ${VM_DIR}/../gc/gceewks.cpp
    ${VM_DIR}/../gc/handletablecache.cpp
    ${VM_DIR}/hosting.cpp
    ${VM_DIR}/ibclogger.cpp
    ${VM_DIR}/ilmarshalers.cpp
    ${VM_DIR}/interopconverter.cpp
    ${VM_DIR}/interoputil.cpp
    ${VM_DIR}/interpreter.cpp
    ${VM_DIR}/invokeutil.cpp
    ${VM_DIR}/jithelpers.cpp
    ${VM_DIR}/listlock.cpp
    ${VM_DIR}/managedmdimport.cpp
    ${VM_DIR}/marshalnative.cpp
    ${VM_DIR}/marvin32.cpp
    ${VM_DIR}/mdaassistants.cpp
    ${VM_DIR}/methodtablebuilder.cpp
    ${VM_DIR}/mlinfo.cpp
    ${VM_DIR}/mscorlib.cpp # <DisablePrecompiledHeaders>true</DisablePrecompiledHeaders>
    ${VM_DIR}/multicorejit.cpp # Condition="'$(FeatureMulticoreJIT)' == 'true'
    ${VM_DIR}/multicorejitplayer.cpp # Condition="'$(FeatureMulticoreJIT)' == 'true'
    ${VM_DIR}/nativeoverlapped.cpp
    ${VM_DIR}/objectlist.cpp
    ${VM_DIR}/olevariant.cpp
    ${VM_DIR}/pefingerprint.cpp
    ${VM_DIR}/pendingload.cpp
    ${VM_DIR}/perfdefaults.cpp
    ${VM_DIR}/profattach.cpp
    ${VM_DIR}/profattachclient.cpp
    ${VM_DIR}/profattachserver.cpp
    ${VM_DIR}/profdetach.cpp
    ${VM_DIR}/profilermetadataemitvalidator.cpp
    ${VM_DIR}/profilingenumerators.cpp
    ${VM_DIR}/profilinghelper.cpp
    ${VM_DIR}/proftoeeinterfaceimpl.cpp
    ${VM_DIR}/qcall.cpp
    ${VM_DIR}/reflectclasswriter.cpp
    ${VM_DIR}/reflectioninvocation.cpp
    ${VM_DIR}/runtimehandles.cpp
    ${VM_DIR}/safehandle.cpp
    ${VM_DIR}/security.cpp
    ${VM_DIR}/securityattributes.cpp
    ${VM_DIR}/securitydeclarative.cpp
    ${VM_DIR}/securitydeclarativecache.cpp
    ${VM_DIR}/securitydescriptorappdomain.cpp
    ${VM_DIR}/securityhostprotection.cpp
    ${VM_DIR}/securitymeta.cpp
    ${VM_DIR}/securitypolicy.cpp
    ${VM_DIR}/securitytransparentassembly.cpp
    ${VM_DIR}/sha1.cpp
    ${VM_DIR}/simplerwlock.cpp
    ${VM_DIR}/sourceline.cpp
    ${VM_DIR}/spinlock.cpp
    ${VM_DIR}/stackingallocator.cpp
    ${VM_DIR}/stringliteralmap.cpp
    ${VM_DIR}/stubcache.cpp
    ${VM_DIR}/stubgen.cpp
    ${VM_DIR}/stubhelpers.cpp
    ${VM_DIR}/syncclean.cpp
    ${VM_DIR}/synch.cpp
    ${VM_DIR}/synchronizationcontextnative.cpp
    ${VM_DIR}/testhookmgr.cpp
    ${VM_DIR}/threaddebugblockinginfo.cpp
    ${VM_DIR}/threadsuspend.cpp
    ${VM_DIR}/typeparse.cpp
    ${VM_DIR}/verifier.cpp
    ${VM_DIR}/weakreferencenative.cpp
)

if(FEATURE_EVENT_TRACE)
    list(APPEND VM_SOURCES_WKS
        ${VM_DIR}/eventtrace.cpp
        )
endif(FEATURE_EVENT_TRACE)

if(WIN32)

set(VM_SOURCES_DAC_AND_WKS_WIN32
    ${VM_DIR}/clrtocomcall.cpp
    ${VM_DIR}/rcwwalker.cpp
    ${VM_DIR}/winrttypenameconverter.cpp
)

list(APPEND VM_SOURCES_WKS 
    ${VM_SOURCES_DAC_AND_WKS_WIN32}
    # These should not be included for Linux
    ${VM_DIR}/appxutil.cpp
    ${VM_DIR}/assemblynativeresource.cpp
    ${VM_DIR}/classcompat.cpp
    ${VM_DIR}/classfactory.cpp
    ${VM_DIR}/clrprivbinderwinrt.cpp
    ${VM_DIR}/clrprivtypecachewinrt.cpp
    ${VM_DIR}/comcache.cpp
    ${VM_DIR}/comcallablewrapper.cpp
    ${VM_DIR}/comconnectionpoints.cpp
    ${VM_DIR}/cominterfacemarshaler.cpp
    ${VM_DIR}/commtmemberinfomap.cpp
    ${VM_DIR}/comtoclrcall.cpp
    ${VM_DIR}/dispatchinfo.cpp
    ${VM_DIR}/dispparammarshaler.cpp
    ${VM_DIR}/dwreport.cpp
    ${VM_DIR}/eventreporter.cpp
    ${VM_DIR}/extensibleclassfactory.cpp
    ${VM_DIR}/microsoft.comservices_i.c
    ${VM_DIR}/mngstdinterfaces.cpp
    ${VM_DIR}/notifyexternals.cpp
    ${VM_DIR}/olecontexthelpers.cpp    ${VM_DIR}/
    ${VM_DIR}/rcwrefcache.cpp
    ${VM_DIR}/rtlfunctions.cpp
    ${VM_DIR}/runtimecallablewrapper.cpp
    ${VM_DIR}/securityprincipal.cpp
    ${VM_DIR}/stacksampler.cpp
    ${VM_DIR}/stdinterfaces.cpp
    ${VM_DIR}/stdinterfaces_wrapper.cpp
    ${VM_DIR}/winrthelpers.cpp    ${VM_DIR}/
)

list(APPEND VM_SOURCES_DAC 
    ${VM_SOURCES_DAC_AND_WKS_WIN32}
    # These should not be included for Linux
    ${VM_DIR}/clrprivbinderwinrt.cpp
    ${VM_DIR}/clrprivtypecachewinrt.cpp
)

if(CLR_CMAKE_TARGET_ARCH_AMD64)
    set(VM_SOURCES_WKS_ARCH_ASM
        ${VM_DIR}/${ARCH_SOURCES_DIR}/AsmHelpers.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/CallDescrWorkerAMD64.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/ComCallPreStub.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/CrtHelpers.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/GenericComCallStubs.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/GenericComPlusCallStubs.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/getstate.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/InstantiatingStub.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/JitHelpers_Fast.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/JitHelpers_FastWriteBarriers.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/JitHelpers_InlineGetAppDomain.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/JitHelpers_InlineGetThread.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/JitHelpers_Slow.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/PInvokeStubs.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/RedirectedHandledJITCase.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/ThePreStubAMD64.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/ExternalMethodFixupThunk.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/TlsGetters.asm # Condition="'$(FeatureImplicitTls)' != 'true'
        ${VM_DIR}/${ARCH_SOURCES_DIR}/UMThunkStub.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/VirtualCallStubAMD64.asm
    )
elseif(CLR_CMAKE_TARGET_ARCH_I386)
    set(VM_SOURCES_WKS_ARCH_ASM
        ${VM_DIR}/${ARCH_SOURCES_DIR}/RedirectedHandledJITCase.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/asmhelpers.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/fptext.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/gmsasm.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/jithelp.asm
    )
elseif(CLR_CMAKE_TARGET_ARCH_ARM64)
    set(VM_SOURCES_WKS_ARCH_ASM
        ${VM_DIR}/${ARCH_SOURCES_DIR}/AsmHelpers.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/CallDescrWorkerARM64.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/CrtHelpers.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/PInvokeStubs.asm
    )

endif()

else(WIN32)

    if(CLR_CMAKE_TARGET_ARCH_AMD64)
        set(VM_SOURCES_WKS_ARCH_ASM
            ${VM_DIR}/${ARCH_SOURCES_DIR}/calldescrworkeramd64.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/crthelpers.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/externalmethodfixupthunk.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/getstate.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/jithelpers_fast.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/jithelpers_fastwritebarriers.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/jithelpers_slow.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/pinvokestubs.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/theprestubamd64.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/unixasmhelpers.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/umthunkstub.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/virtualcallstubamd64.S
        )
    elseif(CLR_CMAKE_TARGET_ARCH_ARM)
        set(VM_SOURCES_WKS_ARCH_ASM
            ${VM_DIR}/${ARCH_SOURCES_DIR}/asmhelpers.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/crthelpers.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/ehhelpers.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/memcpy.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/patchedcode.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/pinvokestubs.S
        )
    elseif(CLR_CMAKE_TARGET_ARCH_ARM64)
        set(VM_SOURCES_WKS_ARCH_ASM
            ${VM_DIR}/${ARCH_SOURCES_DIR}/asmhelpers.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/calldescrworkerarm64.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/crthelpers.S
            ${VM_DIR}/${ARCH_SOURCES_DIR}/pinvokestubs.S
        )
    endif()
    
endif(WIN32)


if(CLR_CMAKE_TARGET_ARCH_AMD64)
    set(VM_SOURCES_DAC_AND_WKS_ARCH
        ${VM_DIR}/${ARCH_SOURCES_DIR}/cgenamd64.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/excepamd64.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/gmsamd64.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/stublinkeramd64.cpp
    )
    
    set(VM_SOURCES_WKS_ARCH
        ${VM_DIR}/${ARCH_SOURCES_DIR}/jithelpersamd64.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/jitinterfaceamd64.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/profiler.cpp
        ${VM_DIR}/exceptionhandling.cpp
        ${VM_DIR}/gcinfodecoder.cpp
        ${VM_DIR}/jitinterfacegen.cpp
    )
elseif(CLR_CMAKE_TARGET_ARCH_I386)
    set(VM_SOURCES_DAC_AND_WKS_ARCH
        ${VM_DIR}/gcdecode.cpp
        ${VM_DIR}/exinfo.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/cgenx86.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/excepx86.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/gmsx86.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/stublinkerx86.cpp
    )
    
    set(VM_SOURCES_WKS_ARCH
        ${VM_DIR}/${ARCH_SOURCES_DIR}/jithelp.asm
        ${VM_DIR}/${ARCH_SOURCES_DIR}/jitinterfacex86.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/profiler.cpp
    )
elseif(CLR_CMAKE_TARGET_ARCH_ARM)
    set(VM_SOURCES_DAC_AND_WKS_ARCH
        ${VM_DIR}/${ARCH_SOURCES_DIR}/exceparm.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/stubs.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/armsinglestepper.cpp
    )
    
    set(VM_SOURCES_WKS_ARCH
        ${VM_DIR}/${ARCH_SOURCES_DIR}/jithelpersarm.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/profiler.cpp
        ${VM_DIR}/exceptionhandling.cpp
        ${VM_DIR}/gcinfodecoder.cpp
    )
elseif(CLR_CMAKE_TARGET_ARCH_ARM64)
    set(VM_SOURCES_DAC_AND_WKS_ARCH
        ${VM_DIR}/${ARCH_SOURCES_DIR}/cgenarm64.cpp
        ${VM_DIR}/${ARCH_SOURCES_DIR}/stubs.cpp
        ${VM_DIR}/exceptionhandling.cpp
        ${VM_DIR}/gcinfodecoder.cpp
    )
endif()

if(CLR_CMAKE_PLATFORM_UNIX)
    list(APPEND VM_SOURCES_WKS_ARCH
        ${VM_DIR}/${ARCH_SOURCES_DIR}/unixstubs.cpp
    )
endif(CLR_CMAKE_PLATFORM_UNIX)

set(VM_SOURCES_DAC_ARCH
    ${VM_DIR}/gcinfodecoder.cpp
    ${VM_DIR}/dbggcinfodecoder.cpp
    ${VM_DIR}/exceptionhandling.cpp
)

list(APPEND VM_SOURCES_WKS 
    ${VM_SOURCES_WKS_ARCH}
    ${VM_SOURCES_DAC_AND_WKS_ARCH}
)

list(APPEND VM_SOURCES_DAC
    ${VM_SOURCES_DAC_ARCH}
    ${VM_SOURCES_DAC_AND_WKS_ARCH}
)

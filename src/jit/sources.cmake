set( JIT_SOURCES
  ${JIT_DIR}/alloc.cpp
  ${JIT_DIR}/assertionprop.cpp
  ${JIT_DIR}/bitset.cpp
  ${JIT_DIR}/block.cpp
  ${JIT_DIR}/codegencommon.cpp
  ${JIT_DIR}/compiler.cpp
  ${JIT_DIR}/copyprop.cpp
  ${JIT_DIR}/disasm.cpp
  ${JIT_DIR}/earlyprop.cpp
  ${JIT_DIR}/ee_il_dll.cpp
  ${JIT_DIR}/eeinterface.cpp
  ${JIT_DIR}/emit.cpp
  ${JIT_DIR}/error.cpp
  ${JIT_DIR}/flowgraph.cpp
  ${JIT_DIR}/gcdecode.cpp
  ${JIT_DIR}/gcencode.cpp
  ${JIT_DIR}/gcinfo.cpp
  ${JIT_DIR}/gentree.cpp
  ${JIT_DIR}/gschecks.cpp
  ${JIT_DIR}/hashbv.cpp
  ${JIT_DIR}/hostallocator.cpp
  ${JIT_DIR}/importer.cpp
  ${JIT_DIR}/inline.cpp
  ${JIT_DIR}/inlinepolicy.cpp
  ${JIT_DIR}/instr.cpp
  ${JIT_DIR}/jitconfig.cpp
  ${JIT_DIR}/jiteh.cpp
  ${JIT_DIR}/jittelemetry.cpp
  ${JIT_DIR}/lclvars.cpp
  ${JIT_DIR}/liveness.cpp
  ${JIT_DIR}/loopcloning.cpp
  ${JIT_DIR}/lower.cpp
  ${JIT_DIR}/lsra.cpp
  ${JIT_DIR}/morph.cpp
  ${JIT_DIR}/optcse.cpp
  ${JIT_DIR}/optimizer.cpp
  ${JIT_DIR}/rangecheck.cpp
  ${JIT_DIR}/rationalize.cpp
  ${JIT_DIR}/regalloc.cpp
  ${JIT_DIR}/register_arg_convention.cpp
  ${JIT_DIR}/regset.cpp
  ${JIT_DIR}/scopeinfo.cpp
  ${JIT_DIR}/sharedfloat.cpp
  ${JIT_DIR}/sm.cpp
  ${JIT_DIR}/smdata.cpp
  ${JIT_DIR}/smweights.cpp
  ${JIT_DIR}/ssabuilder.cpp
  ${JIT_DIR}/ssarenamestate.cpp
  ${JIT_DIR}/typeinfo.cpp
  ${JIT_DIR}/unwind.cpp
  ${JIT_DIR}/utils.cpp
  ${JIT_DIR}/valuenum.cpp
)

if(CLR_CMAKE_TARGET_ARCH_AMD64)
  set( ARCH_SOURCES
    ${JIT_DIR}/codegenxarch.cpp
    ${JIT_DIR}/emitxarch.cpp
    ${JIT_DIR}/lowerxarch.cpp
    ${JIT_DIR}/simd.cpp
    ${JIT_DIR}/simdcodegenxarch.cpp
    ${JIT_DIR}/targetamd64.cpp
    ${JIT_DIR}/unwindamd64.cpp
  )
elseif(CLR_CMAKE_TARGET_ARCH_ARM)
  set( ARCH_SOURCES
    ${JIT_DIR}/codegenarm.cpp
    ${JIT_DIR}/emitarm.cpp
    ${JIT_DIR}/lowerarm.cpp
    ${JIT_DIR}/targetarm.cpp
    ${JIT_DIR}/unwindarm.cpp
  )
elseif(CLR_CMAKE_TARGET_ARCH_I386)
  set( ARCH_SOURCES
    ${JIT_DIR}/codegenxarch.cpp
    ${JIT_DIR}/emitxarch.cpp
    ${JIT_DIR}/lowerxarch.cpp
    ${JIT_DIR}/simd.cpp
    ${JIT_DIR}/simdcodegenxarch.cpp
    ${JIT_DIR}/targetx86.cpp
  )
elseif(CLR_CMAKE_TARGET_ARCH_ARM64)
  set( ARCH_SOURCES
    ${JIT_DIR}/codegenarm64.cpp
    ${JIT_DIR}/emitarm64.cpp
    ${JIT_DIR}/lowerarm64.cpp
    ${JIT_DIR}/targetarm64.cpp
    ${JIT_DIR}/unwindarm.cpp
    ${JIT_DIR}/unwindarm64.cpp
  )
else()
  clr_unknown_arch()
endif()

# The following defines all the source files used by the "legacy" back-end (#ifdef LEGACY_BACKEND).
# It is always safe to include both legacy and non-legacy files in the build, as everything is properly
# #ifdef'ed, though it makes the build slightly slower to do so. Note there is only a legacy backend for
# x86 and ARM.

if(CLR_CMAKE_PLATFORM_ARCH_AMD64)
  set( ARCH_LEGACY_SOURCES
  )
elseif(CLR_CMAKE_PLATFORM_ARCH_ARM)
  set( ARCH_LEGACY_SOURCES
    ${JIT_DIR}/codegenlegacy.cpp
    ${JIT_DIR}/registerfp.cpp
  )
elseif(CLR_CMAKE_PLATFORM_ARCH_I386)
  set( ARCH_LEGACY_SOURCES
    ${JIT_DIR}/codegenlegacy.cpp
    ${JIT_DIR}/stackfp.cpp
  )
elseif(CLR_CMAKE_PLATFORM_ARCH_ARM64)
  set( ARCH_LEGACY_SOURCES
  )
else()
  clr_unknown_arch()
endif()

set( SOURCES
  ${JIT_SOURCES}
  ${ARCH_SOURCES}
  ${ARCH_LEGACY_SOURCES}
 )

add_precompiled_header(jitpch.h ${JIT_DIR}/jitpch.cpp SOURCES)

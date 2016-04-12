if(WIN32)
    add_compile_options(/wd4996)
endif(WIN32)
  
if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

set(UTILCODE_COMMON_SOURCES
  ${UTILCODE_DIR}/clrhost_nodependencies.cpp
  ${UTILCODE_DIR}/ccomprc.cpp
  ${UTILCODE_DIR}/ex.cpp
  ${UTILCODE_DIR}/sbuffer.cpp
  ${UTILCODE_DIR}/sstring_com.cpp
  ${UTILCODE_DIR}/fstring.cpp
  ${UTILCODE_DIR}/namespaceutil.cpp
  ${UTILCODE_DIR}/makepath.cpp
  ${UTILCODE_DIR}/splitpath.cpp
  ${UTILCODE_DIR}/clrconfig.cpp
  ${UTILCODE_DIR}/configuration.cpp
  ${UTILCODE_DIR}/collections.cpp
  ${UTILCODE_DIR}/posterror.cpp
  ${UTILCODE_DIR}/fstream.cpp
  ${UTILCODE_DIR}/clrhelpers.cpp
  ${UTILCODE_DIR}/stgpool.cpp
  ${UTILCODE_DIR}/stgpooli.cpp
  ${UTILCODE_DIR}/stgpoolreadonly.cpp
  ${UTILCODE_DIR}/utsem.cpp
  ${UTILCODE_DIR}/peinformation.cpp
  ${UTILCODE_DIR}/check.cpp
  ${UTILCODE_DIR}/log.cpp
  ${UTILCODE_DIR}/apithreadstress.cpp
  ${UTILCODE_DIR}/arraylist.cpp
  ${UTILCODE_DIR}/bitvector.cpp
  ${UTILCODE_DIR}/comex.cpp
  ${UTILCODE_DIR}/delayloadhelpers.cpp
  ${UTILCODE_DIR}/guidfromname.cpp
  ${UTILCODE_DIR}/jitperf.cpp
  ${UTILCODE_DIR}/memorypool.cpp
  ${UTILCODE_DIR}/iallocator.cpp
  ${UTILCODE_DIR}/loaderheap.cpp
  ${UTILCODE_DIR}/outstring.cpp
  ${UTILCODE_DIR}/ilformatter.cpp
  ${UTILCODE_DIR}/opinfo.cpp
  ${UTILCODE_DIR}/dacutil.cpp
  ${UTILCODE_DIR}/sortversioning.cpp
  ${UTILCODE_DIR}/corimage.cpp
  ${UTILCODE_DIR}/format1.cpp
  ${UTILCODE_DIR}/prettyprintsig.cpp
  ${UTILCODE_DIR}/regutil.cpp
  ${UTILCODE_DIR}/sigbuilder.cpp
  ${UTILCODE_DIR}/sigparser.cpp
  ${UTILCODE_DIR}/sstring.cpp
  ${UTILCODE_DIR}/util_nodependencies.cpp
  ${UTILCODE_DIR}/utilmessagebox.cpp
  ${UTILCODE_DIR}/safewrap.cpp
  ${UTILCODE_DIR}/clrhost.cpp
  ${UTILCODE_DIR}/cycletimer.cpp
  ${UTILCODE_DIR}/md5.cpp
  ${UTILCODE_DIR}/util.cpp
  ${UTILCODE_DIR}/stresslog.cpp
  ${UTILCODE_DIR}/debug.cpp
  ${UTILCODE_DIR}/pedecoder.cpp
  ${UTILCODE_DIR}/winfix.cpp
  ${UTILCODE_DIR}/longfilepathwrappers.cpp 
  ${UTILCODE_DIR}/jithost.cpp
)

# These source file do not yet compile on Linux.
# They should be moved out from here into the declaration
# of UTILCODE_SOURCES above after fixing compiler errors.
if(WIN32)
  list(APPEND UTILCODE_COMMON_SOURCES 
    ${UTILCODE_DIR}/appxutil.cpp
    ${UTILCODE_DIR}/dlwrap.cpp
    ${UTILCODE_DIR}/downlevel.cpp
    ${UTILCODE_DIR}/loadrc.cpp
    ${UTILCODE_DIR}/newapis.cpp
    ${UTILCODE_DIR}/securitywrapper.cpp
    ${UTILCODE_DIR}/securityutil.cpp
    ${UTILCODE_DIR}/stacktrace.cpp
  )

  if(CLR_CMAKE_PLATFORM_ARCH_I386 OR CLR_CMAKE_PLATFORM_ARCH_ARM)
    list(APPEND UTILCODE_COMMON_SOURCES
      ${UTILCODE_DIR}/lazycow.cpp
    )
  endif()
endif(WIN32)

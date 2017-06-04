cmake_minimum_required(VERSION 2.8.12.2)

project(CoreProfiler)

add_definitions(-DPAL_STDCPP_COMPAT)

include_directories(
  BEFORE
  .
  config
  info
  misc
  stacktrace
  storage
  sync
  trace
  ${ARCH_SOURCES_DIR}
)

if(CLR_CMAKE_TARGET_ARCH_ARM)
  include_directories("${CLR_DIR}/src/pal/src/include")
endif()

set(PROFILER_SOURCES_COMMON
  classfactory.cpp
  config/environmentconfigprovider.cpp
  config/loggerconfig.cpp
  config/profilerconfig.cpp
  config/tracelogconfig.cpp
  dllmain.cpp
  info/functioninfo.cpp
  profiler.cpp
  profilerinfo.cpp
  profilermanager.cpp
  sync/shared_mutex.cpp
  trace/basetrace.cpp
  trace/commontrace.cpp
  trace/executiontrace.cpp
  trace/stackchannel.cpp
  tracelog.cpp
)

if(CLR_CMAKE_TARGET_ARCH_AMD64)
  set(PROFILER_SOURCES_ASM
    ${ARCH_SOURCES_DIR}/asmhelpers.S
    ${ARCH_SOURCES_DIR}/archhelpers.cpp
  )
elseif(CLR_CMAKE_TARGET_ARCH_ARM)
  set(PROFILER_SOURCES_ASM
    ${ARCH_SOURCES_DIR}/asmhelpers.S
    ${ARCH_SOURCES_DIR}/archhelpers.cpp
  )
endif()

add_library_clr(coreprof
  SHARED
  ${PROFILER_SOURCES_COMMON}
  ${PROFILER_SOURCES_ASM}
)

set(PROFILER_LINK_LIBRARIES
  corguids
  # utilcodestaticnohost
  # gcinfo
)

if(CLR_CMAKE_PLATFORM_UNIX)
  list(APPEND PROFILER_LINK_LIBRARIES
    # mscorrc_debug
    coreclrpal
    palrt
  )
endif(CLR_CMAKE_PLATFORM_UNIX)

target_link_libraries(coreprof
  ${PROFILER_LINK_LIBRARIES}
)

install_clr(coreprof)

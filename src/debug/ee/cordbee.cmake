add_definitions(-DFEATURE_NO_HOST)

include_directories(BEFORE ${VM_DIR})
include_directories(BEFORE ${VM_DIR}/${ARCH_SOURCES_DIR})
include_directories(BEFORE ${CMAKE_CURRENT_SOURCE_DIR})

if(CLR_CMAKE_PLATFORM_UNIX)
  add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

set(CORDBEE_SOURCES_DAC_AND_WKS
  ${CORDBEE_DIR}/controller.cpp
  ${CORDBEE_DIR}/debugger.cpp
  ${CORDBEE_DIR}/debuggermodule.cpp
  ${CORDBEE_DIR}/functioninfo.cpp
)

set(CORDBEE_SOURCES_WKS
  ${CORDBEE_SOURCES_DAC_AND_WKS}
  ${CORDBEE_DIR}/funceval.cpp
  ${CORDBEE_DIR}/rcthread.cpp
  ${CORDBEE_DIR}/canary.cpp
  ${CORDBEE_DIR}/shared.cpp
  ${CORDBEE_DIR}/frameinfo.cpp
  ${CORDBEE_DIR}/${ARCH_SOURCES_DIR}/primitives.cpp
)

set(CORDBEE_SOURCES_DAC 
  ${CORDBEE_SOURCES_DAC_AND_WKS}
)

if(CLR_CMAKE_PLATFORM_UNIX)
  list(APPEND CORDBEE_SOURCES_WKS 
    ${CORDBEE_DIR}/dactable.cpp
  )
endif(CLR_CMAKE_PLATFORM_UNIX)

if(CLR_CMAKE_TARGET_ARCH_AMD64)
  list(APPEND CORDBEE_SOURCES_WKS 
    ${CORDBEE_DIR}/${ARCH_SOURCES_DIR}/debuggerregdisplayhelper.cpp
    ${CORDBEE_DIR}/${ARCH_SOURCES_DIR}/amd64walker.cpp
  )
elseif(CLR_CMAKE_TARGET_ARCH_I386)
  list(APPEND CORDBEE_SOURCES_WKS 
    ${CORDBEE_DIR}/${ARCH_SOURCES_DIR}/debuggerregdisplayhelper.cpp
    ${CORDBEE_DIR}/${ARCH_SOURCES_DIR}/x86walker.cpp
  )
elseif(CLR_CMAKE_TARGET_ARCH_ARM)
  list(APPEND CORDBEE_SOURCES_WKS ${ARCH_SOURCES_DIR}/armwalker.cpp)
elseif(CLR_CMAKE_TARGET_ARCH_ARM64)
  list(APPEND CORDBEE_SOURCES_WKS ${ARCH_SOURCES_DIR}/arm64walker.cpp)
endif()
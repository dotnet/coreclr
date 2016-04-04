include_directories(BEFORE ${VM_DIR})
include_directories(BEFORE ${VM_DIR}/${ARCH_SOURCES_DIR})
include_directories(BEFORE ${CMAKE_CURRENT_SOURCE_DIR})
include_directories(BEFORE ${CLR_DIR}/src/unwinder)
include_directories(${CLR_DIR}/src/debug/ee)
include_directories(${CLR_DIR}/src/gc)
include_directories(${CLR_DIR}/src/gcdump)
include_directories(${CLR_DIR}/src/debug/daccess)

set(UNWINDER_SOURCES
    ${UNWINDER_DIR}/unwinder.cpp
)

if(NOT DEFINED CLR_CMAKE_PLATFORM_ARCH_I386)    
    include_directories(${ARCH_SOURCES_DIR})
    list(APPEND UNWINDER_SOURCES
        ${UNWINDER_DIR}/${ARCH_SOURCES_DIR}/unwinder_${ARCH_SOURCES_DIR}.cpp
    )
endif()

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

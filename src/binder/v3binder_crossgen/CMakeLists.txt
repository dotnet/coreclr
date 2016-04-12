include(${CLR_DIR}/crossgen.cmake)
set(BINDER_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../sources.cmake)

set(CMAKE_INCLUDE_CURRENT_DIR ON)
include_directories(BEFORE "../../vm/${ARCH_SOURCES_DIR}")
include_directories(BEFORE "../../vm")
include_directories(BEFORE "../inc")

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

add_library_clr(v3binder_crossgen
    STATIC
    ${BINDER_COMMON_SOURCES}
)

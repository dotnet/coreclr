include(${CLR_DIR}/crossgen.cmake)
set(JIT_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../sources.cmake)

set(CMAKE_INCLUDE_CURRENT_DIR ON)
include_directories("../jitstd")

if (CLR_CMAKE_PLATFORM_ARCH_AMD64)
  add_definitions(-DFEATURE_SIMD) 
  add_definitions(-DFEATURE_AVX_SUPPORT) 
endif (CLR_CMAKE_PLATFORM_ARCH_AMD64)

if(CLR_CMAKE_PLATFORM_ARCH_I386 OR CLR_CMAKE_PLATFORM_ARCH_ARM)
  add_definitions(-DLEGACY_BACKEND)
endif(CLR_CMAKE_PLATFORM_ARCH_I386 OR CLR_CMAKE_PLATFORM_ARCH_ARM)

add_library_clr(jit_crossgen ${SOURCES})

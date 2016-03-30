set(CMAKE_INCLUDE_CURRENT_DIR ON)

include_directories("./jitstd")
include_directories("../inc")

# Enable the following for UNIX altjit on Windows
# add_definitions(-DALT_JIT)

if (CLR_CMAKE_PLATFORM_ARCH_AMD64)
  add_definitions(-DFEATURE_SIMD) 
  add_definitions(-DFEATURE_AVX_SUPPORT) 
endif (CLR_CMAKE_PLATFORM_ARCH_AMD64)

set(JIT_DIR ${CMAKE_CURRENT_SOURCE_DIR})
include(sources.cmake)

if( WIN32 )
  # Create .def file containing a list of exports preceeded by
  # 'EXPORTS'.  The file "ClrJit.exports" already contains the list, so we
  # massage it into the correct format here to create "ClrJit.exports.def".
  set(CLRJIT_EXPORTS_DEF ${CMAKE_CURRENT_BINARY_DIR}/ClrJit.exports.def)
  set(CLRJIT_EXPORTS_DEF_TEMP ${CLRJIT_EXPORTS_DEF}.txt)
  file(READ "ClrJit.exports" exports_list)
  file(WRITE ${CLRJIT_EXPORTS_DEF_TEMP} "LIBRARY CLRJIT\n")
  file(APPEND ${CLRJIT_EXPORTS_DEF_TEMP} "EXPORTS\n")
  file(APPEND ${CLRJIT_EXPORTS_DEF_TEMP} ${exports_list})

  # Copy the file only if it has changed.
  execute_process(COMMAND ${CMAKE_COMMAND} -E copy_if_different
    ${CLRJIT_EXPORTS_DEF_TEMP} ${CLRJIT_EXPORTS_DEF})

  set(SHARED_LIB_SOURCES ${SOURCES} ${CLRJIT_EXPORTS_DEF})
else()
  set(SHARED_LIB_SOURCES ${SOURCES})
endif()

set(CLR_EXPORTED_SYMBOL_FILE ${CLRJIT_EXPORTS_DEF})

add_subdirectory(dll)
add_subdirectory(standalone)

if (CLR_CMAKE_PLATFORM_ARCH_I386 OR CLR_CMAKE_PLATFORM_ARCH_ARM)
    add_subdirectory(protojit)
endif (CLR_CMAKE_PLATFORM_ARCH_I386 OR CLR_CMAKE_PLATFORM_ARCH_ARM)

include(${CLR_DIR}/crossgen.cmake)

include_directories(BEFORE ${VM_DIR})
include_directories(${VM_DIR}/${ARCH_SOURCES_DIR})
include_directories(${CLR_DIR}/src/md/compiler)

add_definitions(-DSTRONGNAME_IN_VM -DSNAPI_INTERNAL)

set(STRONGNAME_SOURCES
  ../strongname.cpp
  ../strongnamecoreclr.cpp
  ../strongnameinternal.cpp
)

convert_to_absolute_path(STRONGNAME_SOURCES ${STRONGNAME_SOURCES})

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

add_library_clr(strongname_crossgen ${STRONGNAME_SOURCES})

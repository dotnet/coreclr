 
cmake_minimum_required (VERSION 2.6) 
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake") 
set(SOURCES 
    SafeHandleNative.cpp 
) 
add_library (SafeHandleNative SHARED ${SOURCES}) 
target_link_libraries(SafeHandleNative ${LINK_LIBRARIES_ADDITIONAL})
install (TARGETS SafeHandleNative DESTINATION bin/nativeassets) 

if (WIN32)
  install (FILES $<TARGET_PDB_FILE:SafeHandleNative> DESTINATION bin/nativeassets/PDB)
endif()

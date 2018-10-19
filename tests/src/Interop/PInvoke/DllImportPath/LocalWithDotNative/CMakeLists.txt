cmake_minimum_required (VERSION 2.6) 
project (DllImportPath.Local) 
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake") 
include_directories(${INC_PLATFORM_DIR}) 
set(SOURCES 
    ../DllImportPathNative.cpp 
) 
# Additional files to reference: 
# add the executable 
add_library (DllImportPath.Local SHARED ${SOURCES}) 
target_link_libraries(DllImportPath.Local ${LINK_LIBRARIES_ADDITIONAL}) 
# add the install targets 
install (TARGETS DllImportPath.Local DESTINATION bin) 

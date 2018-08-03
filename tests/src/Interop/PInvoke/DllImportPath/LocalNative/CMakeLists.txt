#VCXPROJ 
# Escaped UTF-16 name: DllImportPath_U&#xbb;n&#xbb;i&#xbb;c&#xbb;o&#xbb;d&#xbb;e
cmake_minimum_required (VERSION 2.6) 
project (DllImportPath_Local) 
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake") 
include_directories(${INC_PLATFORM_DIR}) 
set(SOURCES 
    ../DllImportPathNative.cpp 
) 
# Additional files to reference: 
# add the executable 
add_library (DllImportPath_Local SHARED ${SOURCES}) 
# add the install targets 
install (TARGETS DllImportPath_Local DESTINATION bin) 

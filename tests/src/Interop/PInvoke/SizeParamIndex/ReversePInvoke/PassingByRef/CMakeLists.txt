#VCXPROJ 
cmake_minimum_required (VERSION 2.6) 
project (ReversePInvokePassingByRefNative) 
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake") 
include_directories(${INC_PLATFORM_DIR}) 
set(SOURCES 
    ReversePInvokePassingByRefNative.cpp 
) 
# Additional files to reference: 
# add the executable 
add_library (ReversePInvokePassingByRefNative SHARED ${SOURCES}) 
# add the install targets 
install (TARGETS ReversePInvokePassingByRefNative DESTINATION bin) 

#VCXPROJ 
cmake_minimum_required (VERSION 2.6) 
project (ReversePInvokePassingByOutNative) 
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake") 
include_directories(${INC_PLATFORM_DIR}) 
set(SOURCES 
    ReversePInvokePassingByOutNative.cpp 
) 
# Additional files to reference: 
# add the executable 
add_library (ReversePInvokePassingByOutNative SHARED ${SOURCES}) 
# add the install targets 
install (TARGETS ReversePInvokePassingByOutNative DESTINATION bin) 

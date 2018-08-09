#VCXPROJ 
cmake_minimum_required (VERSION 2.6) 
project (PInvokePassingByRefNative) 
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake")
include_directories("..")
include_directories(${INC_PLATFORM_DIR}) 
set(SOURCES 
    PInvokePassingByRefNative.cpp 
) 
# Additional files to reference: 
#    PInvokePassingByRefNative.def 
# add the executable 
add_library (PInvokePassingByRefNative SHARED ${SOURCES}) 
# add the install targets 
install (TARGETS PInvokePassingByRefNative DESTINATION bin) 

#VCXPROJ 
cmake_minimum_required (VERSION 2.6) 
project (PInvokePassingByOutNative) 
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake")
include_directories("..")
include_directories(${INC_PLATFORM_DIR}) 
set(SOURCES 
    PInvokePassingByOutNative.cpp 
)
# Additional files to reference: 
#    PInvokePassingByOutNative.def 
# add the executable 
add_library (PInvokePassingByOutNative SHARED ${SOURCES}) 
# add the install targets 
install (TARGETS PInvokePassingByOutNative DESTINATION bin) 

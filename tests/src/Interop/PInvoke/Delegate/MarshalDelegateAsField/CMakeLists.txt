#VCXPROJ 
cmake_minimum_required (VERSION 2.6) 
project (PInvoke_Delegate_AsField) 
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake") 
include_directories(${INC_PLATFORM_DIR}) 
set(SOURCES 
    AsFieldNative.cpp 
) 
# Additional files to reference: 
#    AsFieldNative.def 
# add the executable 
add_library (PInvoke_Delegate_AsField SHARED ${SOURCES}) 
# add the install targets 
install (TARGETS PInvoke_Delegate_AsField DESTINATION bin) 

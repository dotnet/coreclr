#VCXPROJ 
cmake_minimum_required (VERSION 2.6) 
project (Char_BestFitMappingNative) 
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake") 
include_directories(${INC_PLATFORM_DIR}) 
set(SOURCES 
    BestFitMappingNative.cpp 
) 
# Additional files to reference: 
#    BestFitMappingNative.def 
# add the executable 
add_library (Char_BestFitMappingNative SHARED ${SOURCES}) 
# add the install targets 
install (TARGETS Char_BestFitMappingNative DESTINATION bin) 

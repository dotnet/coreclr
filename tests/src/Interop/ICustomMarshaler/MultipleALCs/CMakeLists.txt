cmake_minimum_required (VERSION 2.6)
project (CustomMarshalersALCNative)
include_directories(${INC_PLATFORM_DIR})
set(SOURCES CustomMarshalersALCNative.cpp )

# add the executable
add_library (CustomMarshalersALCNative SHARED ${SOURCES})
target_link_libraries(CustomMarshalersALCNative ${LINK_LIBRARIES_ADDITIONAL}) 

# add the install targets
install (TARGETS CustomMarshalersALCNative DESTINATION bin)

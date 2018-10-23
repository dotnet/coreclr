cmake_minimum_required (VERSION 2.6)
project (MarshalFixedBuffer)
include_directories(${INC_PLATFORM_DIR})
set(SOURCES MarshalFixedBuffer.cpp)

# add the executable
add_library (MarshalFixedBuffer SHARED ${SOURCES})
target_link_libraries(MarshalFixedBuffer ${LINK_LIBRARIES_ADDITIONAL}) 

# add the install targets
install (TARGETS MarshalFixedBuffer DESTINATION bin)

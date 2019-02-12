cmake_minimum_required (VERSION 2.6)
project (NewLib)
include_directories(${INC_PLATFORM_DIR})
set(SOURCES NewLib.cpp)

# add the executable
add_library (NewLib SHARED ${SOURCES})
target_link_libraries(NewLib ${LINK_LIBRARIES_ADDITIONAL})

# add the install targets
install (TARGETS NewLib DESTINATION bin)



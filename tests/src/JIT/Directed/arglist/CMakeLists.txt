cmake_minimum_required (VERSION 2.6)
project (vararg)
include_directories(${INC_PLATFORM_DIR})

if(!WIN32)
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fvisibility=hidden")
endif()

# add the executable
add_library (vararg SHARED varargnative.c)

# add the install targets
install (TARGETS vararg DESTINATION bin)

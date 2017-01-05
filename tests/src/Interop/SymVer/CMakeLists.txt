cmake_minimum_required (VERSION 2.6)
project (SymVerNative)
if(CMAKE_SYSTEM_NAME STREQUAL Linux)
  set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -Wl,--version-script,${CMAKE_CURRENT_SOURCE_DIR}/versionscript")
endif()
include_directories(${INC_PLATFORM_DIR})
set(SOURCES SymVerNative.cpp )

# add the executable
add_library (SymVerNative SHARED ${SOURCES})
target_link_libraries(SymVerNative ${LINK_LIBRARIES_ADDITIONAL})

# add the install targets
install (TARGETS SymVerNative DESTINATION bin)

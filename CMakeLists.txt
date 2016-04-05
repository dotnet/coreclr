# Require at least version 2.8.12 of CMake
cmake_minimum_required(VERSION 2.8.12)

# Set the project name
project(CoreCLR)

set(CLR_DIR ${CMAKE_CURRENT_SOURCE_DIR})
set(VM_DIR ${CMAKE_CURRENT_SOURCE_DIR}/src/vm)
set(GENERATED_INCLUDE_DIR ${CMAKE_CURRENT_BINARY_DIR}/src/inc)

include(common.cmake)

if(CLR_CMAKE_PLATFORM_UNIX)
  add_subdirectory(src/corefx)
endif(CLR_CMAKE_PLATFORM_UNIX)

if(CLR_CMAKE_PLATFORM_UNIX)
  add_subdirectory(src/ToolBox/SOS/lldbplugin)
  add_subdirectory(src/pal)
  add_subdirectory(src/coreclr/hosts/unixcoreruncommon)
  add_subdirectory(src/coreclr/hosts/unixcorerun)
  add_subdirectory(src/coreclr/hosts/unixcoreconsole)
  add_subdirectory(src/ildasm/unixcoreclrloader)
endif(CLR_CMAKE_PLATFORM_UNIX)

if(CLR_CMAKE_PLATFORM_DARWIN)
  add_subdirectory(src/coreclr/hosts/osxbundlerun)
endif(CLR_CMAKE_PLATFORM_DARWIN)

# Add this subdir. We install the headers for the jit.
add_subdirectory(src/pal/prebuilt/inc)

add_subdirectory(src/debug/debug-pal)

if(WIN32)
  add_subdirectory(src/gc/sample)
endif()

include(coreclr.cmake)

add_subdirectory(src)

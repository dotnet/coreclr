# Require at least version 2.8.12 of CMake
cmake_minimum_required(VERSION 2.8.12)

# Set the project name
project(CrossComponents)

set(CLR_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
set(VM_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../src/vm)
set(GENERATED_INCLUDE_DIR ${CMAKE_CURRENT_BINARY_DIR}/../src/inc)

include(../coreclrArch.cmake)
include(../common.cmake)
include(../coreclr.cmake)

#include(etw.cmake)

include_directories("../src/inc")
include_directories("../src/strongname/inc")
include_directories("../src/inc/winrt")
include_directories("../src/debug/inc")
include_directories("../src/debug/inc/${ARCH_SOURCES_DIR}")
include_directories("../src/debug/inc/dump")
include_directories("../src/md/inc")
include_directories("../src/classlibnative/bcltype")
include_directories("../src/classlibnative/cryptography")
include_directories("../src/classlibnative/inc")
include_directories("${GENERATED_INCLUDE_DIR}")

# Include the basic prebuilt headers - required for getting fileversion resource details.
include_directories("../src/pal/prebuilt/inc")

if (CLR_CMAKE_PLATFORM_UNIX)
  include_directories("../src/pal/inc")
  include_directories("../src/pal/inc/rt")
  include_directories("../src/pal/src/safecrt")
endif (CLR_CMAKE_PLATFORM_UNIX)

function(add_library_clr)
    add_library(${ARGV})
endfunction()

function(add_executable_clr)
    add_executable(${ARGV})
endfunction()

add_subdirectory(../src/tools/crossgen src/tools/crossgen)
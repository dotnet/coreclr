# Require at least version 2.8.12 of CMake
cmake_minimum_required(VERSION 2.8.12)

# Set the project name
project(CrossComponents)

add_definitions(-DCROSS_COMPILE)

set(CLR_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
set(VM_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../src/vm)
set(GENERATED_INCLUDE_DIR ${CMAKE_CURRENT_BINARY_DIR}/../src/inc)

include(../common.cmake)
include(../coreclr.cmake)

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
add_subdirectory(../src/debug/dbgutil src/debug/dbgutil)
add_subdirectory(../src/debug/debug-pal src/debug/debug-pal)
add_subdirectory(../src/dlls/mscordac src/dlls/mscordac)
add_subdirectory(../src/debug/di src/debug/di)
#TODO can we reuse staticnohost instead of creating new dir
add_subdirectory(../src/utilcode/staticnohostamd64 src/utilcode/staticnohostamd64)
add_subdirectory(../src/debug/ildbsymlib src/debug/ildbsymlib)
add_subdirectory(../src/md/compiler/dbi src/md/compiler/dbi)
add_subdirectory(../src/md/runtime/dbi src/md/runtime/dbi)
add_subdirectory(../src/md/enc/dbi src/md/enc/dbi)
add_subdirectory(../src/md/datasource src/md/datasource)
if(WIN32)
  add_subdirectory(../src/ipcman/ipcman-staticcrt src/ipcman/ipcman-staticcrt)        
  add_subdirectory(../src/md/hotdata/full-staticcrt src/md/hotdata/full-staticcrt)
  add_subdirectory(../src/md/winmd/dbi src/md/winmd/dbi)
endif(WIN32)
add_subdirectory(../src/inc src/inc)
add_subdirectory(../src/dlls/mscordbi src/dlls/mscordbi)
add_subdirectory(../src/debug/shim src/debug/shim)
add_subdirectory(../src/Toolbox/SOS/Strike src/Toolbox/SOS/Strike)
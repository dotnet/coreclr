set(CMAKE_INCLUDE_CURRENT_DIR ON)

include_directories(BEFORE ${CMAKE_CURRENT_SOURCE_DIR})
include_directories(BEFORE ${CMAKE_CURRENT_SOURCE_DIR}/env)

add_definitions(-DBUILD_AS_STANDALONE)

if(CLR_CMAKE_PLATFORM_UNIX)
  add_compile_options(-fPIC)
  # dprintf causes many warnings
  add_compile_options(-Wno-format)
endif(CLR_CMAKE_PLATFORM_UNIX)

set( GC_SOURCES
  gcconfig.cpp
  gccommon.cpp
  gcscan.cpp
  gcsvr.cpp
  gcwks.cpp
  handletable.cpp
  handletablecore.cpp
  handletablescan.cpp
  objecthandle.cpp
  softwarewritewatch.cpp
  gchandletable.cpp
  gceesvr.cpp
  gceewks.cpp
  handletablecache.cpp)

if(NOT CLR_CMAKE_PLATFORM_UNIX)
set ( GC_SOURCES
  ${GC_SOURCES}
  windows/gcenv.windows.cpp)
endif(NOT CLR_CMAKE_PLATFORM_UNIX)

convert_to_absolute_path(GC_SOURCES ${GC_SOURCES})

add_library(gc_standalone STATIC ${GC_SOURCES})

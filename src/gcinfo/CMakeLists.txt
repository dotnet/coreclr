set(CMAKE_INCLUDE_CURRENT_DIR ON)
set(GCINFO_DIR ${CMAKE_CURRENT_SOURCE_DIR})
include(sources.cmake)

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

add_subdirectory(lib)

install (FILES gcinfoencoder.cpp
               DESTINATION gcinfo)

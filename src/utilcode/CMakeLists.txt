
set(CMAKE_INCLUDE_CURRENT_DIR ON)

if(WIN32)
    add_compile_options(/wd4996)
endif(WIN32)

set(UTILCODE_DIR ${CMAKE_CURRENT_SOURCE_DIR})
include(sources.cmake)
  
if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

add_subdirectory(dac)
add_subdirectory(dyncrt)
add_subdirectory(staticnohost)
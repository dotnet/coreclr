set(UNWINDER_DIR ${CMAKE_CURRENT_SOURCE_DIR})
include(unwinder.cmake)

if(CLR_CMAKE_PLATFORM_UNIX)    
    add_subdirectory(wks)
endif(CLR_CMAKE_PLATFORM_UNIX)
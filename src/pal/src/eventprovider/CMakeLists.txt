cmake_minimum_required(VERSION 2.8.12)

set(EVENT_MANIFEST ${CMAKE_CURRENT_SOURCE_DIR}/../../../vm/ClrEtwAll.man)

add_subdirectory(dummyprovider)

# if(CLR_CMAKE_PLATFORM_LINUX OR CLR_CMAKE_PLATFORM_FREEBSD)
#   add_subdirectory(lttngprovider)
# else()
#   add_subdirectory(dummyprovider)
# endif()

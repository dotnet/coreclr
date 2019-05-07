cmake_minimum_required(VERSION 2.8.12)

set(EVENT_MANIFEST ${CMAKE_CURRENT_SOURCE_DIR}/../ClrEtwAll.man)
set(EVENT_EXCLUSIONS ${CMAKE_CURRENT_SOURCE_DIR}/../ClrEtwAllMeta.lst)

add_subdirectory(eventpipe)

if(WIN32)
  add_subdirectory(EtwProvider)
elseif(CLR_CMAKE_PLATFORM_LINUX OR CLR_CMAKE_PLATFORM_FREEBSD)
  add_subdirectory(LttngProvider)
endif()

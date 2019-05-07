cmake_minimum_required(VERSION 2.8.12)

set(EVENT_MANIFEST ${CMAKE_CURRENT_SOURCE_DIR}/../ClrEtwAll.man)
set(EVENT_EXCLUSIONS ${CMAKE_CURRENT_SOURCE_DIR}/../ClrEtwAllMeta.lst)

add_subdirectory(eventpipe)

include(../../md_dbi.cmake)
set(MDRUNTIME_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdruntime.cmake)

add_library_clr(mdruntime-dbi ${MDRUNTIME_SOURCES})
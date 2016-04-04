include(../../md_wks.cmake)
set(MDRUNTIME_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdruntime.cmake)

add_library_clr(mdruntime_wks ${MDRUNTIME_SOURCES})


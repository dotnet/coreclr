include(${CLR_DIR}/dac.cmake)
include(../../md_dac.cmake)
set(MDRUNTIME_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdruntime.cmake)

add_library_clr(mdruntime_dac ${MDRUNTIME_SOURCES})
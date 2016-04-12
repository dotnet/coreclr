include(../../md_dbi.cmake)

set(MDWINMD_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdwinmd.cmake)

add_library_clr(mdwinmd_dbi ${MDWINMD_SOURCES})
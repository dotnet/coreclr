set(MDCOMPILER_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdcompiler.cmake)
include(../../md_dbi.cmake)

add_library_clr(mdcompiler-dbi ${MDCOMPILER_SOURCES})
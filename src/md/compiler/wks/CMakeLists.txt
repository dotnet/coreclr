set(MDCOMPILER_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdcompiler.cmake)
include(../../md_wks.cmake)

add_library_clr(mdcompiler_wks ${MDCOMPILER_SOURCES})
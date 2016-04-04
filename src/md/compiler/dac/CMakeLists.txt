set(MDCOMPILER_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdcompiler.cmake)
include(${CLR_DIR}/dac.cmake)
include(../../md_dac.cmake)

add_library_clr(mdcompiler_dac ${MDCOMPILER_SOURCES})

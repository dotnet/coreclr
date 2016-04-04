include(${CLR_DIR}/dac.cmake)
include(../../md_dac.cmake)
set(MDENC_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdenc.cmake)

add_library_clr(mdruntimerw_dac ${MDRUNTIMERW_SOURCES})

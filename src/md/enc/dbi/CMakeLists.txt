include(../../md_dbi.cmake)
set(MDENC_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdenc.cmake)

add_library_clr(mdruntimerw-dbi ${MDRUNTIMERW_SOURCES})
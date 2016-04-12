include(../../md_wks.cmake)
set(MDENC_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdenc.cmake)

add_library_clr(mdruntimerw_wks ${MDRUNTIMERW_SOURCES})

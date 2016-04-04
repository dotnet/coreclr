include(${CLR_DIR}/crossgen.cmake)
include(../../md_wks.cmake)

set(MDWINMD_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../mdwinmd.cmake)

add_precompiled_header(stdafx.h ../stdafx.cpp MDWINMD_SOURCES)
add_library_clr(mdwinmd_crossgen ${MDWINMD_SOURCES})

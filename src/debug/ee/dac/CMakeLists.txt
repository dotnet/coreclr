include(${CLR_DIR}/dac.cmake)
set(CORDBEE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../cordbee.cmake)

add_precompiled_header(stdafx.h ../stdafx.cpp CORDBEE_SOURCES_DAC)

add_library_clr(cordbee_dac ${CORDBEE_SOURCES_DAC})

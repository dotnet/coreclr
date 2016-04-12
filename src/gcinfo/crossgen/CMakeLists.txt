include(${CLR_DIR}/crossgen.cmake)
set(GCINFO_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../sources.cmake)

add_library_clr(gcinfo_crossgen
    STATIC
    ${GCINFO_SOURCES}
)

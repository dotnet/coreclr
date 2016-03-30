include(${CLR_DIR}/crossgen.cmake)
include(../../md_wks.cmake)
include(../sources.cmake)

add_library_clr(mdcompiler_crossgen ${MDCOMPILER_SOURCES})

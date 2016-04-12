include(${CLR_DIR}/crossgen.cmake)
include(../../md_wks.cmake)
include(../sources.cmake)

add_library_clr(mdhotdata_crossgen ${MDHOTDATA_SOURCES})

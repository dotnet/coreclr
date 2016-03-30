include(${CLR_DIR}/crossgen.cmake)
include(../../md_wks.cmake)
include(../sources.cmake)

add_definitions(-DNO_COR)

add_library_clr(mdruntime_crossgen ${MDRUNTIME_SOURCES})

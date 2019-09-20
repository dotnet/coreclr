include(../../md_wks.cmake)

add_precompiled_header(stdafx.h ../stdafx.cpp MDRUNTIME_SOURCES)

if (WIN32)
    list(APPEND MDRUNTIME_SOURCES ${MDRUNTIME_HEADERS})
endif (WIN32)

add_library_clr(mdruntime_crossgen ${MDRUNTIME_SOURCES})
set_target_properties(mdruntime_crossgen PROPERTIES CROSSGEN_COMPONENT TRUE)

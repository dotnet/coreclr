include(../../md_wks.cmake)

add_precompiled_header(stdafx.h ../stdafx.cpp MDCOMPILER_SOURCES)

if (WIN32)
    list(APPEND MDCOMPILER_SOURCES ${MDCOMPILER_HEADERS})
endif (WIN32)

add_library_clr(mdcompiler_crossgen ${MDCOMPILER_SOURCES})

set_target_properties(mdcompiler_crossgen PROPERTIES CROSSGEN_COMPONENT TRUE)

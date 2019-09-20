
add_precompiled_header(external.h ../external.cpp MDHOTDATA_SOURCES)

if (WIN32)
    list(APPEND MDHOTDATA_SOURCES ${MDHOTDATA_HEADERS})
endif (WIN32)

add_library_clr(mdhotdata_dac ${MDHOTDATA_SOURCES})
set_target_properties(mdhotdata_dac PROPERTIES DAC_COMPONENT TRUE)

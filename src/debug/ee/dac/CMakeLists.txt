
add_precompiled_header(stdafx.h ../stdafx.cpp CORDBEE_SOURCES_DAC)

if (WIN32)
  list(APPEND CORDBEE_SOURCES_DAC ${CORDBEE_HEADERS_DAC})
endif (WIN32)

add_library_clr(cordbee_dac ${CORDBEE_SOURCES_DAC})
set_target_properties(cordbee_dac PROPERTIES DAC_COMPONENT TRUE)
add_dependencies(cordbee_dac eventing_headers)

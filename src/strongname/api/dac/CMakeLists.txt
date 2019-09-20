
add_library_clr(strongname_dac ${STRONGNAME_SOURCES})
add_dependencies(strongname_dac eventing_headers)
set_target_properties(strongname_dac PROPERTIES DAC_COMPONENT TRUE)

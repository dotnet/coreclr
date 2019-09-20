
add_definitions(-DFEATURE_NO_HOST)

add_library_clr(unwinder_dac ${UNWINDER_SOURCES})
add_dependencies(unwinder_dac eventing_headers)
set_target_properties(unwinder_dac PROPERTIES DAC_COMPONENT TRUE)

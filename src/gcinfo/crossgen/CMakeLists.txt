add_library_clr(gcinfo_crossgen
    STATIC
    ${GCINFO_SOURCES}
)

set_target_properties(gcinfo_crossgen PROPERTIES CROSSGEN_COMPONENT TRUE)

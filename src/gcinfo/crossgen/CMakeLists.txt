if(NOT DEFINED CLR_SKIP_BUILD_CROSSGEN)
include(${CLR_DIR}/crossgen.cmake)

add_library_clr(gcinfo_crossgen
    STATIC
    ${GCINFO_SOURCES}
)
endif()

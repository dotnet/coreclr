if(NOT DEFINED CLR_SKIP_BUILD_CROSSGEN)
include(${CLR_DIR}/crossgen.cmake)

add_library_clr(strongname_crossgen ${STRONGNAME_SOURCES})
add_dependencies(strongname_crossgen eventing_headers)
endif()

project(System.Net.Native)

set(CMAKE_INCLUDE_CURRENT_DIR ON)


set(NATIVENET_SOURCES
    corefxnet.cpp
    corefxnet.h
)

add_library(System.Net.Native
    SHARED
    ${NATIVENET_SOURCES}
)


# Disable the "lib" prefix.
set_target_properties(System.Net.Native PROPERTIES PREFIX "")

# This is so we can use the tracing facilities of coreclrpal library
target_link_libraries(System.Net.Native
    coreclrpal)

install (TARGETS System.Net.Native DESTINATION .)

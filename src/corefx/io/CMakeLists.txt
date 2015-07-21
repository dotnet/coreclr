
project(corefx_io)

set(CMAKE_INCLUDE_CURRENT_DIR ON)

add_compile_options(-fPIC)

set(NATIVEIO_SOURCES
    corefx_io.cpp
)

add_library(corefx_io
    SHARED
    ${NATIVEIO_SOURCES}
)

install (TARGETS corefx_io DESTINATION .)
 

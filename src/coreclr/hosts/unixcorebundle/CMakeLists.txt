project(corebundle)

include_directories(../unixcoreruncommon)
include_directories(./bundle)

add_compile_options(-fPIE)

set(COREBUNDLE_SOURCES
    corebundle.cpp
    ./bundle/dir_utils.cpp
    ./bundle/extractor.cpp
    ./bundle/file_entry.cpp
    ./bundle/header.cpp
    ./bundle/manifest.cpp
    ./bundle/marker.cpp
    ./bundle/pal.cpp
    ./bundle/reader.cpp
    ./bundle/runner.cpp
    ./bundle/trace.cpp
    ./bundle/utils.cpp
)

set(HEADERS
    ./bundle/dir_utils.h
    ./bundle/error_codes.h
    ./bundle/extractor.h
    ./bundle/file_entry.h
    ./bundle/file_type.h
    ./bundle/header.h
    ./bundle/manifest.h
    ./bundle/marker.h
    ./bundle/pal.h
    ./bundle/reader.h
    ./bundle/runner.h
    ./bundle/trace.h
    ./bundle/utils.h
)

_add_executable(corebundle
    ${COREBUNDLE_SOURCES}
)

# FreeBSD and NetBSD implement dlopen(3) in libc
if(NOT CMAKE_SYSTEM_NAME STREQUAL FreeBSD AND NOT CMAKE_SYSTEM_NAME STREQUAL NetBSD)
    target_link_libraries(corebundle
        dl
    )
endif(NOT CMAKE_SYSTEM_NAME STREQUAL FreeBSD AND NOT CMAKE_SYSTEM_NAME STREQUAL NetBSD)

target_link_libraries(corebundle
    unixcoreruncommon
)

# Libc turns locks into no-ops if pthread was not loaded into process yet. Loading
# pthread by the process executable ensures that all locks are initialized properly.
if(NOT CLR_CMAKE_PLATFORM_ANDROID)
  target_link_libraries(corebundle
    pthread
  )
endif()

install_clr(corebundle)

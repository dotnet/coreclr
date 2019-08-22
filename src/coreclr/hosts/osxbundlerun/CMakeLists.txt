project(osxbundlerun)

include_directories(../unixcoreruncommon)

add_compile_options(-fPIE)

set(CMAKE_INCLUDE_CURRENT_DIR ON)
include(../unixcoreruncommon/configure.cmake)

set(CORERUN_SOURCES 
    osxbundlerun.cpp 
    ../unixcoreruncommon/coreruncommon.cpp
)

_add_executable(osxbundlerun
    ${CORERUN_SOURCES}
)

target_link_libraries(osxbundlerun 
    dl
)

add_dependencies(osxbundlerun
    coreclr
)

install_clr(osxbundlerun)

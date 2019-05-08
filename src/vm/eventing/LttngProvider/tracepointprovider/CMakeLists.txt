cmake_minimum_required(VERSION 2.8.12.2)

set(CMAKE_INCLUDE_CURRENT_DIR ON)

add_definitions(-DPAL_STDCPP_COMPAT=1)
include_directories(${COREPAL_SOURCE_DIR}/inc/rt)
include_directories(../lttng/)
add_compile_options(-fPIC)

add_library(coreclrtraceptprovider
    SHARED
    ${TRACEPOINT_PROVIDER_SOURCES}
)

find_library(LTTNG NAMES lttng-ust)

target_link_libraries(coreclrtraceptprovider
                      ${LTTNG}
)

set_target_properties(coreclrtraceptprovider PROPERTIES LINKER_LANGUAGE CXX)

# Install the static coreclrtraceptprovider library
install_clr(coreclrtraceptprovider)

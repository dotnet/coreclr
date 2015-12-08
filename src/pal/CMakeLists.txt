cmake_minimum_required(VERSION 2.8.12.2)

project(COREPAL)

set(PAL_CMAKE_ENABLE_TRACE 0)

include_directories(${COREPAL_SOURCE_DIR}/inc)
include_directories(${COREPAL_SOURCE_DIR}/src)
include_directories(${COREPAL_SOURCE_DIR}/../inc)

add_compile_options(-gdwarf-3)
add_compile_options(-fexceptions)
add_compile_options(-fstack-protector-all)

add_subdirectory(src)
add_subdirectory(tests)


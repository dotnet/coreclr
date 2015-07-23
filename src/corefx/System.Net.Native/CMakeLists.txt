

set(CMAKE_INCLUDE_CURRENT_DIR ON)
project(System.Net.Native)
add_compile_options(-fPIC)

add_definitions(-DPLATFORM_UNIX=1)
add_definitions(-D__LINUX__=1)
add_definitions(-DLP64COMPATIBLE=1)
add_definitions(-DFEATURE_PAL=1)
add_definitions(-DCORECLR=1)
add_definitions(-DPIC=1)
add_definitions(-DBIT64=1)
add_definitions(-D_WIN64=1)

include_directories(${COREPAL_SOURCE_DIR}/src/include)
include_directories(${COREPAL_SOURCE_DIR}/inc)
include_directories(${COREPAL_SOURCE_DIR}/src)
include_directories(${COREPAL_SOURCE_DIR}/../inc)
include_directories(${COREPAL_BINARY_DIR}/src)


add_subdirectory(src)
add_subdirectory(tests)


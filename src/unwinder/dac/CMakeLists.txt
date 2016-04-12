include(${CLR_DIR}/dac.cmake)
set(UNWINDER_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../unwinder.cmake)

add_definitions(-DFEATURE_NO_HOST)

add_library_clr(unwinder_dac ${UNWINDER_SOURCES})

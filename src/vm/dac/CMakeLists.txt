include(${CLR_DIR}/dac.cmake)
include(../vm.cmake)

add_precompiled_header(common.h ../common.cpp VM_SOURCES_DAC)
add_library_clr(cee_dac ${VM_SOURCES_DAC})

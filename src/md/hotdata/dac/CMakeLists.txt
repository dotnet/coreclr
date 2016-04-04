if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

include(${CLR_DIR}/dac.cmake)
include(../sources.cmake)

add_library_clr(mdhotdata_dac ${MDHOTDATA_SOURCES})

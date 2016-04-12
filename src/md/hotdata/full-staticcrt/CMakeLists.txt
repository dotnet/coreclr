add_definitions(-D_CRTIMP=) # static link of crt
include(../sources.cmake)

add_library_clr(mdhotdata-staticcrt ${MDHOTDATA_SOURCES})

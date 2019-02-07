project(pedecoder)
add_definitions(-DDISABLE_CONTRACTS)

set (PEDECODER_SOURCES
     ../../utilcode/pedecoder.cpp
     portable_pedecoder.cpp)

_add_library(pedecoder STATIC ${PEDECODER_SOURCES})

_install(TARGETS pedecoder DESTINATION lib)
_install(FILES portable_pedecoder.h DESTINATION inc)

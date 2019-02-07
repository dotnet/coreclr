project(pedecoder)
add_definitions(-DDISABLE_CONTRACTS)

set (PEDECODER_SOURCES
     ../../utilcode/pedecoder.cpp
     portable_pedecoder.cpp)

add_library_clr(pedecoder STATIC ${PEDECODER_SOURCES})

install_clr(pedecoder)

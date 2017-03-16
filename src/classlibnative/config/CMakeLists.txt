set(CMAKE_INCLUDE_CURRENT_DIR ON)

include_directories("../inc")

set(CONFIG_SOURCES
    clrconfiguration.cpp
)

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

add_library_clr(config_wks
    STATIC
    ${CONFIG_SOURCES}
)

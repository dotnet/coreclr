add_definitions(-DFEATURE_NO_HOST)
add_definitions(-DSELF_NO_HOST)

include_directories(${CLR_DIR}/src/debug/shim)
include_directories(${CLR_DIR}/src/vm/)

# TODO: Might need to selectively include this directory based on the target architecture 
include_directories(${CLR_DIR}/src/vm/amd64)

set(PROFATTACH_SOURCES
    profattach.cpp
)

if(WIN32)
    # Use static crt
    add_definitions(-MT) 
    add_definitions(-DFX_VER_INTERNALNAME_STR=profattach.dll)

    list(APPEND PROFATTACH_SOURCES
        profattach.rc
    )

    preprocess_def_file(${CMAKE_CURRENT_SOURCE_DIR}/profattach.ntdef ${CMAKE_CURRENT_BINARY_DIR}/profattach.def)
    list(APPEND PROFATTACH_SOURCES ${CMAKE_CURRENT_BINARY_DIR}/profattach.def)
else(WIN32)
    set(DEF_SOURCES ${CMAKE_CURRENT_SOURCE_DIR}/profattach_unixexports.src)
    set(EXPORTS_FILE ${CMAKE_CURRENT_BINARY_DIR}/profattach.exports)
    generate_exports_file(${DEF_SOURCES} ${EXPORTS_FILE})
endif(WIN32)

if(CLR_CMAKE_PLATFORM_LINUX OR CLR_CMAKE_PLATFORM_FREEBSD OR CLR_CMAKE_PLATFORM_NETBSD)
    # This option is necessary to ensure that the overloaded delete operator defined inside
    # of the utilcode will be used instead of the standard library delete operator.
    set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -Xlinker -Bsymbolic -Xlinker -Bsymbolic-functions")    

    # Add linker exports file option
    set(EXPORTS_LINKER_OPTION -Wl,--version-script=${EXPORTS_FILE})
endif(CLR_CMAKE_PLATFORM_LINUX OR CLR_CMAKE_PLATFORM_FREEBSD OR CLR_CMAKE_PLATFORM_NETBSD)

if(CLR_CMAKE_PLATFORM_DARWIN)
    # Add linker exports file option
    set(EXPORTS_LINKER_OPTION -Wl,-exported_symbols_list,${EXPORTS_FILE})
endif(CLR_CMAKE_PLATFORM_DARWIN)

add_library_clr(profattach SHARED ${PROFATTACH_SOURCES})

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)

    add_custom_target(profattach_exports DEPENDS ${EXPORTS_FILE})
    add_dependencies(profattach profattach_exports)

    set_property(TARGET profattach APPEND_STRING PROPERTY LINK_FLAGS ${EXPORTS_LINKER_OPTION})
    set_property(TARGET profattach APPEND_STRING PROPERTY LINK_DEPENDS ${EXPORTS_FILE})
endif(CLR_CMAKE_PLATFORM_UNIX)

set(PROFATTACH_LIBRARIES
    debug-pal
    corguids
    dbgutil
    utilcodestaticnohost
)

if(WIN32)
    list(APPEND PROFATTACH_LIBRARIES
        kernel32.lib
        ${STATIC_MT_CRT_LIB}
        ${STATIC_MT_VCRT_LIB}
        uuid.lib
        user32.lib
        advapi32.lib
        oleaut32.lib
        WtsApi32.lib
        version.lib
        psapi.lib
    )
else()
    list(APPEND PROFATTACH_LIBRARIES
        coreclrpal
        mscorrc_debug
        palrt
    )
endif(WIN32)

target_link_libraries(profattach ${PROFATTACH_LIBRARIES})

# add the install targets
install_clr(profattach)

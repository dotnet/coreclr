project(crossgen)

include(${CLR_DIR}/crossgen.cmake)

set(CMAKE_INCLUDE_CURRENT_DIR ON)
include_directories(../util)
include_directories(../../pal/prebuilt/corerror)

set(crossgen_SOURCES crossgen.cpp ../util/consoleargs.cpp)
if(WIN32)
    set(crossgen_RESOURCES Native.rc)
    add_definitions(-D_CRT_NON_CONFORMING_WCSTOK)
endif()

if(CLR_CMAKE_PLATFORM_UNIX)
  add_compile_options(-fPIE)
endif(CLR_CMAKE_PLATFORM_UNIX)

add_definitions(-DFX_VER_INTERNALNAME_STR=crossgen.exe)
add_definitions(-DNO_NGENPDB)

add_executable_clr(crossgen
  ${crossgen_SOURCES}
  ${crossgen_RESOURCES}
)

target_link_libraries(crossgen
    cee_crossgen
    mdcompiler_crossgen
    mdruntime_crossgen
    mdruntimerw_crossgen
    mdhotdata_crossgen
    corguids
    jit_crossgen
    gcinfo_crossgen
    corzap_crossgen
    mscorlib_crossgen
    strongname_crossgen
    utilcode_crossgen
    v3binder_crossgen
)

if(CLR_CMAKE_PLATFORM_UNIX)
    target_link_libraries(crossgen
        mscorrc_debug
        coreclrpal
        palrt
    )
else()
    target_link_libraries(crossgen
        advapi32
        ole32
        oleaut32
        uuid
        user32
        version
        shlwapi
        bcrypt
        mdwinmd_crossgen
        msvcrt
    )

endif(CLR_CMAKE_PLATFORM_UNIX)

add_subdirectory(../../zap/crossgen ../../zap/crossgen)
add_subdirectory(../../vm/crossgen ../../vm/crossgen)
add_subdirectory(../../md/compiler/crossgen ../../md/compiler/crossgen)
add_subdirectory(../../md/runtime/crossgen ../../md/runtime/crossgen)
add_subdirectory(../../md/enc/crossgen ../../md/enc/crossgen)
add_subdirectory(../../md/hotdata/crossgen ../../md/hotdata/crossgen)
add_subdirectory(../../vm/crossgen_mscorlib ../../vm/crossgen_mscorlib)
add_subdirectory(../../jit/crossgen ../../jit/crossgen)
add_subdirectory(../../gcinfo/crossgen ../../gcinfo/crossgen)
add_subdirectory(../../strongname/api/crossgen ../../strongname/api/crossgen)
add_subdirectory(../../utilcode/crossgen ../../utilcode/crossgen)
add_subdirectory(../../binder/v3binder_crossgen ../../binder/v3binder_crossgen)

if(WIN32)
  add_subdirectory(../../md/winmd/crossgen ../../md/winmd/crossgen)
endif(WIN32)

# add the install targets
install_clr(crossgen)
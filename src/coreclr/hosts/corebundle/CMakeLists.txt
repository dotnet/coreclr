project(CoreConsole)

set(CMAKE_INCLUDE_CURRENT_DIR ON)
include_directories(../bundle)

include(../bundle/CMakeLists.txt)

set(CoreBundle_SOURCES 
    corebundle.cpp
    HostEnvironment.cpp
    TPA.cpp
    ${Bundle_Windows_SOURCES}
)

set(CoreBundle_RESOURCES native.rc)

set(HEADERS
    TPA.h
    HostEnvironment.h
    StringBuffer.h
    error.h
    ${Bundle_HEADERS}
)


add_definitions(-DFX_VER_INTERNALNAME_STR=CoreBundle.exe)

_add_executable(CoreBundle
    ${CoreBundle_SOURCES}
    ${CoreBundle_RESOURCES}
)

target_link_libraries(CoreBundle
    utilcodestaticnohost
    advapi32.lib
    oleaut32.lib
    user32.lib
    ${STATIC_MT_CRT_LIB}
    ${STATIC_MT_VCRT_LIB}
)

# Can't compile on linux yet so only add for windows
install_clr(CoreBundle)

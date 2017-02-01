project(createdump)

set(CMAKE_INCLUDE_CURRENT_DIR ON)

remove_definitions(-DUNICODE)
remove_definitions(-D_UNICODE)

include_directories(BEFORE ${VM_DIR})

add_definitions(-DPAL_STDCPP_COMPAT=1)

add_compile_options(-fPIC)

set(CREATEDUMP_SOURCES 
    createdump.cpp 
    crashinfo.cpp
    threadinfo.cpp
    datatarget.cpp
    dumpwriter.cpp
)

_add_executable(createdump
    ${CREATEDUMP_SOURCES}
)

target_link_libraries(createdump
    corguids
    coreclrpal
    palrt
)

install_clr(createdump)

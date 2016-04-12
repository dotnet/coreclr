set(CMAKE_INCLUDE_CURRENT_DIR ON)
include_directories(BEFORE "../vm/${ARCH_SOURCES_DIR}")
include_directories(BEFORE "../vm")
include_directories(BEFORE "inc")

set(BINDER_DIR ${CMAKE_CURRENT_SOURCE_DIR})
include(sources.cmake)
      
set(BINDER_SOURCES
    ${BINDER_COMMON_SOURCES}
    ${BINDER_DIR}/clrprivbinderassemblyloadcontext.cpp
)

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

add_subdirectory(v3binder)

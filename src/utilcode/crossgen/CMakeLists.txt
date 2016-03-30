include(${CLR_DIR}/crossgen.cmake)
set(UTILCODE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../sources.cmake)

set(UTILCODE_CROSSGEN_SOURCES 
  ${UTILCODE_COMMON_SOURCES}
  ${UTILCODE_DIR}/hostimpl.cpp
  ${UTILCODE_DIR}/perflog.cpp
)

add_precompiled_header(stdafx.h ${UTILCODE_DIR}/stdafx.cpp UTILCODE_CROSSGEN_SOURCES)
add_library_clr(utilcode_crossgen STATIC ${UTILCODE_CROSSGEN_SOURCES})
if(CLR_CMAKE_PLATFORM_UNIX)
  target_link_libraries(utilcode_crossgen  nativeresourcestring)
endif(CLR_CMAKE_PLATFORM_UNIX)

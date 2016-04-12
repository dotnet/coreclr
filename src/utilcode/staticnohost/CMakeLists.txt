add_definitions(-DSELF_NO_HOST)

set(UTILCODE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
include(../utilcode.cmake)

set(UTILCODE_STATICNOHOST_SOURCES 
  ${UTILCODE_COMMON_SOURCES}
  ${UTILCODE_DIR}/hostimpl.cpp
)

if(WIN32)
     add_definitions(-D_CRTIMP=) # use static version of crt
     add_precompiled_header(stdafx.h ${UTILCODE_DIR}/stdafx.cpp UTILCODE_STATICNOHOST_SOURCES)
endif(WIN32)

add_library_clr(utilcodestaticnohost STATIC ${UTILCODE_STATICNOHOST_SOURCES})

if(CLR_CMAKE_PLATFORM_UNIX)
  target_link_libraries(utilcodestaticnohost  nativeresourcestring)
endif(CLR_CMAKE_PLATFORM_UNIX)

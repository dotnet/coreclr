if(WIN32)
     add_precompiled_header(stdafx.h ../stdafx.cpp UTILCODE_STATICNOHOST_SOURCES)
endif(WIN32)

add_library_clr(utilcodestaticnohost STATIC ${UTILCODE_STATICNOHOST_SOURCES})

if(CLR_CMAKE_PLATFORM_UNIX)
  target_link_libraries(utilcodestaticnohost  nativeresourcestring)
endif(CLR_CMAKE_PLATFORM_UNIX)

target_compile_definitions(utilcodestaticnohost PRIVATE SELF_NO_HOST)

if(WIN32)
    target_compile_definitions(utilcodestaticnohost PRIVATE _CRTIMP=) # use static version of crt
endif(WIN32)

add_dependencies(utilcodestaticnohost ${UTILCODE_DEPENDENCIES})

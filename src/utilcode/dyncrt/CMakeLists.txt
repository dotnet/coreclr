set(UTILCODE_SOURCES 
  ${UTILCODE_COMMON_SOURCES}
  ${UTILCODE_DIR}/perflog.cpp
)

if(WIN32)
  if(NOT DEFINED FEATURE_IMPLICIT_TLS)
    list(APPEND UTILCODE_SOURCES 
      ${UTILCODE_DIR}/tls.cpp
    )
  endif(NOT DEFINED FEATURE_IMPLICIT_TLS)
endif()

if(CLR_CMAKE_PLATFORM_UNIX)
    add_library_clr(utilcode STATIC ${UTILCODE_SOURCES})
    target_link_libraries(utilcode nativeresourcestring)
    add_dependencies(utilcode coreclrpal)
else()
    add_precompiled_header(stdafx.h ../stdafx.cpp UTILCODE_SOURCES)
    add_library_clr(utilcode STATIC ${UTILCODE_SOURCES})
endif(CLR_CMAKE_PLATFORM_UNIX)

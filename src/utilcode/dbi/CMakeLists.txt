add_definitions(-DSELF_NO_HOST)
add_definitions(-DFEATURE_METADATA_EMIT)
add_definitions(-DFEATURE_METADATA_EMIT_IN_DEBUGGER)
add_definitions(-DFEATURE_METADATA_INTERNAL_APIS)
add_definitions(-DFEATURE_METADATA_CUSTOM_DATA_SOURCE)
add_definitions(-DFEATURE_METADATA_DEBUGGEE_DATA_SOURCE)
# Enable mscordbi-only (perf) feature -->
add_definitions(-DFEATURE_METADATA_LOAD_TRUSTED_IMAGES -DFEATURE_METADATA_RELEASE_MEMORY_ON_REOPEN)

if(WIN32)
     add_definitions(-D_CRTIMP=) # use static version of crt
     add_precompiled_header(stdafx.h ../stdafx.cpp UTILCODE_DBI_SOURCES)
endif(WIN32)

add_library_clr(utilcode_dbi STATIC ${UTILCODE_DBI_SOURCES})

if(CLR_CMAKE_PLATFORM_UNIX)
  target_link_libraries(utilcode_dbi  nativeresourcestring)
endif(CLR_CMAKE_PLATFORM_UNIX)

add_dependencies(utilcode_dbi ${UTILCODE_DEPENDENCIES})

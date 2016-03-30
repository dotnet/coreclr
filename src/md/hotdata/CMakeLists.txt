if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

add_subdirectory(dac)
add_subdirectory(full)
if(WIN32)
  add_subdirectory(full-staticcrt)
endif(WIN32)

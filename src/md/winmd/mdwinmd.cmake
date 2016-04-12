if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

set(MDWINMD_SOURCES
  ${MDWINMD_DIR}/adapter.cpp
  ${MDWINMD_DIR}/winmdimport.cpp
  ${MDWINMD_DIR}/winmdinternalimportro.cpp
)
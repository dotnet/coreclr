include(${CLR_DIR}/crossgen.cmake)
include(../../md_wks.cmake)

set(MDWINMD_SOURCES
  ../adapter.cpp
  ../winmdimport.cpp
  ../winmdinternalimportro.cpp
)

convert_to_absolute_path(MDWINMD_SOURCES ${MDWINMD_SOURCES})

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

add_precompiled_header(stdafx.h ../stdafx.cpp MDWINMD_SOURCES)
add_library_clr(mdwinmd_crossgen ${MDWINMD_SOURCES})

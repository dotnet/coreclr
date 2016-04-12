add_definitions(-DNO_COR)

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

set(MDRUNTIME_SOURCES
  ${MDRUNTIME_DIR}/mdcolumndescriptors.cpp
  ${MDRUNTIME_DIR}/liteweightstgdb.cpp
  ${MDRUNTIME_DIR}/mdfileformat.cpp
  ${MDRUNTIME_DIR}/metamodel.cpp
  ${MDRUNTIME_DIR}/metamodelro.cpp
  ${MDRUNTIME_DIR}/recordpool.cpp
  ${MDRUNTIME_DIR}/mdinternaldisp.cpp
  ${MDRUNTIME_DIR}/mdinternalro.cpp
)

add_precompiled_header(stdafx.h ${MDRUNTIME_DIR}/stdafx.cpp MDRUNTIME_SOURCES)
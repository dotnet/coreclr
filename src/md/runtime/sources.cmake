set(MDRUNTIME_SOURCES
  ../mdcolumndescriptors.cpp
  ../liteweightstgdb.cpp
  ../mdfileformat.cpp
  ../metamodel.cpp
  ../metamodelro.cpp
  ../recordpool.cpp
  ../mdinternaldisp.cpp
  ../mdinternalro.cpp
)

convert_to_absolute_path(MDRUNTIME_SOURCES ${MDRUNTIME_SOURCES})

add_precompiled_header(stdafx.h ../stdafx.cpp MDRUNTIME_SOURCES)
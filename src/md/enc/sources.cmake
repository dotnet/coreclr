set(MDRUNTIMERW_SOURCES
  ../liteweightstgdbrw.cpp
  ../metamodelenc.cpp
  ../metamodelrw.cpp
  ../peparse.cpp
  ../rwutil.cpp
  ../stgio.cpp
  ../stgtiggerstorage.cpp
  ../stgtiggerstream.cpp
  ../mdinternalrw.cpp
)

convert_to_absolute_path(MDRUNTIMERW_SOURCES ${MDRUNTIMERW_SOURCES})
add_precompiled_header(stdafx.h ../stdafx.cpp MDRUNTIMERW_SOURCES)

if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

set(MDRUNTIMERW_SOURCES
  ${MDENC_DIR}/liteweightstgdbrw.cpp
  ${MDENC_DIR}/metamodelenc.cpp
  ${MDENC_DIR}/metamodelrw.cpp
  ${MDENC_DIR}/peparse.cpp
  ${MDENC_DIR}/rwutil.cpp
  ${MDENC_DIR}/stgio.cpp
  ${MDENC_DIR}/stgtiggerstorage.cpp
  ${MDENC_DIR}/stgtiggerstream.cpp
  ${MDENC_DIR}/mdinternalrw.cpp
)

add_precompiled_header(stdafx.h ${MDENC_DIR}/stdafx.cpp MDRUNTIMERW_SOURCES)

set(MDHOTDATA_SOURCES
  ../hotmetadata.cpp
  ../hottable.cpp
  ../hotheapsdirectoryiterator.cpp
  ../hotheap.cpp
  ../hotheapwriter.cpp
)

convert_to_absolute_path(MDHOTDATA_SOURCES ${MDHOTDATA_SOURCES})

add_precompiled_header(external.h ../external.cpp MDHOTDATA_SOURCES)
set( GCINFO_SOURCES
  ${GCINFO_DIR}/gcinfoencoder.cpp
  ${GCINFO_DIR}/dbggcinfoencoder.cpp
)

if(CLR_CMAKE_PLATFORM_ARCH_I386)
  list(APPEND GCINFO_SOURCES
    ${GCINFO_DIR}/../gcdump/gcdump.cpp
    ${GCINFO_DIR}/../gcdump/${ARCH_SOURCES_DIR}/gcdumpx86.cpp
  )
endif(CLR_CMAKE_PLATFORM_ARCH_I386)
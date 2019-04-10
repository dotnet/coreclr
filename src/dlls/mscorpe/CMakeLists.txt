project(mscorpe)

add_definitions(-DFEATURE_CORECLR)

set(MSCORPE_SOURCES
  iceefilegen.cpp
  ceefilegenwriter.cpp
  pewriter.cpp
  ceefilegenwritertokens.cpp
)

if(NOT WIN32)
  add_compile_options(-Wno-delete-non-virtual-dtor)
endif()

add_library_clr(mscorpe STATIC
  ${MSCORPE_SOURCES}
)

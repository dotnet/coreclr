if(CLR_CMAKE_PLATFORM_UNIX)
    add_compile_options(-fPIC)
endif(CLR_CMAKE_PLATFORM_UNIX)

set(MDCOMPILER_SOURCES
  ${MDCOMPILER_DIR}/assemblymd.cpp
  ${MDCOMPILER_DIR}/assemblymd_emit.cpp
  ${MDCOMPILER_DIR}/classfactory.cpp
  ${MDCOMPILER_DIR}/custattr_import.cpp
  ${MDCOMPILER_DIR}/custattr_emit.cpp
  ${MDCOMPILER_DIR}/disp.cpp
  ${MDCOMPILER_DIR}/emit.cpp
  ${MDCOMPILER_DIR}/filtermanager.cpp
  ${MDCOMPILER_DIR}/helper.cpp
  ${MDCOMPILER_DIR}/import.cpp
  ${MDCOMPILER_DIR}/importhelper.cpp
  ${MDCOMPILER_DIR}/mdutil.cpp
  ${MDCOMPILER_DIR}/regmeta.cpp
  ${MDCOMPILER_DIR}/regmeta_compilersupport.cpp
  ${MDCOMPILER_DIR}/regmeta_emit.cpp
  ${MDCOMPILER_DIR}/regmeta_import.cpp
  ${MDCOMPILER_DIR}/regmeta_imetadatatables.cpp
  ${MDCOMPILER_DIR}/regmeta_vm.cpp
  ${MDCOMPILER_DIR}/verifylayouts.cpp
)

add_precompiled_header(stdafx.h ${MDCOMPILER_DIR}/stdafx.cpp MDCOMPILER_SOURCES)
set(MDCOMPILER_SOURCES
  ../assemblymd.cpp
  ../assemblymd_emit.cpp
  ../classfactory.cpp
  ../custattr_import.cpp
  ../custattr_emit.cpp
  ../disp.cpp
  ../emit.cpp
  ../filtermanager.cpp
  ../helper.cpp
  ../import.cpp
  ../importhelper.cpp
  ../mdutil.cpp
  ../regmeta.cpp
  ../regmeta_compilersupport.cpp
  ../regmeta_emit.cpp
  ../regmeta_import.cpp
  ../regmeta_imetadatatables.cpp
  ../regmeta_vm.cpp
  ../verifylayouts.cpp
)

convert_to_absolute_path(MDCOMPILER_SOURCES ${MDCOMPILER_SOURCES})

add_precompiled_header(stdafx.h ../stdafx.cpp MDCOMPILER_SOURCES)
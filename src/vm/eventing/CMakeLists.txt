cmake_minimum_required(VERSION 2.8.12)

set(EVENT_MANIFEST ${CMAKE_CURRENT_SOURCE_DIR}/../ClrEtwAll.man)
set(EVENT_EXCLUSIONS ${CMAKE_CURRENT_SOURCE_DIR}/../ClrEtwAllMeta.lst)

if (WIN32)
  set(NONEXTERN_ARG "--nonextern")
  set(NOXPLATHEADER_ARG "--noxplatheader")
else()
  set(NEED_XPLAT_HEADER ON)
endif()

add_subdirectory(eventpipe)

if(WIN32)
  add_subdirectory(EtwProvider)
endif()

include(FindPythonInterp)

set (EventingHeaders
  ${GENERATED_INCLUDE_DIR}/etmdummy.h
  ${GENERATED_INCLUDE_DIR}/clretwallmain.h
  ${GENERATED_INCLUDE_DIR}/clreventpipewriteevents.h
)

if (NEED_XPLAT_HEADER)
  list(APPEND EventingHeaders
    ${GENERATED_INCLUDE_DIR}/clrxplatevents.h)
endif()

set(GENEVENTING_SCRIPT ${CMAKE_CURRENT_SOURCE_DIR}/../../scripts/genEventing.py)

add_custom_target(eventing
  COMMAND ${PYTHON_EXECUTABLE} ${GENEVENTING_SCRIPT} --man ${EVENT_MANIFEST} --inc ${GENERATED_INCLUDE_DIR} --dummy ${GENERATED_INCLUDE_DIR}/etmdummy.h ${NONEXTERN_ARG} ${NOXPLATHEADER_ARG}
  DEPENDS ${EVENT_MANIFEST} ${GENEVENTING_SCRIPT}
  WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

add_dependencies(eventing eventprovider)

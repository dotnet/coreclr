cmake_minimum_required(VERSION 2.8.12)

set(EVENT_MANIFEST ${CMAKE_CURRENT_SOURCE_DIR}/../ClrEtwAll.man)
set(EVENT_EXCLUSIONS ${CMAKE_CURRENT_SOURCE_DIR}/../ClrEtwAllMeta.lst)

if (WIN32)
  set(NONEXTERN_ARG "--nonextern")
  set(NOXPLATHEADER_ARG "--noxplatheader")
endif()

add_subdirectory(eventpipe)

if(WIN32)
  add_subdirectory(EtwProvider)
endif()

include(FindPythonInterp)
execute_process(
  COMMAND ${PYTHON_EXECUTABLE} ${CMAKE_CURRENT_SOURCE_DIR}/../../scripts/genEventing.py --man ${EVENT_MANIFEST} --inc ${GENERATED_INCLUDE_DIR} --dummy ${GENERATED_INCLUDE_DIR}/etmdummy.h ${NONEXTERN_ARG} ${NOXPLATHEADER_ARG})

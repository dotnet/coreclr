cmake_minimum_required(VERSION 2.8.12)

include(FindPythonInterp)

set(ETW_PROVIDER_SCRIPT ${CMAKE_CURRENT_SOURCE_DIR}/../../../scripts/genEtwProvider.py)

add_custom_target(eventprovider
  ${PYTHON_EXECUTABLE} ${ETW_PROVIDER_SCRIPT} --man ${EVENT_MANIFEST} --exc ${EVENT_EXCLUSIONS} --intermediate ${GENERATED_INCLUDE_DIR}
  DEPENDS ${EVENT_MANIFEST} ${EVENT_EXCLUSIONS} ${ETW_PROVIDER_SCRIPT})

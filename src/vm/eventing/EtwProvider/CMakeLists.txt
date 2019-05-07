cmake_minimum_required(VERSION 2.8.12)

include(FindPythonInterp)

execute_process(
  COMMAND ${PYTHON_EXECUTABLE} ${CMAKE_CURRENT_SOURCE_DIR}/../../../scripts/genEtwProvider.py --man ${EVENT_MANIFEST} --exc ${EVENT_EXCLUSIONS} --intermediate ${GENERATED_INCLUDE_DIR})

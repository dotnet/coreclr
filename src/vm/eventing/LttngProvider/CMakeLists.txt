cmake_minimum_required(VERSION 2.8.12)

include(FindPythonInterp)

execute_process(
  COMMAND ${PYTHON_EXECUTABLE} ${CMAKE_CURRENT_SOURCE_DIR}/../../../scripts/genLttngProvider.py --man ${EVENT_MANIFEST} --intermediate ${CMAKE_CURRENT_BINARY_DIR}
  RESULT_VARIABLE GEN_LTTNG_RESULT
  OUTPUT_VARIABLE LTTNG_PROVIDER_OUTPUT
  ERROR_VARIABLE GEN_LTTNG_ERRORS
)

if (NOT GEN_LTTNG_RESULT EQUAL 0)
  message(FATAL_ERROR "Failed to generate Lttng provider: ${GEN_LTTNG_ERRORS}")
endif()

string(REPLACE "\n" ";" LTTNG_PROVIDER_OUTPUT ${LTTNG_PROVIDER_OUTPUT}) # turn the outputted list of files into a CMake list

set (LTTNG_PROVIDER_SOURCES "")
set (TRACEPOINT_PROVIDER_SOURCES "")
foreach(LTTNG_PROVIDER_FILE ${LTTNG_PROVIDER_OUTPUT})
  file(TO_CMAKE_PATH ${LTTNG_PROVIDER_FILE} LTTNG_PROVIDER_FILE)
  set_source_files_properties(${LTTNG_PROVIDER_FILE} PROPERTIES GENERATED ON)
  if ("${LTTNG_PROVIDER_FILE}" MATCHES "/lttng/traceptprov[a-z]+.cpp$")
    list(APPEND TRACEPOINT_PROVIDER_SOURCES ${LTTNG_PROVIDER_FILE})
  else()
    list(APPEND LTTNG_PROVIDER_SOURCES ${LTTNG_PROVIDER_FILE})
  endif()
endforeach()

set(CMAKE_INCLUDE_CURRENT_DIR ON)

add_definitions(-DPAL_STDCPP_COMPAT=1)
include_directories(${COREPAL_SOURCE_DIR}/inc/rt)
include_directories(lttng)

add_library(eventprovider
    STATIC
    ${LTTNG_PROVIDER_SOURCES}
)
set_target_properties(eventprovider PROPERTIES LINKER_LANGUAGE CXX)

add_subdirectory(tracepointprovider)

# Install the static eventprovider library
_install(TARGETS eventprovider DESTINATION lib)
  
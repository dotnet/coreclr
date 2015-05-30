cmake_minimum_required(VERSION 2.8.12.2)

set(CMAKE_INCLUDE_CURRENT_DIR ON)

set(SOURCES
  localhostaddrs.c
)

include_directories(${PROJECT_SOURCE_DIR}/src)
include_directories(${PALTESTSUITE_SOURCE_DIR}/common)

add_executable(corefxtest_getaddrs_test1
  ${SOURCES}
)


add_dependencies(corefxtest_getaddrs_test1 System.Net.Native)

target_link_libraries(corefxtest_getaddrs_test1
  pthread
  m
  System.Net.Native
)

cmake_minimum_required(VERSION 2.8.12.2)

set(CMAKE_INCLUDE_CURRENT_DIR ON)

set(SOURCES
  LoadLibraryA.cpp
)

add_executable(paltest_loadlibrarya_test9
  ${SOURCES}
)

add_library(addon
  SHARED
  Addon.c
)

add_library(mainlibrary
  SHARED
  MainLibrary.c
)

add_dependencies(paltest_loadlibrarya_test9 coreclrpal)

target_link_libraries(paltest_loadlibrarya_test9
  ${COMMON_TEST_LIBRARIES}
)

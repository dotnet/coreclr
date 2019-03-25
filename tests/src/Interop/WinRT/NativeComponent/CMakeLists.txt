cmake_minimum_required (VERSION 2.8.12)
project (WinRTNativeComponent)
include_directories( ${INC_PLATFORM_DIR} )
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake") 
include_directories("../Contracts")
include_directories(${CMAKE_CURRENT_SOURCE_DIR})

set(SOURCES
  Component.Contracts.ArrayTesting.cpp
  Component.Contracts.BindingProjectionsTesting.cpp
  Component.Contracts.BindingViewModel.cpp
  Component.Contracts.BooleanTesting.cpp
  Component.Contracts.ExceptionTesting.cpp
  Component.Contracts.KeyValuePairTesting.cpp
  Component.Contracts.NullableTesting.cpp
  Component.Contracts.StringTesting.cpp
  Component.Contracts.TypeTesting.cpp
  Component.Contracts.UriTesting.cpp
)

set (MIDL_GENERATED_WINMD ${CMAKE_CURRENT_BINARY_DIR}/WinMD/Component.Contracts.winmd)
string(REPLACE "/" "\\" MIDL_GENERATED_WINMD_WINDOWS_PATH ${MIDL_GENERATED_WINMD})

set (MIDL_FILE ../Contracts/Component.Contracts.idl)
set (MIDL_COMMAND_LINE_ARGS
  /winrt
  /metadata_dir "C:\\Program Files (x86)\\Windows Kits\\10\\References\\10.0.17763.0\\windows.foundation.foundationcontract\\3.0.0.0" 
  /W1
  /enum_class 
  /ns_prefix 
  /target "NT60"  
  /nomidl 
  /reference "C:\\Program Files (x86)\\Windows Kits\\10\\References\\10.0.17763.0\\Windows.Foundation.FoundationContract\\3.0.0.0\\Windows.Foundation.FoundationContract.winmd" 
  /reference "C:\\Program Files (x86)\\Windows Kits\\10\\References\\10.0.17763.0\\Windows.Foundation.UniversalApiContract\\7.0.0.0\\Windows.Foundation.UniversalApiContract.winmd"
)

find_program(MIDLRT midlrt)

add_custom_command(
  OUTPUT ${MIDL_GENERATED_WINMD}
  COMMAND ${MIDLRT} ${MIDL_COMMAND_LINE_ARGS} ${MIDL_FILE} /winmd ${MIDL_GENERATED_WINMD_WINDOWS_PATH}
  WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
  DEPENDS ${MIDL_FILE}
)

set_source_files_properties(${MIDL_GENERATED_WINMD} PROPERTIES GENERATED TRUE)

set (CPP_WINRT_OUTPUT_FOLDER ${CMAKE_CURRENT_BINARY_DIR}/Generated)

set (CPP_WINRT_OUTPUT_FILES
  ${CPP_WINRT_OUTPUT_FOLDER}/winrt/impl/Component.Contracts.0.h
  ${CPP_WINRT_OUTPUT_FOLDER}/winrt/impl/Component.Contracts.1.h
  ${CPP_WINRT_OUTPUT_FOLDER}/winrt/impl/Component.Contracts.2.h
  ${CPP_WINRT_OUTPUT_FOLDER}/winrt/Component.Contracts.h
  ${CPP_WINRT_OUTPUT_FOLDER}/module.g.cpp
)

include_directories(${CPP_WINRT_OUTPUT_FOLDER})

find_program(CPPWINRT cppwinrt)

add_custom_command(
  OUTPUT ${CPP_WINRT_OUTPUT_FILES}
  COMMAND ${CPPWINRT} -overwrite -name Contracts -prefix -comp ${CMAKE_CURRENT_SOURCE_DIR} -in ${MIDL_GENERATED_WINMD} -ref 10.0.17763.0 -out ${CPP_WINRT_OUTPUT_FOLDER}
  DEPENDS ${MIDL_GENERATED_WINMD}
)

set_source_files_properties(${CPP_WINRT_OUTPUT_FILES} PROPERTIES GENERATED TRUE)

list(APPEND SOURCES ${CPP_WINRT_OUTPUT_FOLDER}/module.g.cpp)

# add the executable
add_library (WinRTNativeComponent SHARED ${SOURCES})
target_link_libraries(WinRTNativeComponent ${LINK_LIBRARIES_ADDITIONAL})
set_target_properties(WinRTNativeComponent PROPERTIES CXX_STANDARD 17)

if(WIN32)
  target_link_libraries(WinRTNativeComponent RuntimeObject.lib)
endif()

# add the install targets
install (TARGETS WinRTNativeComponent DESTINATION bin)

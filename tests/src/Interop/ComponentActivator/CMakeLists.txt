cmake_minimum_required (VERSION 2.6)
project (ComponentActivatorTestDll)
include ("${CLR_INTEROP_TEST_ROOT}/Interop.cmake")
set(SOURCES ComponentActivator.cpp )

include_directories("${CLR_DIR}/src/pal/prebuilt/inc")

# add the executable
add_library (ComponentActivatorTestDll SHARED ${SOURCES})
target_link_libraries(ComponentActivatorTestDll ${LINK_LIBRARIES_ADDITIONAL})

# add the install targets
install (TARGETS ComponentActivatorTestDll DESTINATION bin)

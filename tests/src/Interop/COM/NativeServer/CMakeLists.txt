cmake_minimum_required (VERSION 2.6)
project (COMNativeServer)
include_directories( ${INC_PLATFORM_DIR} )
include_directories( "../ServerContracts" )
set(SOURCES Servers.cpp stdafx.cpp Exports.def COMNativeServer.X.manifest)

# add the shared library
add_library (COMNativeServer SHARED ${SOURCES})
target_link_libraries(COMNativeServer ${LINK_LIBRARIES_ADDITIONAL}) 

# Copy manifest file to project output
configure_file (COMNativeServer.X.manifest ${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_BUILD_TYPE_INIT}/COMNativeServer.X.manifest COPYONLY)

# add the install targets
install (TARGETS COMNativeServer DESTINATION bin)

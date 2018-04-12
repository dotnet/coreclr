cmake_minimum_required (VERSION 2.6)
project (DllFile.Probe)
set(SOURCES DllFile.Probe.cpp )

# add the executable
add_library (DllFile.Probe SHARED ${SOURCES})
target_link_libraries(DllFile.Probe ${LINK_LIBRARIES_ADDITIONAL}) 

#get_cmake_property(_variableNames VARIABLES)
#foreach (_variableName ${_variableNames})
#    message(STATUS "${_variableName}=${${_variableName}}")
#endforeach()
 

#SET(LIBRARY_OUTPUT_PATH ${PROJECT_BINARY_DIR}/lib)

# add the install targets
install (TARGETS DllFile.Probe DESTINATION bin)
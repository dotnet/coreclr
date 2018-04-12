cmake_minimum_required (VERSION 2.6)
project (DllFileProbe)
set(SOURCES DllFileProbe.cpp )

# add the executable
add_library (DllFileProbe SHARED ${SOURCES})
target_link_libraries(DllFileProbe ${LINK_LIBRARIES_ADDITIONAL}) 

#get_cmake_property(_variableNames VARIABLES)
#foreach (_variableName ${_variableNames})
#    message(STATUS "${_variableName}=${${_variableName}}")
#endforeach()
 

#SET(LIBRARY_OUTPUT_PATH ${PROJECT_BINARY_DIR}/lib)

# add the install targets
install (TARGETS DllFileProbe DESTINATION bin)
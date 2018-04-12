cmake_minimum_required (VERSION 2.6)
project (ExeFile)
set(SOURCES ExeFile.cpp )

# add the executable
add_executable (ExeFile ${SOURCES})
target_link_libraries(ExeFile ${LINK_LIBRARIES_ADDITIONAL}) 

#get_cmake_property(_variableNames VARIABLES)
#foreach (_variableName ${_variableNames})
#    message(STATUS "${_variableName}=${${_variableName}}")
#endforeach()
 

#SET(LIBRARY_OUTPUT_PATH ${PROJECT_BINARY_DIR}/lib)

# add the install targets
install (TARGETS ExeFile DESTINATION bin)
if (FEATURE_HISTDBG)
	set(HISTDBG_LIBRARIES
		coreclrpal)

	set(HISTDBG_SOURCES
		histdbg.cpp
		classfactory.cpp
		log.cpp
		profiler.cpp
		historicaldebugging.cpp)
	set(DEF_SOURCES ${CMAKE_CURRENT_SOURCE_DIR}/histdbg_unixexports.src)
	set(EXPORTS_FILE ${CMAKE_CURRENT_BINARY_DIR}/histdbg.exports)
	include_directories(${VM_DIR})
	generate_exports_file(${DEF_SOURCES} ${EXPORTS_FILE})
	add_library_clr(histdbg SHARED ${HISTDBG_SOURCES})
	target_link_libraries(histdbg ${HISTDBG_LIBRARIES}) 
	install_clr(histdbg)
endif (FEATURE_HISTDBG)
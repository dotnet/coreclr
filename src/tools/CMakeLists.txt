add_subdirectory(crossgen)

if(WIN32)
    add_subdirectory(InjectResource)
    add_subdirectory(GenClrDebugResource)
endif(WIN32)
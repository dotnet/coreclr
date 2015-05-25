# Contains the dac build specific definitions. Included by the leaf dac cmake files.

add_definitions(-DDACCESS_COMPILE)
add_definitions(-DFEATURE_ENABLE_HARDWARE_EXCEPTIONS)
if(WIN32)
    remove_definitions(-DPROFILING_SUPPORTED)
    add_definitions(-DPROFILING_SUPPORTED_DATA)
    add_definitions(-MT)
else()
	# In DAC builds, a lot of the private fields of structs and classes is not
	# accessed. So we disable the warning here, real unused fields are detected
	# during the non-DAC build.
	add_compile_options(-Wno-unused-private-field)
endif(WIN32)

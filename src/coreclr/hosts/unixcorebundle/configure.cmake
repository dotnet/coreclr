check_symbol_exists(getauxval sys/auxv.h HAVE_GETAUXVAL)

check_include_files(
    GSS/GSS.h
    HAVE_GSSFW_HEADERS)

option(HeimdalGssApi "use heimdal implementation of GssApi" OFF)
if (HeimdalGssApi)
   check_include_files(
       gssapi/gssapi.h
       HAVE_HEIMDAL_HEADERS)
endif()

configure_file(
	${CMAKE_CURRENT_SOURCE_DIR}/config.h.in
	${CMAKE_CURRENT_BINARY_DIR}/config.h)

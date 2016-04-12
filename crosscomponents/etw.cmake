# The Following Logic is used to wire up Build dependencies for Generated files in Event Logging 
# ClrEtwAll.man                  - Event Schema
# ClrEtwAllMeta.lst              - MetaData list [provided to ensure Windows Desktop is not broken]
# genXplatEventing.py            - has the core logic for parsing Event Schema
# genWinEtw.py                   - Uses genXplatEventing to generate Windows Specific ETW Files
# clretwallmain.h and etmdummy.h - Provides the Event Logging Functionality to the VM
# clrxplatevents.h               - Used by clretwallmain.h on Non Windows platform
# ClrEtwAll.h                    - Used by clretwallmain.h on  Windows 
# ClrEtwAll.rc                   - Used by src/dlls/clretwrc/clretrw.rc on Windows

set (ScriptGeneratedEventFiles
      ${GENERATED_INCLUDE_DIR}/clretwallmain.h
      ${GENERATED_INCLUDE_DIR}/etmdummy.h
)
set (GeneratedEventFiles)

if(WIN32)
    set (GenEventFilesScript "${CLR_DIR}/src/scripts/genWinEtw.py")
    set (GenEventArgs --eventheader "${GENERATED_INCLUDE_DIR}/ClrEtwAll.h" --macroheader "${GENERATED_INCLUDE_DIR}/clretwallmain.h")

    list (APPEND ScriptGeneratedEventFiles
          ${GENERATED_INCLUDE_DIR}/ClrEtwAll.h
    )

    list (APPEND GeneratedEventFiles
         ${GENERATED_INCLUDE_DIR}/ClrEtwAll.rc
    )

    add_custom_command(
      COMMENT "Generating ETW resource Files"
      COMMAND ${MC} -h ${GENERATED_INCLUDE_DIR} -r ${GENERATED_INCLUDE_DIR} -b -co -um -p FireEtw "${VM_DIR}/ClrEtwAll.man"
      OUTPUT ${GENERATED_INCLUDE_DIR}/ClrEtwAll.h
      DEPENDS "${VM_DIR}/ClrEtwAll.man"
    )
else()
    set (GenEventFilesScript "${CLR_DIR}/src/scripts/genXplatEventing.py")
    set (GenEventArgs   --inc  "${GENERATED_INCLUDE_DIR}")

    list (APPEND ScriptGeneratedEventFiles
          ${GENERATED_INCLUDE_DIR}/clrxplatevents.h
    )
endif(WIN32)

add_custom_command(
  COMMENT "Generating Eventing Files"
  COMMAND ${PYTHON} -B -Wall -Werror ${GenEventFilesScript} ${GenEventArgs} --man "${VM_DIR}/ClrEtwAll.man" --exc "${VM_DIR}/ClrEtwAllMeta.lst" --dummy "${GENERATED_INCLUDE_DIR}/etmdummy.h"
  OUTPUT ${ScriptGeneratedEventFiles}
  DEPENDS ${GenEventFilesScript} "${VM_DIR}/ClrEtwAll.man" "${VM_DIR}/ClrEtwAllMeta.lst" "${CLR_DIR}/src/scripts/genXplatEventing.py"
)

list (APPEND GeneratedEventFiles
      ${ScriptGeneratedEventFiles}
)

add_custom_target(
  GeneratedEventingFiles
  DEPENDS ${GeneratedEventFiles}
)

function(add_library_clr)
    add_library(${ARGV})
    add_dependencies(${ARGV0} GeneratedEventingFiles)
endfunction()

function(add_executable_clr)
    add_executable(${ARGV})
    add_dependencies(${ARGV0} GeneratedEventingFiles)
endfunction()
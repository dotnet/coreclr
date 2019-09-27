
if (WIN32)
  add_precompiled_header(common.h ../common.cpp VM_SOURCES_WKS)
  # mscorlib.cpp does not compile with precompiled header file
  set_source_files_properties(../mscorlib.cpp PROPERTIES COMPILE_FLAGS "/Y-") 

  # asm files require preprocessing using cl.exe on arm32 and arm64
  if(CLR_CMAKE_PLATFORM_ARCH_ARM OR CLR_CMAKE_PLATFORM_ARCH_ARM64)

      get_include_directories_asm(ASM_INCLUDE_DIRECTORIES)

      foreach(ASM_FILE ${VM_SOURCES_WKS_ARCH_ASM})

          # Inserts a custom command in CMake build to preprocess each asm source file
          get_filename_component(name ${ASM_FILE} NAME_WE)
          file(TO_CMAKE_PATH "${CMAKE_CURRENT_BINARY_DIR}/${name}.asm" ASM_PREPROCESSED_FILE)
          preprocess_def_file(${ASM_FILE} ${ASM_PREPROCESSED_FILE})

          # We do not pass any defines since we have already done pre-processing above
          set (ASM_CMDLINE "-o ${CMAKE_CURRENT_BINARY_DIR}/${name}.obj ${ASM_PREPROCESSED_FILE}")

          # Generate the batch file that will invoke the assembler
          file(TO_CMAKE_PATH "${CMAKE_CURRENT_BINARY_DIR}/runasm_${name}.cmd" ASM_SCRIPT_FILE)

          file(GENERATE OUTPUT "${ASM_SCRIPT_FILE}"
               CONTENT "\"${CMAKE_ASM_MASM_COMPILER}\" -g ${ASM_INCLUDE_DIRECTORIES} ${ASM_CMDLINE}")

          message("Generated  - ${ASM_SCRIPT_FILE}")

          # Need to compile asm file using custom command as include directories are not provided to asm compiler
          add_custom_command(OUTPUT ${CMAKE_CURRENT_BINARY_DIR}/${name}.obj
                             COMMAND ${ASM_SCRIPT_FILE}
                             DEPENDS ${ASM_PREPROCESSED_FILE}
                             COMMENT "Assembling ${ASM_PREPROCESSED_FILE} - ${ASM_SCRIPT_FILE}")

          # mark obj as source that does not require compile
          set_source_files_properties(${CMAKE_CURRENT_BINARY_DIR}/${name}.obj PROPERTIES EXTERNAL_OBJECT TRUE)

          # Add the generated OBJ in the dependency list so that it gets consumed during linkage
          set(VM_SOURCES_WKS_ARCH_ASM ${VM_SOURCES_WKS_ARCH_ASM} ${CMAKE_CURRENT_BINARY_DIR}/${name}.obj)

      endforeach()

  endif()

endif (WIN32)

add_library_clr(cee_wks ${VM_SOURCES_WKS} ${VM_SOURCES_WKS_ARCH_ASM})
add_dependencies(cee_wks eventing_headers)

if (WIN32)

  if(NOT CLR_CMAKE_PLATFORM_ARCH_ARM AND NOT CLR_CMAKE_PLATFORM_ARCH_ARM64)

    if (CLR_CMAKE_PLATFORM_ARCH_I386)
        set_source_files_properties(${VM_SOURCES_WKS_ARCH_ASM} PROPERTIES COMPILE_FLAGS "/Zm /safeseh")
    endif (CLR_CMAKE_PLATFORM_ARCH_I386)

    # Convert AsmConstants.h into AsmConstants.inc
    find_program(POWERSHELL powershell)
    if (POWERSHELL STREQUAL "POWERSHELL-NOTFOUND")
        message(FATAL_ERROR "POWERSHELL not found")
    endif()

    # Get the current list of definitions
    get_compile_definitions(DEFINITIONS)
    add_custom_command(
        OUTPUT "${CMAKE_CURRENT_BINARY_DIR}/AsmConstants.inc"
        DEPENDS ${VM_DIR}/${ARCH_SOURCES_DIR}/asmconstants.h
        COMMAND ${POWERSHELL} -NoProfile -ExecutionPolicy Bypass -NonInteractive \"& \"\"${VM_DIR}/h2inc.ps1\"\"\" \"\"\"${VM_DIR}/${ARCH_SOURCES_DIR}/asmconstants.h\"\"\" >"${CMAKE_CURRENT_BINARY_DIR}/AsmConstants.tmp"
        COMMAND ${CMAKE_CXX_COMPILER} ${DEFINITIONS} /EP "${CMAKE_CURRENT_BINARY_DIR}/AsmConstants.tmp" >"${CMAKE_CURRENT_BINARY_DIR}/AsmConstants.inc"
    )

    set_source_files_properties(${CMAKE_CURRENT_BINARY_DIR}/AsmConstants.inc PROPERTIES GENERATED TRUE)

    add_custom_target(
        asmconstants_inc
        DEPENDS ${CMAKE_CURRENT_BINARY_DIR}/AsmConstants.inc
    )

    add_dependencies(cee_wks asmconstants_inc)

  endif(NOT CLR_CMAKE_PLATFORM_ARCH_ARM AND NOT CLR_CMAKE_PLATFORM_ARCH_ARM64)

endif (WIN32)

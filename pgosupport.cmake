function(clr_pgo_unknown_arch)
    if (WIN32)
        message(FATAL_ERROR "Only AMD64, ARM and I386 are supported for PGO")
    else()
        message(FATAL_ERROR "PGO not currently supported on the current platform")
    endif()
endfunction(clr_pgo_unknown_arch)

function(append_prop_strings TargetName PropertyName)
    foreach(Flag IN LISTS ARGN)
        set_property(TARGET ${TargetName} APPEND_STRING PROPERTY ${PropertyName} " ${Flag}")
    endforeach(Flag)
endfunction(append_prop_strings)

# Adds Profile Guided Optimization (PGO) flags to the current target
function(add_pgo TargetName)
    if(WIN32)
        set(ProfileFileName "${TargetName}.pgd")
    elseif(UNIX)
        set(ProfileFileName "${TargetName}.profdata")
    endif(WIN32)

    file(TO_NATIVE_PATH
        "${CLR_CMAKE_PACKAGES_DIR}/${CLR_CMAKE_OPTDATA_PACKAGEWITHRID}/${CLR_CMAKE_OPTDATA_VERSION}/data/${ProfileFileName}"
        ProfilePath
    )

    # Enable PGO only for optimized configs
    set(ConfigList RELEASE RELWITHDEBINFO)
    set(IsReleaseConfig "$<OR:$<CONFIG:RELEASE>,$<CONFIG:RELWITHDEBINFO>>")

    if(CLR_CMAKE_PGO_INSTRUMENT)
        # Unfortunately LINK_FLAGS_* don't support generator expressions, so we need to use a loop
        foreach(Config IN LISTS ConfigList)
            if(WIN32)
                append_prop_strings(${TargetName} LINK_FLAGS_${Config} /LTCG /GENPROFILE)
            elseif(UNIX)
                append_prop_strings(${TargetName} LINK_FLAGS_${Config} -flto -fuse-ld=gold -fprofile-instr-generate)
            endif(WIN32)
        endforeach(Config)
        if(UNIX)
            # On Unix we need to pass PGO flags to the compiler as well as the linker
            target_compile_options(${TargetName} PRIVATE $<${IsReleaseConfig}:-flto -fprofile-instr-generate>)
        endif(UNIX)
    else(CLR_CMAKE_PGO_INSTRUMENT)
        # If we don't have profile data availble, gracefully fall back to a non-PGO opt build
        # if(EXISTS ${ProfilePath})
            # Unfortunately LINK_FLAGS_* don't support generator expressions, so we need to use a loop
            foreach(Config IN LISTS ConfigList)
                if(WIN32)
                    append_prop_strings(${TargetName} LINK_FLAGS_${Config} /LTCG /USEPROFILE:PGD=${ProfilePath})
                elseif(UNIX)
                    append_prop_strings(${TargetName} LINK_FLAGS_${Config} -flto -fuse-ld=gold)
                endif(WIN32)
            endforeach(Config)
            if(UNIX)
                ## On Unix we need to pass PGO flags to the compiler as well as the linker
                target_compile_options(${TargetName} PRIVATE $<${IsReleaseConfig}:-flto>)
            endif(UNIX)
        # endif(EXISTS ${ProfilePath})
    endif(CLR_CMAKE_PGO_INSTRUMENT)
endfunction(add_pgo)

set(CLR_CMAKE_OPTDATA_PACKAGEID "optimization.PGO.CoreCLR")
set(CLR_CMAKE_OPTDATA_PACKAGEWITHRID "optimization.${CLR_CMAKE_TARGET_OS}-${CLR_CMAKE_TARGET_ARCH}.PGO.CoreCLR")

# Parse optdata package version from project.json
file(TO_NATIVE_PATH "${CMAKE_SOURCE_DIR}/extract-from-json.py" ExtractFromJsonScript)
file(TO_NATIVE_PATH "${CMAKE_SOURCE_DIR}/src/.nuget/optdata/project.json" OptDataProjectJsonPath)
execute_process(
  COMMAND python "${ExtractFromJsonScript}" -rf "${OptDataProjectJsonPath}" dependencies "${CLR_CMAKE_OPTDATA_PACKAGEID}"
  OUTPUT_VARIABLE CLR_CMAKE_OPTDATA_VERSION
  OUTPUT_STRIP_TRAILING_WHITESPACE
)

if(WIN32)
  if(CLR_CMAKE_PGO_INSTRUMENT)
    # Instrumented PGO binaries on Windows introduce an additional runtime dependency, pgort<ver>.dll.
    # Make sure we copy it next to the installed product to make it easier to redistribute the package.

    string(SUBSTRING ${CMAKE_VS_PLATFORM_TOOLSET} 1 -1 VS_PLATFORM_VERSION_NUMBER)
    set(PGORT_FILENAME "pgort${VS_PLATFORM_VERSION_NUMBER}.dll")

    get_filename_component(PATH_CXX_ROOTDIR ${CMAKE_CXX_COMPILER} DIRECTORY)

    if(CLR_CMAKE_PLATFORM_ARCH_I386)
      set(PATH_VS_PGORT_DLL "${PATH_CXX_ROOTDIR}/${PGORT_FILENAME}")
    elseif(CLR_CMAKE_PLATFORM_ARCH_AMD64)
      set(PATH_VS_PGORT_DLL "${PATH_CXX_ROOTDIR}/../amd64/${PGORT_FILENAME}")
    elseif(CLR_CMAKE_PLATFORM_ARCH_ARM)
      set(PATH_VS_PGORT_DLL "${PATH_CXX_ROOTDIR}/../arm/${PGORT_FILENAME}")
    else()
      clr_pgo_unknown_arch()
    endif()

    if (EXISTS ${PATH_VS_PGORT_DLL})
      message(STATUS "Found PGO runtime: ${PATH_VS_PGORT_DLL}")
      install(PROGRAMS ${PATH_VS_PGORT_DLL} DESTINATION .)
    else()
      message(FATAL_ERROR "file not found: ${PATH_VS_PGORT_DLL}")
    endif()

  endif(CLR_CMAKE_PGO_INSTRUMENT)
endif(WIN32)

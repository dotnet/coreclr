add_definitions(-DCROSS_COMPILE)

set (CLR_CROSS_COMPONENTS_LIST  
  crossgen   
  mscordaccore   
  mscordbi   
  sos
  clrjit
  protojit
)

if(CLR_CMAKE_PLATFORM_LINUX AND CLR_CMAKE_TARGET_ARCH_ARM)
    list(REMOVE_ITEM CLR_CROSS_COMPONENTS_LIST
        mscordaccore
        mscordbi
        sos
    )
endif()


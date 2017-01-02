#include "platformdefines.h"

#include <stdio.h>
#include <stdlib.h>
#include <xplatform.h>

extern "C"
DLL_EXPORT int foo_10() 
{
  return 10;
}

extern "C"
DLL_EXPORT int foo_11() 
{
  return 11;
}

extern "C"
DLL_EXPORT int foo_12() 
{
  return 12;
}

__asm__(".symver foo_10,foo@");
__asm__(".symver foo_11,foo@VERS_1.1");
__asm__(".symver foo_12,foo@VERS_1.2");
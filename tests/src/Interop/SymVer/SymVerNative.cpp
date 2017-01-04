extern "C"
int foo_10() 
{
  return 10;
}

extern "C"
int foo_11() 
{
  return 11;
}

extern "C"
int foo_12() 
{
  return 12;
}

#ifdef __linux__
__asm__(".symver foo_10,foo@");
__asm__(".symver foo_11,foo@VERS_1.1");
__asm__(".symver foo_12,foo@@VERS_1.2");
#endif

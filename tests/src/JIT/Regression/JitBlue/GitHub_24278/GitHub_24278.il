.assembly extern System.Runtime { }
.assembly GitHub_24278 { }
.module GitHub_24278.exe

.class private auto ansi beforefieldinit GitHub_24278.Program
       extends [System.Runtime]System.Object
{
  .method private hidebysig static bool  True() cil managed noinlining
  {
    // Code size       2 (0x2)
    .maxstack  8
    IL_0000:  ldc.i4.1
    IL_0001:  ret
  } // end of method Program::True

  .method private hidebysig static void  Use(int32 arg0) cil managed noinlining
  {
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  call       void [System.Runtime]System.GC::Collect()
    IL_0005:  ret
  } // end of method Program::Use

  .method private hidebysig static void  MainMethod() cil managed noinlining
  {
    // Code size       31 (0x1f)
    .maxstack  1
    .locals init (object V_0)
    IL_0000:  ldc.i4.s   32
    IL_0002:  conv.u
    IL_0003:  localloc
    IL_0005:  ldind.i4
    IL_0006:  call       void GitHub_24278.Program::Use(int32)
    IL_000b:  call       bool GitHub_24278.Program::True()
    IL_0010:  brfalse.s  IL_0018

    IL_0012:  newobj     instance void [System.Runtime]System.Object::.ctor()
    IL_0017:  stloc.0
    IL_0018:  ldloc.0
    IL_0019:  call       void [System.Runtime]System.GC::KeepAlive(object)
    IL_001e:  ret
  } // end of method Program::MainMethod

  .method private hidebysig static int32
          Main(string[] args) cil managed
  {
    .entrypoint
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  call       void GitHub_24278.Program::MainMethod()
    IL_0005:  ldc.i4.s   100
    IL_0007:  ret
  } // end of method Program::Main
}

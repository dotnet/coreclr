
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.6.1055.0
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly valuetypes
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 07 01 00 00 00 00 ) 

  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module valuetypes.exe
// MVID: {9846D80F-5EE3-4996-B805-CDC59811204D}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x01020000


// =============== CLASS MEMBERS DECLARATION ===================

.class interface private abstract auto ansi IFoo
{
  .method public hidebysig newslot virtual 
          instance int32  Foo(int32 a) cil managed
  {
    // Code size       32 (0x20)
    .maxstack  2
    .locals init (class ISum V_0,
             int32 V_1)
    IL_0000:  nop
    IL_0001:  ldstr      "At ISum::Sum"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.0
    IL_000d:  castclass  ISum
    IL_0012:  stloc.0
    IL_0013:  ldloc.0
    IL_0014:  callvirt   instance int32 ISum::Sum()
    IL_0019:  ldarg.1
    IL_001a:  add
    IL_001b:  stloc.1
    IL_001c:  br.s       IL_001e

    IL_001e:  ldloc.1
    IL_001f:  ret
  } // end of method IFoo::Foo

} // end of class IFoo

.class interface private abstract auto ansi ISum
{
  .method public hidebysig newslot abstract virtual 
          instance int32  Sum() cil managed
  {
  } // end of method ISum::Sum

} // end of class ISum

.class private sequential ansi sealed beforefieldinit FooBarStruct
       extends [mscorlib]System.ValueType
       implements IFoo,
                  ISum
{
  .field public int32 a
  .field public int32 b
  .method public hidebysig newslot virtual final 
          instance int32  Sum() cil managed
  {
    // Code size       30 (0x1e)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "At FooBarStruct::Sum"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.0
    IL_000d:  ldfld      int32 FooBarStruct::a
    IL_0012:  ldarg.0
    IL_0013:  ldfld      int32 FooBarStruct::b
    IL_0018:  add
    IL_0019:  stloc.0
    IL_001a:  br.s       IL_001c

    IL_001c:  ldloc.0
    IL_001d:  ret
  } // end of method FooBarStruct::Sum
} // end of class FooBarStruct

.class private auto ansi beforefieldinit Program
       extends [mscorlib]System.Object
{
  .method public hidebysig static int32  Main() cil managed
  {
    .entrypoint
    // Code size       78 (0x4e)
    .maxstack  2
    .locals init (valuetype FooBarStruct V_0,
             class IFoo V_1,
             int32 V_2)
    IL_0000:  nop
    IL_0001:  ldloca.s   V_0
    IL_0003:  initobj    FooBarStruct
    IL_0009:  ldloca.s   V_0
    IL_000b:  ldc.i4.s   10
    IL_000d:  stfld      int32 FooBarStruct::a
    IL_0012:  ldloca.s   V_0
    IL_0014:  ldc.i4.s   20
    IL_0016:  stfld      int32 FooBarStruct::b
    IL_001b:  ldloc.0
    IL_001c:  box        FooBarStruct
    IL_0021:  stloc.1
    IL_0022:  ldstr      "Calling IFoo.Foo on FooBarStruct"
    IL_0027:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_002c:  nop
    IL_002d:  ldloc.1
    IL_002e:  ldc.i4.s   10
    IL_0030:  callvirt   instance int32 IFoo::Foo(int32)
    IL_0035:  ldc.i4.s   40
    IL_0037:  ceq
    IL_0039:  ldstr      "Calling default method IFoo.Foo on FooBarStruct fa"
    + "iled"
    IL_003e:  call       void Test::Assert(bool,
                                           string)
    IL_0043:  nop
    IL_0044:  call       int32 Test::Ret()
    IL_0049:  stloc.2
    IL_004a:  br.s       IL_004c

    IL_004c:  ldloc.2
    IL_004d:  ret
  } // end of method Program::Main

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  nop
    IL_0007:  ret
  } // end of method Program::.ctor

} // end of class Program

.class private auto ansi beforefieldinit Test
       extends [mscorlib]System.Object
{
  .field private static bool Pass
  .method public hidebysig static int32  Ret() cil managed
  {
    // Code size       19 (0x13)
    .maxstack  1
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldsfld     bool Test::Pass
    IL_0006:  brtrue.s   IL_000c

    IL_0008:  ldc.i4.s   101
    IL_000a:  br.s       IL_000e

    IL_000c:  ldc.i4.s   100
    IL_000e:  stloc.0
    IL_000f:  br.s       IL_0011

    IL_0011:  ldloc.0
    IL_0012:  ret
  } // end of method Test::Ret

  .method public hidebysig static void  Assert(bool cond,
                                               string msg) cil managed
  {
    // Code size       47 (0x2f)
    .maxstack  2
    .locals init (bool V_0)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  stloc.0
    IL_0003:  ldloc.0
    IL_0004:  brfalse.s  IL_0015

    IL_0006:  nop
    IL_0007:  ldstr      "PASS"
    IL_000c:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_0011:  nop
    IL_0012:  nop
    IL_0013:  br.s       IL_002e

    IL_0015:  nop
    IL_0016:  ldstr      "FAIL: "
    IL_001b:  ldarg.1
    IL_001c:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_0021:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_0026:  nop
    IL_0027:  ldc.i4.0
    IL_0028:  stsfld     bool Test::Pass
    IL_002d:  nop
    IL_002e:  ret
  } // end of method Test::Assert

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  nop
    IL_0007:  ret
  } // end of method Test::.ctor

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldc.i4.1
    IL_0001:  stsfld     bool Test::Pass
    IL_0006:  ret
  } // end of method Test::.cctor

} // end of class Test


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file valuetypes.res

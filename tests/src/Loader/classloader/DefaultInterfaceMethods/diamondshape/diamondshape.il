
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.6.1055.0
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly diamondshape
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 07 01 00 00 00 00 ) 

  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module diamondshape.exe
// MVID: {25B467F3-D284-4DB6-BAD4-C7EAC75CF064}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x010B0000


// =============== CLASS MEMBERS DECLARATION ===================

.class interface private abstract auto ansi IFoo
{
  .method public hidebysig newslot virtual 
          instance int32  Foo(int32 a) cil managed
  {
    // Code size       7 (0x7)
    .maxstack  1
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  stloc.0
    IL_0003:  br.s       IL_0005

    IL_0005:  ldloc.0
    IL_0006:  ret                 
  } // end of method IFoo::Foo

} // end of class IFoo

.class interface private abstract auto ansi IFoo2
       implements IFoo
{
  .method public hidebysig newslot virtual final 
          instance int32  Foo(int32 a) cil managed
  {
    .override IFoo::Foo
    // Code size       20 (0x14)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "At IFoo2.Foo"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.1
    IL_000e:  add
    IL_000f:  stloc.0
    IL_0010:  br.s       IL_0012

    IL_0012:  ldloc.0
    IL_0013:  ret
  } // end of method IFoo2::IFoo.Foo

} // end of class IFoo2

.class interface private abstract auto ansi IFooEx
       implements IFoo
{
  .method public hidebysig newslot virtual final 
          instance int32  IFoo.Foo(int32 a) cil managed
  {
    .override IFoo::Foo
    // Code size       20 (0x14)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "At IFooEx.Foo"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.2
    IL_000e:  add
    IL_000f:  stloc.0
    IL_0010:  br.s       IL_0012

    IL_0012:  ldloc.0
    IL_0013:  ret
  } // end of method IFooEx::IFoo.Foo
} // end of class IFooEx

.class private auto ansi beforefieldinit FooClass
       extends [mscorlib]System.Object
       implements IFoo2,
                  IFoo,
                  IFooEx
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  nop
    IL_0007:  ret
  } // end of method FooClass::.ctor

} // end of class FooClass

.class interface private abstract auto ansi I1
{
  .method public hidebysig newslot abstract virtual 
          instance int32  Func(int32 a) cil managed
  {
  } // end of method I1::Func

} // end of class I1

.class interface private abstract auto ansi I2
       implements I1
{
.class interface private abstract auto ansi I3
       implements I1
{
  .method private hidebysig newslot virtual final 
          instance int32  I2.Func(int32 a) cil managed
  {
    .override I1::Func
    // Code size       20 (0x14)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "At I2.Func"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.1
    IL_000e:  add
    IL_000f:  stloc.0
    IL_0010:  br.s       IL_0012

    IL_0012:  ldloc.0
    IL_0013:  ret
  } // end of method I2::I1.Func    
} // end of class I3
} // end of class I2

.class interface private abstract auto ansi I3
       implements I1
{
  .method private hidebysig newslot virtual final 
          instance int32  I3.Func(int32 a) cil managed
  {
    .override I1::Func
    // Code size       20 (0x14)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "At I3.Func"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.1
    IL_000e:  add
    IL_000f:  stloc.0
    IL_0010:  br.s       IL_0012

    IL_0012:  ldloc.0
    IL_0013:  ret
  } // end of method I3::I1.Func    
} // end of class I3

.class interface private abstract auto ansi I4
       implements I2,
                  I1,
                  I3
{
  .method private hidebysig newslot virtual final 
          instance int32  I1.Func(int32 a) cil managed
  {
    .override I1::Func
    // Code size       20 (0x14)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "At I4.Func"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.1
    IL_000e:  add
    IL_000f:  stloc.0
    IL_0010:  br.s       IL_0012

    IL_0012:  ldloc.0
    IL_0013:  ret
  } // end of method I4::I1.Func   
} // end of class I4

.class private auto ansi beforefieldinit I4Class
       extends [mscorlib]System.Object
       implements I4,
                  I2,
                  I1,
                  I3
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  nop
    IL_0007:  ret
  } // end of method I4Class::.ctor

} // end of class I4Class

.class private auto ansi beforefieldinit Program
       extends [mscorlib]System.Object
{
  .method public hidebysig static void  Negative() cil managed
  {
    // Code size       51 (0x33)
    .maxstack  2
    .locals init (class FooClass V_0,
             class IFoo V_1)
    IL_0000:  nop
    IL_0001:  newobj     instance void FooClass::.ctor()
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  stloc.1
    IL_0009:  ldstr      "Calling IFoo.Foo on Foo - expecting exception."
    IL_000e:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_0013:  nop
    .try
    {
      IL_0014:  nop
      IL_0015:  ldloc.1
      IL_0016:  ldc.i4.s   10
      IL_0018:  callvirt   instance int32 IFoo::Foo(int32)
      IL_001d:  pop
      IL_001e:  ldc.i4.0
      IL_001f:  ldstr      "Expecting exception"
      IL_0024:  call       void Test::Assert(bool,
                                             string)
      IL_0029:  nop
      IL_002a:  nop
      IL_002b:  leave.s    IL_0032

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_002d:  pop
      IL_002e:  nop
      IL_002f:  nop
      IL_0030:  leave.s    IL_0032

    }  // end handler
    IL_0032:  ret
  } // end of method Program::Negative

  .method public hidebysig static void  Positive() cil managed
  {
    // Code size       33 (0x21)
    .maxstack  2
    .locals init (class I4Class V_0,
             class I1 V_1)
    IL_0000:  nop
    IL_0001:  newobj     instance void I4Class::.ctor()
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  stloc.1
    IL_0009:  ldloc.1
    IL_000a:  ldc.i4.s   10
    IL_000c:  callvirt   instance int32 I1::Func(int32)
    IL_0011:  ldc.i4.s   12
    IL_0013:  ceq
    IL_0015:  ldstr      "Expecting I1.Func to land on I4.Func"
    IL_001a:  call       void Test::Assert(bool,
                                           string)
    IL_001f:  nop
    IL_0020:  ret
  } // end of method Program::Positive

  .method public hidebysig static int32  Main() cil managed
  {
    .entrypoint
    // Code size       23 (0x17)
    .maxstack  1
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  call       void Program::Negative()
    IL_0006:  nop
    IL_0007:  call       void Program::Positive()
    IL_000c:  nop
    IL_000d:  call       int32 Test::Ret()
    IL_0012:  stloc.0
    IL_0013:  br.s       IL_0015

    IL_0015:  ldloc.0
    IL_0016:  ret
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
// WARNING: Created Win32 resource file diamondshape.res

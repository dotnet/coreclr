
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
  .method private hidebysig newslot virtual final 
          instance int32  I1.Func(int32 a) cil managed
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
    IL_000d:  ldc.i4.2
    IL_000e:  add
    IL_000f:  stloc.0
    IL_0010:  br.s       IL_0012

    IL_0012:  ldloc.0
    IL_0013:  ret
  } // end of method I2::I1.Func    
} // end of class I2

.class interface private abstract auto ansi I3
       implements I1
{
  .method private hidebysig newslot virtual final 
          instance int32  I1.Func(int32 a) cil managed
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
    IL_000d:  ldc.i4.3
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
    IL_000d:  ldc.i4.4
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

.class interface private abstract auto ansi I5
       implements I1
{
  .method private hidebysig newslot virtual final 
          instance int32  I1.Func(int32 a) cil managed
  {
    .override I1::Func
    // Code size       20 (0x14)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "At I5.Func"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.5
    IL_000e:  add
    IL_000f:  stloc.0
    IL_0010:  br.s       IL_0012

    IL_0012:  ldloc.0
    IL_0013:  ret
  } // end of method I5::I1.Func  
} // end of class I5

.class interface private abstract auto ansi I6
       implements I1
{
  .method private hidebysig newslot virtual final 
          instance int32  I1.Func(int32 a) cil managed
  {
    .override I1::Func
    // Code size       20 (0x14)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "At I6.Func"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.6
    IL_000e:  add
    IL_000f:  stloc.0
    IL_0010:  br.s       IL_0012

    IL_0012:  ldloc.0
    IL_0013:  ret
  } // end of method I6::I1.Func  
} // end of class I6

.class interface private abstract auto ansi I7
       implements I5,
                  I1,
                  I6
{
  .method private hidebysig newslot virtual final 
          instance int32  I1.Func(int32 a) cil managed
  {
    .override I1::Func
    // Code size       20 (0x14)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "At I7.Func"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.7
    IL_000e:  add
    IL_000f:  stloc.0
    IL_0010:  br.s       IL_0012

    IL_0012:  ldloc.0
    IL_0013:  ret
  } // end of method I7::I1.Func  
} // end of class I7

.class interface private abstract auto ansi I8
       implements I6,
                  I1,
                  I7,
                  I5
{
  .method private hidebysig newslot virtual final 
          instance int32  I1.Func(int32 a) cil managed
  {
    .override I1::Func
    // Code size       20 (0x14)
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "At I8.Func"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.8
    IL_000e:  add
    IL_000f:  stloc.0
    IL_0010:  br.s       IL_0012

    IL_0012:  ldloc.0
    IL_0013:  ret
  } // end of method I8::I1.Func  
} // end of class I8

.class private auto ansi beforefieldinit I47Class
       extends [mscorlib]System.Object
       implements I4,
                  I2,
                  I1,
                  I3,
                  I7,
                  I5,
                  I6
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
  } // end of method I47Class::.ctor

} // end of class I47Class

.class private auto ansi beforefieldinit I8Class
       extends [mscorlib]System.Object
       implements I8,
                  I6,
                  I1,
                  I7,
                  I5
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
  } // end of method I8Class::.ctor

} // end of class I8Class  

.class private auto ansi beforefieldinit Program
       extends [mscorlib]System.Object
{
  .method public hidebysig static void  Negative() cil managed
  {
    // Code size       100 (0x64)
    .maxstack  2
    .locals init (class FooClass V_0,
             class IFoo V_1,
             class I47Class V_2,
             class I1 V_3)
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
      IL_001f:  ldstr      "Expecting exception on Foo"
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
    IL_0032:  newobj     instance void I47Class::.ctor()
    IL_0037:  stloc.2
    IL_0038:  ldloc.2
    IL_0039:  stloc.3
    IL_003a:  ldstr      "Calling I1.Func on I47Class - expecting exception"
    IL_003f:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_0044:  nop
    .try
    {
      IL_0045:  nop
      IL_0046:  ldloc.3
      IL_0047:  ldc.i4.s   10
      IL_0049:  callvirt   instance int32 I1::Func(int32)
      IL_004e:  pop
      IL_004f:  ldc.i4.0
      IL_0050:  ldstr      "Expecting exception on I47Class"
      IL_0055:  call       void Test::Assert(bool,
                                             string)
      IL_005a:  nop
      IL_005b:  nop
      IL_005c:  leave.s    IL_0063

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_005e:  pop
      IL_005f:  nop
      IL_0060:  nop
      IL_0061:  leave.s    IL_0063

    }  // end handler
    IL_0063:  ret
  } // end of method Program::Negative

  .method public hidebysig static void  Positive() cil managed
  {
    // Code size       86 (0x56)
    .maxstack  2
    .locals init (class I4Class V_0,
             class I1 V_1,
             class I8Class V_2)
    IL_0000:  nop
    IL_0001:  ldstr      "Calling I1.Func on I4Class - expecting I4.Func"
    IL_0006:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_000b:  nop
    IL_000c:  newobj     instance void I4Class::.ctor()
    IL_0011:  stloc.0
    IL_0012:  ldloc.0
    IL_0013:  stloc.1
    IL_0014:  ldloc.1
    IL_0015:  ldc.i4.s   10
    IL_0017:  callvirt   instance int32 I1::Func(int32)
    IL_001c:  ldc.i4.s   14
    IL_001e:  ceq
    IL_0020:  ldstr      "Expecting I1.Func to land on I4.Func"
    IL_0025:  call       void Test::Assert(bool,
                                           string)
    IL_002a:  nop
    IL_002b:  ldstr      "Calling I1.Func on I8Class - expecting I8.Func"
    IL_0030:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_0035:  nop
    IL_0036:  newobj     instance void I8Class::.ctor()
    IL_003b:  stloc.2
    IL_003c:  ldloc.2
    IL_003d:  stloc.1
    IL_003e:  ldloc.1
    IL_003f:  ldc.i4.s   10
    IL_0041:  callvirt   instance int32 I1::Func(int32)
    IL_0046:  ldc.i4.s   18
    IL_0048:  ceq
    IL_004a:  ldstr      "Expecting I1.Func to land on I8.Func"
    IL_004f:  call       void Test::Assert(bool,
                                           string)
    IL_0054:  nop
    IL_0055:  ret
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

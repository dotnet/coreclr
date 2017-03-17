
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.6.1055.0
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly sharedgenerics
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 07 01 00 00 00 00 ) 

  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module sharedgenerics.exe
// MVID: {5523BE47-F0E9-4738-9962-78F730E0804B}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x01340000


// =============== CLASS MEMBERS DECLARATION ===================

.class interface private abstract auto ansi IFoo`1<T>
{
  .method public hidebysig newslot /* abstract */ virtual 
          instance class [mscorlib]System.Type 
          Foo(!T a) cil managed
  {
     // Code size       45 (0x2d)
    .maxstack  2
    .locals init (class [mscorlib]System.Type V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "Calling IFoo.Foo:"
    IL_0006:  ldarga.s   a
    IL_0008:  constrained. !T
    IL_000e:  callvirt   instance string [mscorlib]System.Object::ToString()
    IL_0013:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_0018:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_001d:  nop
    IL_001e:  ldtoken    !T
    IL_0023:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
    IL_0028:  stloc.0
    IL_0029:  br.s       IL_002b

    IL_002b:  ldloc.0
    IL_002c:  ret
 } // end of method IFoo`1::Foo

} // end of class IFoo`1

.class interface private abstract auto ansi IBar`1<- T>
{
  .method public hidebysig newslot /* abstract */ virtual 
          instance class [mscorlib]System.Type 
          Bar(!T b) cil managed
  {
     // Code size       45 (0x2d)
    .maxstack  2
    .locals init (class [mscorlib]System.Type V_0)
    IL_0000:  nop
    IL_0001:  ldstr      "Calling IBar.Bar:"
    IL_0006:  ldarga.s   b
    IL_0008:  constrained. !T
    IL_000e:  callvirt   instance string [mscorlib]System.Object::ToString()
    IL_0013:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_0018:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_001d:  nop
    IL_001e:  ldtoken    !T
    IL_0023:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
    IL_0028:  stloc.0
    IL_0029:  br.s       IL_002b

    IL_002b:  ldloc.0
    IL_002c:  ret
 } // end of method IBar`1::Bar

} // end of class IBar`1

.class private auto ansi beforefieldinit FooBar`2<T,U>
       extends [mscorlib]System.Object
       implements class IFoo`1<!T>,
                  class IBar`1<!U>
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
  } // end of method FooBar`2::.ctor

} // end of class FooBar`2

.class private auto ansi beforefieldinit Program
       extends [mscorlib]System.Object
{
  .method public hidebysig static int32  Main() cil managed
  {
    .entrypoint
    // Code size       104 (0x68)
    .maxstack  5
    .locals init (class FooBar`2<string,object> V_0,
             class IFoo`1<string> V_1,
             class IBar`1<string[]> V_2,
             int32 V_3)
    IL_0000:  nop
    IL_0001:  newobj     instance void class FooBar`2<string,object>::.ctor()
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  stloc.1
    IL_0009:  ldloc.0
    IL_000a:  stloc.2
    IL_000b:  ldloc.1
    IL_000c:  ldstr      "ABC"
    IL_0011:  callvirt   instance class [mscorlib]System.Type class IFoo`1<string>::Foo(!0)
    IL_0016:  ldtoken    [mscorlib]System.String
    IL_001b:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
    IL_0020:  call       bool [mscorlib]System.Type::op_Equality(class [mscorlib]System.Type,
                                                                 class [mscorlib]System.Type)
    IL_0025:  ldstr      "Calling IFoo.Foo on FooBar"
    IL_002a:  call       void Test::Assert(bool,
                                           string)
    IL_002f:  nop
    IL_0030:  ldloc.2
    IL_0031:  ldc.i4.1
    IL_0032:  newarr     [mscorlib]System.String
    IL_0037:  dup
    IL_0038:  ldc.i4.0
    IL_0039:  ldstr      "ABC"
    IL_003e:  stelem.ref
    IL_003f:  callvirt   instance class [mscorlib]System.Type class IBar`1<string[]>::Bar(!0)
    IL_0044:  ldtoken    object
    IL_0049:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
    IL_004e:  call       bool [mscorlib]System.Type::op_Equality(class [mscorlib]System.Type,
                                                                 class [mscorlib]System.Type)
    IL_0053:  ldstr      "Calling IBar.Bar on FooBar"
    IL_0058:  call       void Test::Assert(bool,
                                           string)
    IL_005d:  nop
    IL_005e:  call       int32 Test::Ret()
    IL_0063:  stloc.3
    IL_0064:  br.s       IL_0066

    IL_0066:  ldloc.3
    IL_0067:  ret
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
// WARNING: Created Win32 resource file sharedgenerics.res

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

.assembly extern System.Runtime { }
.assembly extern System.Console { }
.assembly GitHub_20799 { }
.module GitHub_20799.exe

.class private sequential ansi sealed beforefieldinit GitHub_20799.SByte
       extends [System.Runtime]System.ValueType
{
  .field public int8 m_value
}

.class private sequential ansi sealed beforefieldinit GitHub_20799.Int16
       extends [System.Runtime]System.ValueType
{
  .field public int16 m_value
}

.class private auto ansi beforefieldinit GitHub_20799.Program
       extends [System.Runtime]System.Object
{
  .method private hidebysig static void  Use(bool arg1) cil managed noinlining
  {
    .maxstack  8
    IL_0000:  ret
  }

  .method private hidebysig static void  Use(int32 arg1) cil managed noinlining
  {
    .maxstack  8
    IL_0000:  ret
  }

  .method private hidebysig static int32
          Run(int8 arg1,
              int8 arg2) cil managed noinlining
  {
    .maxstack  8
    IL_0000:  ldc.i4.4
    IL_0001:  conv.u

    // Unsafe buffer would require GSSecurityCookie
    IL_0002:  localloc
    IL_0004:  ldind.i4
    IL_0005:  call       void GitHub_20799.Program::Use(int32)
    IL_000a:  ldarga.s   arg1
    IL_000c:  ldarg.0
    IL_000d:  box        [System.Runtime]System.SByte

    // arg1 becomes a "vulnerable" parameter
    IL_0012:  call       instance bool [System.Runtime]System.SByte::Equals(object)
    IL_0017:  call       void GitHub_20799.Program::Use(bool)

    // A shadow copy of arg1 is being used instead
    IL_001c:  ldarga.s   arg1
    IL_001e:  ldfld      int8 GitHub_20799.SByte::m_value
    IL_0023:  conv.u1
    IL_0024:  ldarga.s   arg2
    IL_0026:  ldfld      int8 GitHub_20799.SByte::m_value
    IL_002b:  conv.u1
    IL_002c:  beq.s      IL_0030
    IL_002e:  ldc.i4.1
    IL_002f:  ret
    IL_0030:  ldc.i4.s   100
    IL_0032:  ret
  }

  .method private hidebysig static int32
          Run(int16 arg1,
              int16 arg2) cil managed noinlining
  {
    .maxstack  8
    IL_0000:  ldc.i4.4
    IL_0001:  conv.u

    // Unsafe buffer would require GSSecurityCookie
    IL_0002:  localloc    IL_0004:  ldind.i4
    IL_0005:  call       void GitHub_20799.Program::Use(int32)
    IL_000a:  ldarga.s   arg1
    IL_000c:  ldarg.0
    IL_000d:  box        [System.Runtime]System.Int16

    // arg1 becomes a "vulnerable" parameter
    IL_0012:  call       instance bool [System.Runtime]System.Int16::Equals(object)
    IL_0017:  call       void GitHub_20799.Program::Use(bool)

    // A shadow copy of arg1 is being used instead
    IL_001c:  ldarga.s   arg1
    IL_001e:  ldfld      int16 GitHub_20799.Int16::m_value
    IL_0023:  conv.u2
    IL_0024:  ldarga.s   arg2
    IL_0026:  ldfld      int16 GitHub_20799.Int16::m_value
    IL_002b:  conv.u2
    IL_002c:  beq.s      IL_0030
    IL_002e:  ldc.i4.1
    IL_002f:  ret
    IL_0030:  ldc.i4.s   100
    IL_0032:  ret
  }

  .method private hidebysig static int32
          Main(string[] args) cil managed
  {
    .entrypoint
    .maxstack  2
    .locals init (int32 V_0)
    IL_0000:  ldc.i4.s   100
    IL_0002:  stloc.0
    IL_0003:  ldc.i4.s   -127
    IL_0005:  ldc.i4.s   -127
    IL_0007:  call       int32 GitHub_20799.Program::Run(int8, int8)
    IL_000c:  ldc.i4.s   100
    IL_000e:  beq.s      IL_001c
    IL_0010:  ldstr      "FAILED: sbyte"
    IL_0015:  call       void [System.Console]System.Console::WriteLine(string)
    IL_001a:  ldc.i4.1
    IL_001b:  stloc.0
    IL_001c:  ldc.i4     0xffffaec1
    IL_0021:  ldc.i4     0xffffaec1
    IL_0026:  call       int32 GitHub_20799.Program::Run(int16, int16)
    IL_002b:  ldc.i4.s   100
    IL_002d:  beq.s      IL_003b
    IL_002f:  ldstr      "FAILED: short"
    IL_0034:  call       void [System.Console]System.Console::WriteLine(string)
    IL_0039:  ldc.i4.1
    IL_003a:  stloc.0
    IL_003b:  ldloc.0
    IL_003c:  ldc.i4.s   100
    IL_003e:  bne.un.s   IL_004a
    IL_0040:  ldstr      "PASS"
    IL_0045:  call       void [System.Console]System.Console::WriteLine(string)
    IL_004a:  ldloc.0
    IL_004b:  ret
  }

  .method public hidebysig specialname rtspecialname
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [System.Runtime]System.Object::.ctor()
    IL_0006:  ret
  }
}

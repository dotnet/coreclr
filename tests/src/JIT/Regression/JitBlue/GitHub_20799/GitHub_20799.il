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
          Run(int8 arg1, int8 arg2, int8 arg3, int8 arg4, int8 arg5, int8 arg6, int8 arg7, int8 arg8, int8 arg9) cil managed noinlining
  {
    .maxstack  8
    IL_0000:  ldc.i4.4
    IL_0001:  conv.u

    // Unsafe buffer would require GSSecurityCookie
    IL_0002:  localloc
    IL_0004:  ldind.i4
    IL_0005:  call       void GitHub_20799.Program::Use(int32)
    IL_000a:  ldarga.s   arg9
    IL_000c:  ldarg.s    arg9

    // arg9 becomes a "vulnerable" parameter
    IL_000e:  box        [System.Runtime]System.SByte
    IL_0013:  call       instance bool [System.Runtime]System.SByte::Equals(object)
    IL_0018:  call       void GitHub_20799.Program::Use(bool)

    IL_001d:  ldarga.s   arg1
    IL_001f:  ldfld      int8 GitHub_20799.SByte::m_value
    IL_0024:  conv.u1

    // A shadow copy of arg9 is being used instead
    IL_0025:  ldarga.s   arg9
    IL_0027:  ldfld      int8 GitHub_20799.SByte::m_value
    IL_002c:  conv.u1
    IL_002d:  beq.s      IL_0031

    IL_002f:  ldc.i4.1
    IL_0030:  ret

    IL_0031:  ldc.i4.s   100
    IL_0033:  ret
  }

  .method private hidebysig static int32
          Run(int16 arg1, int16 arg2, int16 arg3, int16 arg4, int16 arg5, int16 arg6, int16 arg7, int16 arg8, int16 arg9) cil managed noinlining
  {
    .maxstack  8
    IL_0000:  ldc.i4.4
    IL_0001:  conv.u

    // Unsafe buffer would require GSSecurityCookie
    IL_0002:  localloc
    IL_0004:  ldind.i4
    IL_0005:  call       void GitHub_20799.Program::Use(int32)
    IL_000a:  ldarga.s   arg9
    IL_000c:  ldarg.0

    // arg9 becomes a "vulnerable" parameter
    IL_000d:  box        [System.Runtime]System.Int16
    IL_0012:  call       instance bool [System.Runtime]System.Int16::Equals(object)
    IL_0017:  call       void GitHub_20799.Program::Use(bool)

    IL_001c:  ldarga.s   arg1
    IL_001e:  ldfld      int16 GitHub_20799.Int16::m_value
    IL_0023:  conv.u2

    // A shadow copy of arg9 is being used instead
    IL_0024:  ldarga.s   arg9
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

    .maxstack  16
    .locals init (int32 V_0)
    IL_0000:  ldc.i4.s   100
    IL_0002:  stloc.0
    IL_0003:  ldc.i4.s   -127
    IL_0005:  ldc.i4.s   -127
    IL_0007:  ldc.i4.s   -127
    IL_0009:  ldc.i4.s   -127
    IL_000b:  ldc.i4.s   -127
    IL_000d:  ldc.i4.s   -127
    IL_000f:  ldc.i4.s   -127
    IL_0011:  ldc.i4.s   -127
    IL_0013:  ldc.i4.s   -127
    IL_0015:  call       int32 GitHub_20799.Program::Run(int8, int8, int8, int8, int8, int8, int8, int8, int8)
    IL_001a:  ldc.i4.s   100
    IL_001c:  beq.s      IL_002a

    IL_001e:  ldstr      "FAILED: sbyte"
    IL_0023:  call       void [System.Console]System.Console::WriteLine(string)
    IL_0028:  ldc.i4.1
    IL_0029:  stloc.0
    IL_002a:  ldc.i4     -20799
    IL_002f:  ldc.i4     -20799
    IL_0034:  ldc.i4     -20799
    IL_0039:  ldc.i4     -20799
    IL_003e:  ldc.i4     -20799
    IL_0043:  ldc.i4     -20799
    IL_0048:  ldc.i4     -20799
    IL_004d:  ldc.i4     -20799
    IL_0052:  ldc.i4     -20799
    IL_0057:  call       int32 GitHub_20799.Program::Run(int16, int16, int16, int16, int16, int16, int16, int16, int16)
    IL_005c:  ldc.i4.s   100
    IL_005e:  beq.s      IL_006c

    IL_0060:  ldstr      "FAILED: short"
    IL_0065:  call       void [System.Console]System.Console::WriteLine(string)
    IL_006a:  ldc.i4.1
    IL_006b:  stloc.0
    IL_006c:  ldloc.0
    IL_006d:  ldc.i4.s   100
    IL_006f:  bne.un.s   IL_007b

    IL_0071:  ldstr      "PASS"
    IL_0076:  call       void [System.Console]System.Console::WriteLine(string)
    IL_007b:  ldloc.0
    IL_007c:  ret
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

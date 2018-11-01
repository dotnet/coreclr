// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

.assembly extern System.Runtime
{
}

.assembly GitHub_20625
{
}

.class private abstract auto ansi beforefieldinit Base
       extends [System.Runtime]System.Object
{
  .method public hidebysig newslot abstract virtual
          instance void  M(int32 arg1,
                           int32 arg2,
                           int32 arg3,
                           class Base arg4) cil managed
  {
  }

  .method family hidebysig specialname rtspecialname
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [System.Runtime]System.Object::.ctor()
    IL_0006:  ret
  }
}

.class private auto ansi beforefieldinit Der1
       extends Base
{
  .method public hidebysig virtual instance void
          M(int32 arg1,
            int32 arg2,
            int32 arg3,
            class Base arg4) cil managed nooptimization
  {
    .maxstack  8
    IL_0000:  ldarg.s    arg4
    IL_0002:  ldarg.1
    IL_0003:  ldarg.2
    IL_0004:  ldarg.3
    IL_0005:  ldnull
    IL_0006:  tail. callvirt   instance void Base::M(int32,
                                                            int32,
                                                            int32,
                                                            class Base)
    IL_000b:  ret
  }

  .method public hidebysig specialname rtspecialname
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Base::.ctor()
    IL_0006:  ret
  }
}

.class private auto ansi beforefieldinit Der2
       extends Base
{
  .method public hidebysig virtual instance void
          M(int32 arg1,
            int32 arg2,
            int32 arg3,
            class Base arg4) cil managed
  {
    .maxstack  8
    IL_0000:  ret
  }

  .method public hidebysig specialname rtspecialname
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void Base::.ctor()
    IL_0006:  ret
  }
}

.class private auto ansi beforefieldinit Program
       extends [System.Runtime]System.Object
{
  .method private hidebysig static int32
          Main(string[] args) cil managed
  {
    .entrypoint

    .maxstack  5
    .locals init (class Der1 V_0,
             class Der2 V_1,
             int32 V_2)
    IL_0000:  newobj     instance void Der1::.ctor()
    IL_0005:  stloc.0
    IL_0006:  newobj     instance void Der2::.ctor()
    IL_000b:  stloc.1
    .try
    {
      IL_000c:  ldloc.0
      IL_000d:  ldc.i4.s   20
      IL_000f:  ldc.i4.s   62
      IL_0011:  ldc.i4.5
      IL_0012:  ldloc.1
      IL_0013:  callvirt   instance void Base::M(int32,
                                                              int32,
                                                              int32,
                                                              class Base)
      IL_0018:  ldc.i4.s   100
      IL_001a:  stloc.2
      IL_001b:  leave.s    IL_0022

    }
    catch [System.Runtime]System.NullReferenceException
    {
      IL_001d:  pop
      IL_001e:  ldc.i4.1
      IL_001f:  stloc.2
      IL_0020:  leave.s    IL_0022

    }
    IL_0022:  ldloc.2
    IL_0023:  ret
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

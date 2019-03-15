// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

.assembly extern mscorlib { }

.assembly reabstraction { }

.class interface public abstract auto ansi I1
{
  .method public hidebysig newslot virtual
          instance int32 Add(int32 x) cil managed
  {
    ldc.i4.1
    ldarg.1
    add
    ret
  }
}

.class interface public abstract auto ansi I2
  implements I1
{
  .method public hidebysig newslot virtual abstract final
          instance int32 Add(int32 x) cil managed
  {
    .override I1::Add
  }
}

.class public auto ansi C1
  extends [mscorlib]System.Object
  implements I2
{
  .method public hidebysig specialname rtspecialname 
          instance void .ctor() cil managed
  {
    ldarg.0
    call instance void [mscorlib]System.Object::.ctor()
    ret
  }
}

.method private hidebysig static void CallPureVirtual() cil managed
{
  newobj instance void C1::.ctor()
  ldc.i4.0
  callvirt instance int32 I1::Add(int32)
  pop
  ret
}

.method private hidebysig static int32 Main() cil managed
{
  .entrypoint

  //.try
  //{
    call void CallPureVirtual()
  //  leave Fail
  //}
  //catch [System.Runtime]System.BadImageFormatException
  //{
  //  pop
  //  leave AbstractClassOK
  //}


  ldc.i4 100
  ret
}

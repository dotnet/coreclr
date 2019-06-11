// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
// This test ensures that packing is ignored for byref-like types
//

.assembly extern System.Runtime { }

.assembly refstructpacking { }

// This will show up as a blittable type
.class private sequential ansi sealed beforefieldinit NumberBuffer0
       extends [System.Runtime]System.ValueType
{
  .pack 1
  .size 0
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.IsByRefLikeAttribute::.ctor() = ( 01 00 00 00 ) 
  .field public int8 Sign
  .field public valuetype [System.Runtime]System.Span`1<char> Digits
}

// This will show up as a ManagedSequential type
.class private sequential ansi sealed beforefieldinit NumberBuffer1
       extends [System.Runtime]System.ValueType
{
  .pack 1
  .size 0
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.IsByRefLikeAttribute::.ctor() = ( 01 00 00 00 ) 
  .field public bool Sign
  .field public valuetype [System.Runtime]System.Span`1<char> Digits
}


.method private hidebysig static int32 Main() cil managed
{
  .entrypoint
  .locals init (valuetype NumberBuffer0 V_0,
                valuetype NumberBuffer1 V_1)

  ldloca.s V_0
  ldflda valuetype [System.Runtime]System.Span`1<char> NumberBuffer0::Digits
  ldloca.s V_0
  sub
  conv.i4
  ldc.i4 100
  mul
  // Leaves PointerSize * 100 at the top of the stack

  ldloca.s V_1
  ldflda valuetype [System.Runtime]System.Span`1<char> NumberBuffer1::Digits
  ldloca.s V_1
  sub
  conv.i4
  // Leaves PointerSize at the top of the stack

  add
  // Should have PointerSize * 100 + PointerSize at the top of stack

  sizeof native int
  conv.i4
  ldc.i4 100
  mul
  sizeof native int
  conv.i4
  add
  // Should have PointerSize * 100 + PointerSize at the top of stack

  sub
  // Should have 0 at the top of stack

  ldc.i4 100
  add

  ret
}

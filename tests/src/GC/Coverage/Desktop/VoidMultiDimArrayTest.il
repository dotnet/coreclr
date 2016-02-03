// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/* TEST NAME: 	VoidMultiDimArrayTest
 * 
 * DESCRIPTION:	gains 1 block in gcscan.cpp
 * NOTE:	test is not verifiable
 */


.assembly extern System.Console
{
  .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )
  .ver 4:0:0:0
}
.assembly extern legacy library mscorlib {}

.assembly test
{
  .permission reqmin ['mscorlib']System.Security.Permissions.SecurityPermissionAttribute ( "SkipVerification" = true )
}

.class public auto ansi beforefieldinit Test
       extends [mscorlib]System.Object
{
  .method public hidebysig static int32  Main() cil managed
  {
    .entrypoint
    .maxstack  2
    .locals init (void[0...,0...] V_0,
             class [mscorlib]System.ArgumentException V_1,
             int32 V_2)
    .try
    {
      ldc.i4.s   10
      ldc.i4.s   10

      // create a multidim array of type void
      newobj     instance void void[0...,0...]::.ctor(int32, int32)
      stloc.0
      leave.s    FAIL

    }
    catch [mscorlib]System.ArgumentException
    {
      stloc.1
      ldloc.1
      call       void [System.Console]System.Console::WriteLine(object)
      ldstr      "Test Passed"
      call       void [System.Console]System.Console::WriteLine(string)
      ldc.i4.s   100
      stloc.2
      leave.s    PASS

    }

    FAIL:  nop
    ldstr      "Test Failed"
    call       void [System.Console]System.Console::WriteLine(string)
    ldc.i4.1
    stloc.2
    br.s       PASS

    PASS:  nop
    ldloc.2
    ret
  }

}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

.module mcc.windows.netmodule

.namespace MCCTest
{
  .class MyClass
  {
    .method assembly static pinvokeimpl("libnative_i1c.so" as "#1" cdecl) 
        vararg valuetype MCCTest.VType1 Sum(float32) cil managed preservesig {
    }
  } // end of class MyClass
}
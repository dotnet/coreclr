// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

.module mcc.windows.netmodule

.namespace MCCTest
{
  .class MyClass
  {
    .method assembly static pinvokeimpl("native_i1s.dll" as "#1" stdcall) 
        valuetype MCCTest.VType1 Sum(float32, float32, float32, float32, float32, float32, float32, float32, float32, float32, float32, float32) cil managed preservesig {
    }
 } // end of class MyClass
}
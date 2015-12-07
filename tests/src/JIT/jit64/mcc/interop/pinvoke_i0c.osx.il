// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

.module mcc.windows.netmodule

.namespace MCCTest
{
  .class MyClass
  {
    .method assembly static pinvokeimpl("libnative_i0c.dylib" as "#1" cdecl) 
        vararg valuetype MCCTest.VType0 Sum(unsigned int64) cil managed preservesig {
    }
  } // end of class MyClass
}
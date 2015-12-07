// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

.module mcc.windows.netmodule

.namespace MCCTest
{
  .class MyClass
  {
    .method assembly static pinvokeimpl("libnative_i7s.so" as "#1" stdcall) 
        valuetype MCCTest.VType7 Sum(unsigned int64, valuetype MCCTest.VType7, float64,	valuetype MCCTest.VType7, float32, valuetype MCCTest.VType7,
									int32, valuetype MCCTest.VType7, unsigned int16, valuetype MCCTest.VType7, unsigned int32, valuetype MCCTest.VType7, 
									float32, valuetype MCCTest.VType7, int64, valuetype MCCTest.VType7, float32, valuetype MCCTest.VType7,
									float64, valuetype MCCTest.VType7, float32, valuetype MCCTest.VType7, int16, valuetype MCCTest.VType7) cil managed preservesig {
    }
  } // end of class MyClass
}
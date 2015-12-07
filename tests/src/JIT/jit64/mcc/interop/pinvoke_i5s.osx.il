// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

.module mcc.windows.netmodule

.namespace MCCTest
{
  .class MyClass
  {
    .method assembly static pinvokeimpl("libnative_i5s.dylib" as "#1" stdcall) 
        valuetype MCCTest.VType5 Sum(unsigned int64, valuetype MCCTest.VType5, float64,	valuetype MCCTest.VType5, float32, valuetype MCCTest.VType5,
									int32, valuetype MCCTest.VType5, unsigned int16, valuetype MCCTest.VType5, unsigned int32, valuetype MCCTest.VType5, 
									float32, valuetype MCCTest.VType5, int64, valuetype MCCTest.VType5, float32, valuetype MCCTest.VType5,
									float64, valuetype MCCTest.VType5, float32, valuetype MCCTest.VType5, int16, valuetype MCCTest.VType5) cil managed preservesig {
    }
  } // end of class MyClass
}
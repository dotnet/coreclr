// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

.module mcc.windows.netmodule

.namespace MCCTest
{
  .class MyClass
  {
    .method assembly static pinvokeimpl("native_i6s.dll" as "#1" stdcall) 
        valuetype MCCTest.VType6 Sum(unsigned int64, valuetype MCCTest.VType6, float64,	valuetype MCCTest.VType6, float32, valuetype MCCTest.VType6,
									int32, valuetype MCCTest.VType6, unsigned int16, valuetype MCCTest.VType6, unsigned int32, valuetype MCCTest.VType6, 
									float32, valuetype MCCTest.VType6, int64, valuetype MCCTest.VType6, float32, valuetype MCCTest.VType6,
									float64, valuetype MCCTest.VType6, float32, valuetype MCCTest.VType6, int16, valuetype MCCTest.VType6) cil managed preservesig {
    }
  } // end of class MyClass
}
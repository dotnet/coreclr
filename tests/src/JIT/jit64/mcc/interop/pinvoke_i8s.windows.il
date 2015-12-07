// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

.module mcc.windows.netmodule

.namespace MCCTest
{
  .class MyClass
  {
    .method assembly static pinvokeimpl("native_i8s.dll" as "#1" stdcall) 
        valuetype MCCTest.VType8 Sum(unsigned int64, valuetype MCCTest.VType8, float64,	valuetype MCCTest.VType8, float32, valuetype MCCTest.VType8,
									int32, valuetype MCCTest.VType8, unsigned int16, valuetype MCCTest.VType8, unsigned int32, valuetype MCCTest.VType8, 
									float32, valuetype MCCTest.VType8, int64, valuetype MCCTest.VType8, float32, valuetype MCCTest.VType8,
									float64, valuetype MCCTest.VType8, float32, valuetype MCCTest.VType8, int16, valuetype MCCTest.VType8) cil managed preservesig {
    }
  } // end of class MyClass
}
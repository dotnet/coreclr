// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

.module mcc.windows.netmodule

.namespace MCCTest
{
  .class MyClass
  {
	.method assembly static pinvokeimpl("native_i6c.dll" as "#1" cdecl) 
        vararg valuetype MCCTest.VType6 Sum(float64, int32, int64, float32, int16, float64) cil managed preservesig {
    }       
  } // end of class MyClass
}
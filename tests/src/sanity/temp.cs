// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

class Example
{
    static int Memfail()
    {
        int retval =0;
        {
            MemoryFailPoint memFailPoint = null;
            
            try
            {
                // Check for available memory.
                memFailPoint = new MemoryFailPoint(0);
            }
            catch (ArgumentException e)
            {
                retval += 50;
            }
            catch (Exception ex)
            {
                retval -= 1;
            }
            try
            {
                memFailPoint = new MemoryFailPoint(2147483647);
            }
            catch (InsufficientMemoryException e)
            {
                retval += 50;
            }
            catch (Exception ex)
            {
                retval -= 2;
            }
            memFailPoint = new MemoryFailPoint(2);
        }
        return retval;
    }
    static int RuntimeEnvironmentTest()
    {
        if(RuntimeEnvironment.SystemConfigurationFile == null) { return -1;}
        Type clsType2   = typeof(Example);
        Assembly assem = clsType2.Assembly;
        if(RuntimeEnvironment.FromGlobalAccessCache(assem)) { return -2; }
        if(RuntimeEnvironment.GetRuntimeDirectory() == null) { return -3; }
        
       Guid guid;
          try
          { 
              RuntimeEnvironment.GetRuntimeInterfaceAsIntPtr(guid,guid);
          }
          catch (System.InvalidCastException ex) {}
          try
          {
              RuntimeEnvironment.GetRuntimeInterfaceAsObject(guid,guid);
          }
          catch (System.InvalidCastException ex) {}

        if(RuntimeEnvironment.GetSystemVersion() == null) { return -4; }
        return 100;
    }
    static int Main()
    {
        return (Memfail() + RuntimeEnvironmentTest())/2;
    }
}


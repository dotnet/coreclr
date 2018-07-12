// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

using System;
using System.Text;
using System.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using TestLibrary;

public class Reflection
{
    /// <summary>
    /// Try to reflect load ComImport Types by enumerate
    /// </summary>
    /// <returns></returns>
    static bool RelectionLoad()
    {
        try
        {
            Console.WriteLine("Scenario: RelectionLoad");
            var asm = Assembly.LoadFrom("NetServer.dll");
            foreach (Type t in asm.GetTypes())
            {
                Console.WriteLine(t.Name);
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Caught unexpected exception: " + e);
            return false;
        }
    }

    /// <summary>
    /// Try to test Type.IsCOMObject
    /// </summary>
    /// <returns></returns>
    static bool TypeIsComObject()
    {
        try
        {
            Console.WriteLine("Scenario: TypeIsComObject");
            Type classType = typeof(NETServer.ContextMenu);
            if (!classType.IsCOMObject)
            {
                Console.WriteLine("ComImport Class's IsCOMObject should return true");
                return false;
            }

            Type interfaceType = typeof(NETServer.IEnumVARIANT);
            if (interfaceType.IsCOMObject)
            {
                Console.WriteLine("ComImport interface's IsCOMObject should return false");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Caught unexpected exception: " + e);
            return false;
        }
    }

    /// <summary>
    /// Try to create COM instance
    /// </summary>
    /// <returns></returns>
    static bool AcivateCOMType()
    {
        try
        {
            Console.WriteLine("Scenario: AcivateCOMType");
            var contextMenu = (NETServer.ContextMenu)Activator.CreateInstance(typeof(NETServer.ContextMenu));

            // Linux should throw PlatformNotSupportedException
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }
            
            if (contextMenu == null)
            {
                Console.WriteLine("AcivateCOMType failed");
                return false;
            }

            return true;
        }
        catch (System.Reflection.TargetInvocationException e)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && e.InnerException is PlatformNotSupportedException)
            {
                return true;
            }
            
            Console.WriteLine("Caught unexpected PlatformNotSupportedException: " + e);
            return false;
        }
        catch(System.Runtime.InteropServices.COMException e)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return true;
            }
            
            Console.WriteLine("Caught unexpected COMException: " + e);
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine("Caught unexpected exception: " + e);
            return false;
        }
    }

    [System.Security.SecuritySafeCritical]
    static int Main()
    {
        int failures = 0;
        if (!RelectionLoad())
        {
            Console.WriteLine("RelectionLoad Failed");
            failures++;
        }

        if (!TypeIsComObject())
        {
            Console.WriteLine("TypeIsComObject Failed");
            failures++;
        }

        if (!AcivateCOMType())
        {
            Console.WriteLine("AcivateCOMType Failed");
            failures++;
        }

        return failures > 0 ? 101 : 100;
    }
}
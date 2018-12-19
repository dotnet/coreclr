// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using TestLibrary;

public class RunInALC
{
    public static int Main(string[] args)
    {
        Run();
        Run();
        return 100;
    }

    static void Run()
    {
        string currentAssemblyDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
        var context = new CustomLoadContext(currentAssemblyDirectory);
        Assembly inContextAssembly = context.LoadFromAssemblyPath(Path.Combine(currentAssemblyDirectory, "CustomMarshalerInALC.dll"));
        Type inContextType = inContextAssembly.GetType("CustomMarshalerTest");
        object instance = Activator.CreateInstance(inContextType);
        MethodInfo parseIntMethod = inContextType.GetMethod("ParseInt", BindingFlags.Instance | BindingFlags.Public);
        Assert.AreEqual(1234, (int)parseIntMethod.Invoke(instance, new object[]{"1234"}));
    }
}

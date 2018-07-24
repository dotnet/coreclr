// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Reflection.Emit;

class Utf8LiteralTest
{
    static DynamicMethod EmitDynamicMethod(string strToGet)
    {
        DynamicMethod method = new DynamicMethod(
        "MyMethod", 
        typeof(Utf8String), 
        new Type[0], 
        typeof(Utf8LiteralTest).GetTypeInfo().Module);

        ILGenerator il = method.GetILGenerator();
        il.Emit(OpCodes.Ldstr, strToGet);
        il.Emit(OpCodes.Call, typeof(RuntimeHelpers).GetMethod("GetUtf8StringLiteral"));
        il.Emit(OpCodes.Ret);

        return method;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static int ShouldFailToCompileFunc()
    {
        Utf8String literalWithInvalidUTF16 = RuntimeHelpers.GetUtf8StringLiteral("\uD800");
        return literalWithInvalidUTF16.Length;
    }

    static int Main()
    {
        Utf8String literalHello = RuntimeHelpers.GetUtf8StringLiteral("Hello");
        Utf8String literalWorld = RuntimeHelpers.GetUtf8StringLiteral("World");
        Utf8String literalLongString = RuntimeHelpers.GetUtf8StringLiteral("ThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongString");
        Utf8String literalEmbeddedNullString = RuntimeHelpers.GetUtf8StringLiteral("Hello\0World");

        if (literalHello.ToString() != "Hello")
        {
            // Check that the literal had the right data in it
            return 1;
        }

        if (literalWorld.ToString() != "World")
        {
            // Check that multiple different literals are supported
            return 2;
        }

        if (literalLongString.ToString() != "ThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongThisIsJustAReallyLongLongLongLongLongString")
        {
            return 3;
        }

        if (literalEmbeddedNullString.ToString() != "Hello\0World")
        {
            // Check that a string with an embedded null is valid
            return 4;
        }

        if (Object.ReferenceEquals(literalHello.ToString(), "Hello"))
        {
            // Check that the above check wasn't just some sort of jit optimized result
            return 5;
        }

        if (!Object.ReferenceEquals(literalHello, RuntimeHelpers.GetUtf8StringLiteral(literalHello.ToString())))
        {
            // Check to see that GetUf8StringLiteral interns its input
            return 6;
        }

        try
        {
            // Test exceptional behavior of GetUtf8StringLiteral when executed dynamically
            char c = (char)0xD800;
            string highSurrogateString = new string(c, 1);
            RuntimeHelpers.GetUtf8StringLiteral(highSurrogateString);
            return 7; // That shouldn't have worked. 
        }
        catch (System.InvalidProgramException)
        {
        }

        try
        {
            // Test exceptional behavior of GetUtf8StringLiteral when executed by the JIT
            ShouldFailToCompileFunc();
            return 8; // That shouldn't have worked. 
        }
        catch (System.InvalidProgramException)
        {
        }

        Func<Utf8String> dynamicLdstr = (Func<Utf8String>)EmitDynamicMethod("SomeStringThing").CreateDelegate(typeof(Func<Utf8String>));
        if (dynamicLdstr().ToString() != "SomeStringThing")
        {
            // Check that a creating a utf8string literal on a dynamic method works correctly
            return 9;
        }

        return 100;
   }
}

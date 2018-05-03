// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
// This test makes sure that Reflection.Emit can set the default value
// of any parameter to null using `ParameterBuilder.SetConstant`.
// Specifically, this should be possible even for non-nullable value-
// typed parameters, since a null constant is how e.g. the Roslyn C#
// compiler would typically encode a default value of `default(TStruct)`.
//

using System;
using System.Reflection;
using System.Reflection.Emit;

public class Program
{
    public static int Main()
    {
        bool setOnFailure = false;

        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Assembly"), AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");

        VerifySetConstantNullSucceedsFor(typeof(bool), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(bool?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(byte), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(byte?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(char), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(char?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(DateTime), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(DateTime?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(decimal), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(decimal?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(double), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(double?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(float), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(float?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(int), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(int?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(long), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(long?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(object), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(sbyte), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(sbyte?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(short), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(short?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(string), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(uint), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(uint?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(ulong), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(ulong?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(ushort), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(ushort?), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(UserDefinedClass), moduleBuilder, ref setOnFailure);

        VerifySetConstantNullSucceedsFor(typeof(UserDefinedStruct), moduleBuilder, ref setOnFailure);
        VerifySetConstantNullSucceedsFor(typeof(UserDefinedStruct?), moduleBuilder, ref setOnFailure);

        return setOnFailure ? -1 : 100;
    }


    static void VerifySetConstantNullSucceedsFor(Type parameterType, ModuleBuilder moduleBuilder, ref bool setOnFailure)
    {
        Console.Write($"{parameterType.FullName}: ");

        var randomTypeName = "Type_" + Guid.NewGuid().ToString("N");
        var typeBuilder = moduleBuilder.DefineType(randomTypeName, TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Interface);
        var methodBuilder = typeBuilder.DefineMethod("Method", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Abstract, typeof(void), new[] { parameterType });
        var parameterBuilder = methodBuilder.DefineParameter(1, ParameterAttributes.Optional, "arg");

        try
        {
            parameterBuilder.SetConstant(null);
            Console.WriteLine("ok ");
        }
        catch (Exception ex)
        {
            Console.WriteLine("FAILED");
            Console.WriteLine(ex);
            setOnFailure = true;
        }

        var type = typeBuilder.CreateType();

        Console.WriteLine();
    }


    public class UserDefinedClass { }

    public struct UserDefinedStruct { }
}

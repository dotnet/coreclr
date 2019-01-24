// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for Microsoft.FSharp.Core type forwarding

namespace FSharp.Core.UnitTests.FSharp_Core.Type_Forwarding

open System
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

#if NET46
[<TestFixture>]
type TypeForwardingModule() =
    [<Test>]
    member this.TypeForwarding() =
        let currentRuntimeVersion = System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion()
        let currentFSharpCoreTargetRuntime = typeof<int list>.Assembly.ImageRuntimeVersion
        let tupleAssemblyName = typeof<System.Tuple<int,int>>.Assembly.FullName

        let mscorlib4AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        let fsharpCore2AssemblyName = "FSharp.Core, Version=2.3.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"

        printfn "currentRuntimeVersion = %s; currentFSharpCoreTargetRuntime=%s tupleAssemblyName=%s" currentRuntimeVersion currentFSharpCoreTargetRuntime tupleAssemblyName
        match (currentRuntimeVersion, currentFSharpCoreTargetRuntime) with
        | ("v2.0.50727", _)           
        | ("v4.0.30319", "v2.0.50727") ->
           Assert.AreEqual(tupleAssemblyName, fsharpCore2AssemblyName)
        | ("v4.0.30319", "v4.0.30319") ->
            Assert.AreEqual(tupleAssemblyName, mscorlib4AssemblyName)
        | _ -> failwith "Unknown scenario."
        ()
#else
// TODO named #define ?
#endif
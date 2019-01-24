// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Control

open System
open NUnit.Framework

open Microsoft.FSharp.Collections
open FSharp.Core.UnitTests.LibraryTestFx

[<TestFixture>]
type LazyType() =
   
    [<Test>]
    member this.Create() =
        
        // int 
        let intLazy  = Lazy<_>.Create(fun () -> 2)
        Assert.AreEqual(intLazy.Value, 2)
        
        // string
        let stringLazy = Lazy<_>.Create(fun () -> "string")
        Assert.AreEqual(stringLazy.Value, "string")
        
        // null
        let nullLazy = Lazy<_>.Create(fun () -> ())
        Assert.AreEqual(nullLazy.Value, null)
        
    [<Test>]
    member this.CreateFromValue() =
        
        // int 
        let intLazy  = Lazy<_>.CreateFromValue( 2)
        Assert.AreEqual(intLazy.Value,2)
        
        // string
        let stringLazy = Lazy<_>.CreateFromValue( "string")
        Assert.AreEqual(stringLazy.Value,"string")
        
        //null
        let nullLazy = Lazy<_>.CreateFromValue(null)
        Assert.AreEqual(nullLazy.Value,null)
         
        
    [<Test>]
    member this.Force() =
        
        // int 
        let intLazy  = Lazy<_>.CreateFromValue( 2)
        let intForce = intLazy.Force()
        Assert.AreEqual(intForce,2)
        
        // string
        let stringLazy = Lazy<_>.CreateFromValue( "string")
        let stringForce = stringLazy.Force()
        Assert.AreEqual(stringForce,"string")
        
        //null
        let nullLazy = Lazy<_>.CreateFromValue(null)
        let nullForce = nullLazy.Force()
        Assert.AreEqual(nullForce,null)
        
    [<Test>]
    member this.Value() =
        
        // int 
        let intLazy  = Lazy<_>.CreateFromValue( 2)
        Assert.AreEqual(intLazy.Value,2)
        
        // string
        let stringLazy = Lazy<_>.CreateFromValue( "string")
        Assert.AreEqual(stringLazy.Value,"string")
        
        //null
        let nullLazy = Lazy<_>.CreateFromValue(null)
        Assert.AreEqual(nullLazy.Value,null)
        
    [<Test>]
    member this.IsDelayed() =
        
        // int 
        let intLazy  = Lazy<_>.Create( fun () -> 1)
        Assert.AreEqual(not intLazy.IsValueCreated,true)
        let resultIsDelayed = intLazy.Force()
        Assert.AreEqual(not intLazy.IsValueCreated,false)
        
        // string
        let stringLazy = Lazy<_>.Create( fun () -> "string")
        Assert.AreEqual(not stringLazy.IsValueCreated,true)
        let resultIsDelayed = stringLazy.Force()
        Assert.AreEqual(not stringLazy.IsValueCreated,false)
        
        
        //null
        let nullLazy = Lazy<_>.Create(fun () -> null)
        Assert.AreEqual(not nullLazy.IsValueCreated,true)
        let resultIsDelayed = nullLazy.Force()
        Assert.AreEqual(not nullLazy.IsValueCreated,false)
        
    [<Test>]
    member this.IsForced() =
        
        // int 
        let intLazy  = Lazy<_>.Create( fun () -> 1)
        Assert.AreEqual( intLazy.IsValueCreated,false)
        let resultIsForced = intLazy.Force()
        Assert.AreEqual( intLazy.IsValueCreated,true)
        
        // string
        let stringLazy = Lazy<_>.Create( fun () -> "string")
        Assert.AreEqual( stringLazy.IsValueCreated,false)
        let resultIsForced = stringLazy.Force()
        Assert.AreEqual( stringLazy.IsValueCreated,true)
        
        
        //null
        let nullLazy = Lazy<_>.Create(fun () -> null)
        Assert.AreEqual( nullLazy.IsValueCreated,false)
        let resultIsForced = nullLazy.Force()
        Assert.AreEqual( nullLazy.IsValueCreated,true)
        
    [<Test>]
    member this.Printing() =
        let n = lazy 12
        Assert.AreEqual( n.IsValueCreated, false )
//        printfn "%A" n
        Assert.AreEqual( n.IsValueCreated, false )
//        printfn "%s" (n.ToString())
        Assert.AreEqual( n.IsValueCreated, false )
        
        

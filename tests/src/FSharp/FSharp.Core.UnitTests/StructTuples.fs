// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for Microsoft.FSharp.Core type forwarding

namespace FSharp.Core.UnitTests.FSharpStructTuples

#if TUPLE_SAMPLE
open System
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework
open TupleSample

[<TestFixture>]
type StructTuplesCSharpInterop() =

    [<Test>]
    member this.ValueTupleDirect () =

        // Basic Tuple Two Values
        let struct (one,two) =  System.ValueTuple.Create(1,2)
        Assert.IsTrue( ((one=1) && (two=2)) )

        // Basic Tuple Three Values
        let struct (one,two,three) = System.ValueTuple.Create(1, 2, 3)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) )

        // Basic Tuple Four Values
        let struct (one,two,three,four) = System.ValueTuple.Create(1, 2, 3, 4)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) )

        // Basic Tuple Five Values
        let struct (one,two,three,four,five) = System.ValueTuple.Create(1, 2, 3, 4, 5)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five=5))

        // Basic Tuple six Values
        let struct (one,two,three,four,five,six) = System.ValueTuple.Create(1, 2, 3, 4, 5, 6)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five=5) && (six=6) )

        // Basic Tuple seven Values
        let struct (one,two,three,four,five,six,seven) = System.ValueTuple.Create(1, 2, 3, 4, 5, 6, 7)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five=5) && (six=6) && (seven=7) )

        // Basic Tuple eight Values
        let struct (one,two,three,four,five,six,seven,eight) = System.ValueTuple.Create(1, 2, 3, 4, 5, 6, 7, 8)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five=5) && (six=6) && (seven=7) && (eight=8))
        ()

    [<Test>]
    member this.CSharpInteropTupleReturns () =

        // Basic Tuple Two Values
        let struct (one,two) =  TupleReturns.GetTuple(1, 2)
        Assert.IsTrue( ((one=1) && (two=2)) )

        // Basic Tuple Three Values
        let struct (one,two,three) = TupleReturns.GetTuple(1, 2, 3)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) )

        // Basic Tuple Four Values
        let struct (one,two,three,four) = TupleReturns.GetTuple(1, 2, 3, 4)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) )

        // Basic Tuple Five Values
        let struct (one,two,three,four,five) = TupleReturns.GetTuple(1, 2, 3, 4, 5)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) )

        // Basic Tuple six Values
        let struct (one,two,three,four,five,six) = TupleReturns.GetTuple(1, 2, 3, 4, 5, 6)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) )

        // Basic Tuple seven Values
        let struct (one,two,three,four,five,six,seven) = TupleReturns.GetTuple(1, 2, 3, 4, 5, 6, 7)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) )

        // Basic Tuple eight Values
        let struct (one,two,three,four,five,six,seven,eight) = TupleReturns.GetTuple(1, 2, 3, 4, 5, 6, 7, 8)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) && (eight=8) )

        // Basic Tuple nine Values
        let struct (one,two,three,four,five,six,seven,eight,nine) = TupleReturns.GetTuple(1, 2, 3, 4, 5, 6, 7, 8, 9)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) && (eight=8) && (nine=9))

        // Basic Tuple ten Values
        let struct (one,two,three,four,five,six,seven,eight,nine,ten) = TupleReturns.GetTuple(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) && (eight=8) && (nine=9) && (ten=10) )

        // Basic Tuple fifteen Values + 7T + 7T + 1T
        let struct (one,two,three,four,five,six,seven,eight,nine,ten,eleven,twelve,thirteen,fourteen,fifteen) = TupleReturns.GetTuple(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) && (eight=8) && (nine=9) && (ten=10) && (eleven=11) && (twelve=12) && (thirteen=13) && (fourteen=14) && (fifteen=15) )

        // Basic Tuple sixteen Values + 7T + 7T + 2T
        let struct (one,two,three,four,five,six,seven,eight,nine,ten,eleven,twelve,thirteen,fourteen,fifteen,sixteen) = TupleReturns.GetTuple(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16)
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) && (eight=8) && (nine=9) && (ten=10) && (eleven=11) && (twelve=12) && (thirteen=13) && (fourteen=14) && (fifteen=15) && (sixteen=16) )
        ()

    [<Test>]
    member this.CSharpInteropTupleArguments () =

        // Basic Tuple Two Values
        let struct (one,two) = TupleArguments.GetTuple( struct (1, 2) )
        Assert.IsTrue( (one=1) && (two=2) )

        // Basic Tuple Three Values
        let struct (one,two,three) = TupleArguments.GetTuple( struct (1, 2, 3) )
        Assert.IsTrue( (one=1) && (two=2) && (three=3) )

        // Basic Tuple Four Values
        let struct (one,two,three,four) = TupleArguments.GetTuple( struct (1, 2, 3, 4) )
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) )

        // Basic Tuple Five Values
        let struct (one,two,three,four,five) = TupleArguments.GetTuple(struct (1, 2, 3, 4, 5) )
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) )

        // Basic Tuple six Values
        let struct (one,two,three,four,five,six) = TupleArguments.GetTuple( struct (1, 2, 3, 4, 5, 6) )
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) )

        // Basic Tuple seven Values
        let struct (one,two,three,four,five,six,seven) = TupleArguments.GetTuple( struct (1, 2, 3, 4, 5, 6, 7) )
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) )

        // Basic Tuple eight Values
        let struct (one,two,three,four,five,six,seven,eight) = TupleArguments.GetTuple( struct (1, 2, 3, 4, 5, 6, 7, 8) )
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) && (eight=8) )

        // Basic Tuple nine Values
        let struct (one,two,three,four,five,six,seven,eight,nine) = TupleArguments.GetTuple( struct (1, 2, 3, 4, 5, 6, 7, 8, 9) )
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) && (eight=8) && (nine=9))

        // Basic Tuple ten Values
        let struct (one,two,three,four,five,six,seven,eight,nine,ten) = TupleArguments.GetTuple( struct (1, 2, 3, 4, 5, 6, 7, 8, 9, 10) )
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) && (eight=8) && (nine=9) && (ten=10) )

        // Basic Tuple fifteen Values + 7T + 7T + 1T
        let struct (one,two,three,four,five,six,seven,eight,nine,ten,eleven,twelve,thirteen,fourteen,fifteen) = TupleArguments.GetTuple( struct (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15) )
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) && (eight=8) && (nine=9) && (ten=10) && (eleven=11) && (twelve=12) && (thirteen=13) && (fourteen=14) && (fifteen=15) )

        // Basic Tuple sixteen Values + 7T + 7T + 2T
        let struct (one,two,three,four,five,six,seven,eight,nine,ten,eleven,twelve,thirteen,fourteen,fifteen,sixteen) = TupleArguments.GetTuple( struct (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16) )
        Assert.IsTrue( (one=1) && (two=2) && (three=3) && (four=4) && (five = 5) && (six=6) && (seven=7) && (eight=8) && (nine=9) && (ten=10) && (eleven=11) && (twelve=12) && (thirteen=13) && (fourteen=14) && (fifteen=15) && (sixteen=16) )
        ()
#endif


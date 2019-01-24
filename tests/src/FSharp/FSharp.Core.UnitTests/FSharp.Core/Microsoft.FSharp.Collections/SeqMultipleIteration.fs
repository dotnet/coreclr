namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections

open NUnit.Framework

[<TestFixture>]
module SeqMultipleIteration =
    let makeNewSeq () =
        let haveCalled = false |> ref
        seq {
            if !haveCalled then failwith "Should not have iterated this sequence before"
            haveCalled := true
            yield 3
        }, haveCalled

    [<Test>]
    let ``Seq.distinct only evaluates the seq once`` () =
        let s, haveCalled = makeNewSeq ()
        let distincts = Seq.distinct s
        Assert.IsFalse !haveCalled
        CollectionAssert.AreEqual (distincts |> Seq.toList, [3])
        Assert.IsTrue !haveCalled

    [<Test>]
    let ``Seq.distinctBy only evaluates the seq once`` () =
        let s, haveCalled = makeNewSeq ()
        let distincts = Seq.distinctBy id s
        Assert.IsFalse !haveCalled
        CollectionAssert.AreEqual (distincts |> Seq.toList, [3])
        Assert.IsTrue !haveCalled

    [<Test>]
    let ``Seq.groupBy only evaluates the seq once`` () =
        let s, haveCalled = makeNewSeq ()
        let groups : seq<int * seq<int>> = Seq.groupBy id s
        Assert.IsFalse !haveCalled
        let groups : list<int * seq<int>> = Seq.toList groups
        // Seq.groupBy iterates the entire sequence as soon as it begins iteration.
        Assert.IsTrue !haveCalled

    [<Test>]
    let ``Seq.countBy only evaluates the seq once`` () =
        let s, haveCalled = makeNewSeq ()
        let counts : seq<int * int> = Seq.countBy id s
        Assert.IsFalse !haveCalled
        let counts : list<int * int> = Seq.toList counts
        Assert.IsTrue !haveCalled
        CollectionAssert.AreEqual (counts |> Seq.toList, [(3, 1)])

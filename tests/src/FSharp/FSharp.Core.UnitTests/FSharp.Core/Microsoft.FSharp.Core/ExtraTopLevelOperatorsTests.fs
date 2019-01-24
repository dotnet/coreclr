namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Core

open NUnit.Framework
open FSharp.Core.UnitTests.LibraryTestFx
open System.Collections
open System.Collections.Generic

[<TestFixture>]
type DictTests () =

    [<Test>]
    member this.IEnumerable() =
        // Legit IE
        let ie = (dict [|(1,1);(2,4);(3,9)|]) :> IEnumerable
        let enum = ie.GetEnumerator()

        let testStepping() =
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(1,1))

            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(2,4))
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(3,9))
            Assert.AreEqual(enum.MoveNext(), false)
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)

        testStepping()
        enum.Reset()
        testStepping()

        // Empty IE
        let ie = [] |> dict :> IEnumerable  // Note no type args
        let enum = ie.GetEnumerator()

        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
        Assert.AreEqual(enum.MoveNext(), false)
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)

    [<Test>]
    member this.IEnumerable_T() =
        // Legit IE
        let ie = (dict [|(1,1);(2,4);(3,9)|]) :> IEnumerable<KeyValuePair<_,_>>
        let enum = ie.GetEnumerator()

        let testStepping() =
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(1,1))

            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(2,4))
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(3,9))
            Assert.AreEqual(enum.MoveNext(), false)
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)

        testStepping()
        enum.Reset()
        testStepping()

        // Empty IE
        let ie = [] |> dict :> IEnumerable  // Note no type args
        let enum = ie.GetEnumerator()

        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
        Assert.AreEqual(enum.MoveNext(), false)
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)


    [<Test>]
    member this.IDictionary() =
        // Legit ID
        let id = (dict [|(1,1);(2,4);(3,9)|]) :> IDictionary<_,_>

        Assert.IsTrue(id.ContainsKey(1))
        Assert.IsFalse(id.ContainsKey(5))
        Assert.AreEqual(id.[1], 1)
        Assert.AreEqual(id.[3], 9)
        Assert.AreEqual(id.Keys,   [| 1; 2; 3|])
        Assert.AreEqual(id.Values, [| 1; 4; 9|])

        CheckThrowsNotSupportedException(fun () -> id.[2] <-88)

        CheckThrowsNotSupportedException(fun () -> id.Add(new KeyValuePair<int,int>(4,16)))
        let mutable value = 0
        Assert.IsTrue(id.TryGetValue(2, &value))
        Assert.AreEqual(4, value)
        Assert.IsFalse(id.TryGetValue(100, &value))
        Assert.AreEqual(4, value)
        CheckThrowsNotSupportedException(fun () -> id.Remove(1) |> ignore)

        // Empty ID
        let id = dict [] :> IDictionary<int, int>   // Note no type args
        Assert.IsFalse(id.ContainsKey(5))
        CheckThrowsKeyNotFoundException(fun () -> id.[1] |> ignore)
        Assert.AreEqual(id.Keys,   [| |] )
        Assert.AreEqual(id.Values, [| |] )

    [<Test>]
    member this.``IReadOnlyDictionary on readOnlyDict``() =
        let irod = (readOnlyDict [|(1,1);(2,4);(3,9)|]) :> IReadOnlyDictionary<_,_>

        Assert.IsTrue(irod.ContainsKey(1))
        Assert.IsFalse(irod.ContainsKey(5))
        Assert.AreEqual(irod.[1], 1)
        Assert.AreEqual(irod.[3], 9)
        Assert.AreEqual(irod.Keys,   [| 1; 2; 3|])
        Assert.AreEqual(irod.Values, [| 1; 4; 9|])

        let mutable value = 0
        Assert.IsTrue(irod.TryGetValue(2, &value))
        Assert.AreEqual(4, value)

        Assert.IsFalse(irod.TryGetValue(100, &value))

        // value should not have been modified
        Assert.AreEqual(4, value)

        // Empty IROD
        let irod = readOnlyDict [] :> IReadOnlyDictionary<int, int>   // Note no type args
        Assert.IsFalse(irod.ContainsKey(5))
        CheckThrowsKeyNotFoundException(fun () -> irod.[1] |> ignore)
        Assert.AreEqual(irod.Keys,   [| |] )
        Assert.AreEqual(irod.Values, [| |] )

    [<Test>]
    member this.ICollection() =
        // Legit IC
        let ic = (dict [|(1,1);(2,4);(3,9)|]) :> ICollection<KeyValuePair<_,_>>

        Assert.AreEqual(ic.Count, 3)
        Assert.IsTrue(ic.Contains(new KeyValuePair<int,int>(3,9)))
        let newArr = Array.create 5 (new KeyValuePair<int,int>(3,9))
        ic.CopyTo(newArr,0)
        Assert.IsTrue(ic.IsReadOnly)


        // raise ReadOnlyCollection exception
        CheckThrowsNotSupportedException(fun () -> ic.Add(new KeyValuePair<int,int>(3,9)) |> ignore)
        CheckThrowsNotSupportedException(fun () -> ic.Clear() |> ignore)
        CheckThrowsNotSupportedException(fun () -> ic.Remove(new KeyValuePair<int,int>(3,9)) |> ignore)


        // Empty IC
        let ic = dict [] :> ICollection<KeyValuePair<int, int>>
        Assert.IsFalse(ic.Contains(new KeyValuePair<int,int>(3,9)))
        let newArr = Array.create 5 (new KeyValuePair<int,int>(0,0))
        ic.CopyTo(newArr,0)

    [<Test>]
    member this.``IReadOnlyCollection on readOnlyDict``() =
        // Legit IROC
        let iroc = (readOnlyDict [|(1,1);(2,4);(3,9)|]) :> IReadOnlyCollection<KeyValuePair<_,_>>

        Assert.AreEqual(iroc.Count, 3)

        // Empty IROC
        let iroc = readOnlyDict [] :> IReadOnlyCollection<KeyValuePair<int, int>>

        Assert.AreEqual(iroc.Count, 0)

    [<Test>]
    member this.``IEnumerable on readOnlyDict``() =
        // Legit IE
        let ie = (readOnlyDict [|(1,1);(2,4);(3,9)|]) :> IEnumerable
        let enum = ie.GetEnumerator()

        let testStepping() =
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(1,1))

            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(2,4))
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(3,9))
            Assert.AreEqual(enum.MoveNext(), false)
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)

        testStepping()
        enum.Reset()
        testStepping()

        // Empty IE
        let ie = [] |> readOnlyDict :> IEnumerable  // Note no type args
        let enum = ie.GetEnumerator()

        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
        Assert.AreEqual(enum.MoveNext(), false)
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)

    [<Test>]
    member this.``IEnumerable_T on readOnlyDict``() =
        // Legit IE
        let ie = (readOnlyDict [|(1,1);(2,4);(3,9)|]) :> IEnumerable<KeyValuePair<_,_>>
        let enum = ie.GetEnumerator()

        let testStepping() =
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(1,1))

            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(2,4))
            Assert.AreEqual(enum.MoveNext(), true)
            Assert.AreEqual(enum.Current, new KeyValuePair<int,int>(3,9))
            Assert.AreEqual(enum.MoveNext(), false)
            CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)

        testStepping()
        enum.Reset()
        testStepping()

        // Empty IE
        let ie = [] |> readOnlyDict :> IEnumerable  // Note no type args
        let enum = ie.GetEnumerator()

        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)
        Assert.AreEqual(enum.MoveNext(), false)
        CheckThrowsInvalidOperationExn(fun () -> enum.Current |> ignore)

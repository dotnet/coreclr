// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Collections.Set module

namespace FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections

open System
open FSharp.Core.UnitTests.LibraryTestFx
open NUnit.Framework

(*
[Test Strategy]
Make sure each method works on:
* Empty set
* Single-element set
* Sets with 4 more more elements
*)

[<TestFixture>][<Category "Collections.Set">][<Category "FSharp.Core.Collections">]
type SetModule() =

    [<Test>]
    member this.Empty() =
        let emptySet = Set.empty
        if Set.count emptySet <> 0 then Assert.Fail()    
        
        let c : Set<int>    = Set.empty
        let d : Set<string> = Set.empty
        ()

    [<Test>]
    member this.Singleton() =
        let intSingleton = Set.singleton 5
        Assert.IsTrue(intSingleton.Count = 1)
        Assert.IsTrue(intSingleton.Contains(5))
                
        let stringSingleton = Set.singleton (null)
        Assert.IsFalse(stringSingleton.Contains(""))
        
    [<Test>]
    member this.Add() =
        let empty = Set.empty
        let x     = Set.add 'x' empty
        let xy    = Set.add 'y' x
        let xyz   = Set.add 'z' xy
        let wxyz  = Set.add 'w' xyz
        
        Assert.IsTrue(Set.count xy   = 2)
        Assert.IsTrue(Set.count xyz  = 3)
        Assert.IsTrue(Set.count wxyz = 4)
        
    [<Test>]
    member this.Contains() =
        // Empty set searching for null = false
        if Set.contains null (Set.empty) <> false then Assert.Fail()

        // Single element set (of tuple) = true
        let digits = new Set<string * int>([("one", 1)])
        if Set.contains ("one", 1) digits <> true then Assert.Fail()

        let odds = new Set<int>([1 .. 2 .. 11])
        if Set.contains 6 odds <> false then Assert.Fail()
        ()
        
    [<Test>]
    member this.Count() = 
        let empty = Set.empty
        if Set.count empty <> 0 then Assert.Fail()
        
        let one = Set.add 1 empty
        if Set.count one <> 1 then Assert.Fail()
        
        let multi = new Set<char>([| 'a' .. 'z' |])
        if Set.count multi <> 26 then Assert.Fail()
        ()
        
    [<Test>]
    member this.Diff() = 
        // Given a large set and removing 0, 1, x elements...
        let alphabet = new Set<char>([| 'a' .. 'z' |])
        let emptyChar = Set.empty : Set<char>
        
        let removeEmpty = alphabet - emptyChar
        if (alphabet = removeEmpty) <> true then Assert.Fail()
        
        let number = Set.singleton '1'
        let removeNumber = alphabet - number
        if (alphabet = removeNumber) <> true then Assert.Fail()
        
        let vowels = new Set<char>([| 'a'; 'e'; 'i'; 'o'; 'u' |])
        let noVowels = alphabet - vowels
        if noVowels.Count <> 21 then Assert.Fail()
        
        // Give a set of 0, 1, x elements remove some other set
        let odds  = new Set<int>([1 .. 2 .. 10])
        let evens = new Set<int>([2 .. 2 .. 10])
        
        let emptyNum = Set.empty : Set<int>
        let removeOddsFromEmpty = emptyNum - odds 
        if (emptyNum = removeOddsFromEmpty) <> true then Assert.Fail()
        
        let one = Set.singleton 1
        let removeOddsFrom1 = one - odds
        if (removeOddsFrom1 = emptyNum) <> true then Assert.Fail()
        
        let evensSansOdds = evens - odds
        if (evensSansOdds = evens) <> true then Assert.Fail()
        ()

    [<Test>]
    member this.Equal() =
        let emptySet1 : Set<string> = Set.empty
        let emptySet2 : Set<string> = Set.empty
        if (emptySet1 = emptySet2) <> true then Assert.Fail()
        
        let a  = new Set<int>([1; 2; 3; 4; 5])
        let b = new Set<int>([1; 3; 5])
        
        if (a = b) <> false then Assert.Fail()
        
        let a = a |> Set.remove 2 |> Set.remove 4
        if (a = b) <> true then Assert.Fail()
        ()
        
    [<Test>]
    member this.Compare() =
        // Comparing empty sets
        let emptyString1 = Set.empty : Set<string>
        let emptyString2 = Set.empty : Set<string>
        
        if compare emptyString1 emptyString1 <> 0 then Assert.Fail()
        if compare emptyString1 emptyString2 <> 0 then Assert.Fail()

        // Comparing single-element sets
        let one = Set.singleton 1
        let two = Set.singleton 2
        if compare one two <> -1 then Assert.Fail()
        if compare one one <> 0  then Assert.Fail()
        if compare two two <> 0  then Assert.Fail()
        if compare two one <> 1  then Assert.Fail()

        // Comparing multi-element sets
        let alphabet = new Set<char>(['a' .. 'z'])
        let vowels   = new Set<char>(['a'; 'e'; 'i'; 'o'; 'u'])
        
        let noVowelAlpa = alphabet - vowels
        if compare noVowelAlpa alphabet     <> 1  then Assert.Fail()
        if compare alphabet alphabet        <> 0  then Assert.Fail()
        if compare noVowelAlpa noVowelAlpa  <> 0  then Assert.Fail()
        if compare alphabet noVowelAlpa     <> -1 then Assert.Fail()
        ()

    [<Test>]
    member this.Exists() =
        
        let emptyInt = Set.empty : Set<int>
        if Set.exists (fun _ -> true) emptyInt <> false then Assert.Fail()
        
        let x = Set.singleton 'x'
        if Set.exists (fun c -> c = 'x') x  <> true  then Assert.Fail()
        if Set.exists (fun c -> c <> 'x') x <> false then Assert.Fail()
        
        let letNumPairs = new Set<string * int>([("one", 1); ("two", 2); ("three", 3)])
        if Set.exists (fun (text, num) -> text = "one" && num = 1) letNumPairs <> true then Assert.Fail()
        if Set.exists (fun (text, num) -> text = "four") letNumPairs           <> false then Assert.Fail()
        ()
        
    [<Test>]
    member this.Filter() =
        
        let emptyComplex = Set.empty : Set<int * List<string * Set<decimal>> * Set<int * string * (char * char * char)>>
        let fileredEmpty = Set.filter (fun _ -> false) emptyComplex 
        if (fileredEmpty = emptyComplex) <> true then Assert.Fail()
        
        let nullSet = Set.singleton null
        if nullSet.Count <> 1 then Assert.Fail()
        let filteredNull = Set.filter (fun x -> x <> null) nullSet
        if filteredNull.Count <> 0 then Assert.Fail()
        
        let digits = new Set<int>([1 .. 10])
        let evens  = new Set<int>([2 .. 2 .. 10])
        let filteredDigits = Set.filter(fun i -> i % 2 = 0) digits
        if (filteredDigits = evens) <> true then Assert.Fail()
        ()
        

    [<Test>]
    member this.Map() =
        let emptySet : Set<string> = Set.empty
        
        let result = Set.map (fun _ -> Assert.Fail(); "") emptySet
        if (emptySet = result) <> true then Assert.Fail()
        
        let alphabet = new Set<_>(['a' .. 'z'])
        let capped = Set.map (fun c -> Char.ToUpper(c)) alphabet
        
        if Set.exists (fun c -> c = Char.ToLower(c)) capped then Assert.Fail()
        ()

    [<Test>]
    member this.Fold() =
        
        let emptySet : Set<decimal> = Set.empty
        let result = Set.fold (fun _ _ -> Assert.Fail(); -1I) 0I emptySet
        if result <> 0I then Assert.Fail()
        
        let callOrder = ref ([] : (int * int) list)
        let input = new Set<_>([1; 2; 3; 4; 5])
        
        let result = Set.fold 
                            (fun acc i -> callOrder := (acc, i) :: !callOrder; acc + i) 
                            0 
                            input
        if result    <> 15 then Assert.Fail()
        if !callOrder <> [(10, 5); (6, 4); (3, 3); (1, 2); (0, 1)] then Assert.Fail()
        ()
        
    [<Test>]
    member this.FoldBack() =
        
        let emptySet : Set<decimal> = Set.empty
        let result = Set.foldBack (fun _ _ -> Assert.Fail(); -1I) emptySet 0I
        if result <> 0I then Assert.Fail()
        
        let callOrder = ref ([] : (int * int) list)
        let input = new Set<_>([1; 2; 3; 4; 5])
        
        let result = Set.foldBack
                            (fun i acc -> callOrder := (acc, i) :: !callOrder; acc + i) 
                            input
                            0
        if result    <> 15 then Assert.Fail()
        if !callOrder <> [(14, 1); (12, 2); (9, 3); (5, 4); (0, 5)] then Assert.Fail()
        ()

    [<Test>]
    member this.ForAll() =

        let emptySet : Set<string> = Set.empty
        let result = Set.forall (fun x -> Assert.Fail(); false) emptySet
        if result <> true then Assert.Fail()
        
        let seta = new Set<_>( [1 .. 99] |> List.map (fun i -> i.ToString()) )
        let result = seta |> Set.forall (fun str -> str.Length < 3)
        Assert.IsTrue(result)

        let setb = new Set<_>( [50 .. 150] |> List.map (fun i -> i.ToString()) )
        let result = setb |> Set.forall (fun str -> str.Length < 3)
        Assert.IsFalse(result)
        ()

    [<Test>]
    member this.Intersect() =
        
        let emptySet1 : Set<int> = Set.empty
        let emptySet2 : Set<int> = Set.empty
        let four                = Set.singleton 4
       
        let emptyInterEmpty = Set.intersect emptySet1 emptySet2
        Assert.IsTrue( (emptyInterEmpty = emptySet1) )
        
        let xInterEmpty = Set.intersect four emptySet1
        Assert.IsFalse( (four = xInterEmpty) )
        
        let emptyInterX = Set.intersect emptySet1 four
        Assert.IsFalse( (four = emptyInterX) )
        ()
    
    [<Test>]
    member this.Intersect2() =
        let a = new Set<int>([3; 4; 5; 6])
        let b = new Set<int>([5; 6; 7; 8])
        
        let intersection   = Set.intersect a b
        let expectedResult = new Set<int>([5; 6])
        Assert.IsTrue( (intersection = expectedResult) )

    
    [<Test>]
    member this.IntersectMany() =
        (* IntersectAll
            1234567
             234567
              34567
               4567
                567
                 67 *)
        let setsToIntersect = 
            [
                for i = 1 to 6 do
                    yield new Set<int>([i .. 7])
            ]
            
        let result = Set.intersectMany setsToIntersect
        Assert.IsTrue(result.Count = 2)
        
        let contains x s = s |> Set.exists (fun i -> i = x) 
        Assert.IsTrue(contains 6 result)
        Assert.IsTrue(contains 7 result)
                  
    [<Test>]
    member this.IntersectMany2() =
        let all   = new Set<_>([1 .. 10])
        let odds  = new Set<_>([1 .. 2 .. 10])
        let evens = new Set<_>([2 .. 2 .. 10])
        
        let result = Set.intersectMany [odds; evens; all]
        Assert.IsTrue(Set.count result = 0)

    [<Test>]
    member this.IntersectMany3() =
        let all   = new Set<_>([1 .. 10])
        let empty = Set.empty : Set<int>
        
        let result = Set.intersectMany [all; empty; all]
        Assert.IsTrue(Set.count result = 0)
        
        
    [<Test>]
    member this.IntersectMany4() =
        CheckThrowsArgumentException (fun () -> Set.intersectMany (Seq.empty : seq<Set<int>>) |> ignore)
        ()

    [<Test>]
    member this.Union() =
        let emptySet1 : Set<int> = Set.empty
        let emptySet2 : Set<int> = Set.empty
        let four                 = Set.singleton 4
       
        let emptyUnionEmpty = Set.union emptySet1 emptySet2
        Assert.IsTrue( (emptyUnionEmpty = emptySet1) )
        
        let xUnionEmpty = Set.union four emptySet1
        Assert.IsTrue( (four = xUnionEmpty) )
        
        let emptyUnionX = Set.union emptySet1 four
        Assert.IsTrue( (four = emptyUnionX) )
        ()
    
    [<Test>]
    member this.Union2() =
        let a = new Set<int>([1; 2; 3; 4])
        let b = new Set<int>([5; 6; 7; 8])
        
        let union = Set.union a b
        let expectedResult = new Set<int>([1 .. 8])
        Assert.IsTrue( (union = expectedResult) )

    [<Test>]
    member this.Union3() =
        let x = 
            Set.singleton 1
            |> Set.union (Set.singleton 1)
            |> Set.union (Set.singleton 1)
            |> Set.union (Set.singleton 1)
            
        Assert.IsTrue(x.Count = 1)
        
    [<Test>]
    member this.UnionMany() =
        let odds  = new Set<int>([1 .. 2 .. 10])
        let evens = new Set<int>([2 .. 2 .. 10])
        let empty = Set.empty : Set<int>
        let rest  = new Set<int>([11 .. 19])
        let zero  = Set.singleton 0
        
        let result = Set.unionMany [odds; evens; empty; rest; zero]
        Assert.IsTrue(result.Count = 20)

    [<Test>]
    member this.UnionMany2() =
        let result = Set.unionMany (Seq.empty : seq<Set<string>>)
        Assert.IsTrue(result.Count = 0)
        
    [<Test>]
    member this.IsEmpty() =
        let zero  = Set.empty : Set<decimal>
        let zero2 = new Set<int>([])
        let one   = Set.singleton "foo"
        let n     = new Set<_>( [1 .. 10] )
        
        Assert.IsTrue(Set.isEmpty zero)
        Assert.IsTrue(Set.isEmpty zero2)
        
        Assert.IsFalse(Set.isEmpty one)
        Assert.IsFalse(Set.isEmpty n)
        
    [<Test>]
    member this.Iter() =

        // Empty set
        Set.empty |> Set.iter (fun _ -> Assert.Fail())

        // Full set
        let elements = [| for i = 0 to 9 do yield false |]
        
        let set = new Set<_>(['0' .. '9'])
        Set.iter (fun c -> let i = int c - int '0'
                           elements.[i] <- true) set
        
        Assert.IsTrue (Array.forall ( (=) true ) elements)

    [<Test>]
    member this.Parition() =
        
        // Empty
        let resulta, resultb = Set.partition (fun (x : int) -> Assert.Fail(); false) Set.empty
        Assert.IsTrue(resulta.Count = 0 && resultb.Count = 0)

        // One
        let single = Set.singleton "foo"
        
        let resulta, resultb = Set.partition (fun (str : string) -> str.Length <> 3) single
        Assert.IsTrue(resulta.Count = 0 && resultb.Count = 1)
        
        let resulta, resultb = Set.partition (fun (str : string) -> str.Length = 3) single
        Assert.IsTrue(resulta.Count = 1 && resultb.Count = 0)

        // Multi
        let alphabet = Set.ofList ['a' .. 'z']
        let isVowel = function |'a' | 'e' | 'i' | 'o' | 'u' -> true
                               | _ -> false

        let resulta, resultb = Set.partition isVowel alphabet
        Assert.IsTrue(resulta.Count = 5 && resultb.Count = 21)

    [<Test>]
    member this.Remove() =
        
        let emptySet : Set<int> = Set.empty
        let result = Set.remove 42 emptySet
        Assert.IsTrue(result.Count = 0)
        
        // One
        let single = Set.singleton 100I
        let resulta = Set.remove 100I single
        let resultb = Set.remove   1I single
        
        Assert.IsTrue (resulta.Count = 0)
        Assert.IsTrue (resultb.Count = 1)
        
        // Multi
        let a = new Set<int>([1 .. 5])
        Assert.IsTrue(a.Count = 5)
        
        let b = Set.remove 3 a
        Assert.IsTrue(b.Count = 4)
        // Call again, double delete
        let c = Set.remove 3 b
        Assert.IsTrue(c.Count = 4)
        
        Assert.IsFalse(Set.exists ( (=) 3 ) c)

    [<Test>]
    member this.Of_List() =
        
        // Empty
        let emptySet = Set.ofList ([] : (string * int * Set<int>) list)
        Assert.IsTrue(Set.isEmpty emptySet)
        
        // Single
        let single = Set.ofList [1]
        Assert.IsTrue(single.Count = 1)
        Assert.IsTrue(Set.exists ( (=) 1 ) single)
        
        // Multi
        let multi = Set.ofList ["mon"; "tue"; "wed"; "thu"; "fri"]
        Assert.IsTrue(multi.Count = 5)
        let expected = new Set<_>(["mon"; "tue"; "wed"; "thu"; "fri"])
        Assert.IsTrue( (multi = expected) )

    [<Test>]
    member this.To_List() =

        // Empty
        let emptySet : Set<byte> = Set.empty
        Assert.IsTrue(Set.toList emptySet = [])
        
        // Single
        let single = Set.singleton "stuff"
        Assert.IsTrue(Set.toList single = ["stuff"])
        
        // Multi
        let multi = new Set<_>([5; 2; 3; 1; 4])
        Assert.IsTrue(Set.toList multi = [1; 2; 3; 4; 5])

    [<Test>]
    member this.Of_Array() =
        
        // Empty
        let emptySet = Set.ofArray ([| |] : (string * int * Set<int>) [])
        Assert.IsTrue(Set.isEmpty emptySet)
        
        // Single
        let single = Set.ofArray [| 1 |]
        Assert.IsTrue(single.Count = 1)
        Assert.IsTrue(Set.exists ( (=) 1 ) single)
        
        // Multi
        let multi = Set.ofArray [| "mon"; "tue"; "wed"; "thu"; "fri" |]
        Assert.IsTrue(multi.Count = 5)
        let expected = new Set<_>(["mon"; "tue"; "wed"; "thu"; "fri"])
        Assert.IsTrue( (multi = expected) )

    [<Test>]
    member this.To_Array() =

        // Empty
        let emptySet : Set<byte> = Set.empty
        Assert.IsTrue(Set.toArray emptySet = [| |])
        
        // Single
        let single = Set.singleton "stuff"
        Assert.IsTrue(Set.toArray single = [| "stuff" |])
        
        // Multi
        let multi = new Set<_>([5; 2; 3; 1; 4])
        Assert.IsTrue(Set.toArray multi = [| 1; 2; 3; 4; 5 |])


    [<Test>]
    member this.Of_Seq() =
        
        // Empty
        let emptySet = Set.ofSeq ([| |] : (string * int * Set<int>) [])
        Assert.IsTrue(Set.isEmpty emptySet)
        
        // Single
        let single = Set.ofSeq [ 1 ]
        Assert.IsTrue(single.Count = 1)
        Assert.IsTrue(Set.exists ( (=) 1 ) single)
        
        // Multi
        let multi = Set.ofSeq [| "mon"; "tue"; "wed"; "thu"; "fri" |]
        Assert.IsTrue(multi.Count = 5)
        let expected = new Set<_>(["mon"; "tue"; "wed"; "thu"; "fri"])
        Assert.IsTrue( (multi = expected) )

    [<Test>]
    member this.To_Seq() =

        // Empty
        let emptySet : Set<byte> = Set.empty
        let emptySeq = Set.toSeq emptySet
        Assert.IsTrue (Seq.length emptySeq = 0)
        
        // Single
        let single = Set.singleton "stuff"
        let singleSeq = Set.toSeq single
        Assert.IsTrue(Seq.toList singleSeq = [ "stuff" ])
        
        // Multi
        let multi = new Set<_>([5; 2; 3; 1; 4])
        let multiSeq = Set.toSeq multi
        Assert.IsTrue(Seq.toList multiSeq = [ 1; 2; 3; 4; 5 ])
        

    [<Test>]
    member this.MinElement() =
        
        // Check for an argument exception "Set contains no members"
        CheckThrowsArgumentException(fun () -> Set.minElement Set.empty |> ignore)
        
        let set1 = Set.ofList [10; 8; 100; 1; 50]
        Assert.AreEqual(Set.minElement set1, 1)
        
        let set2 = Set.ofList ["abcd"; "a"; "abc"; "ab"]
        Assert.AreEqual(Set.minElement set2, "a")
        
    [<Test>]
    member this.MaxElement() =
        
        // Check for an argument exception "Set contains no members"
        CheckThrowsArgumentException(fun () -> Set.maxElement Set.empty |> ignore)
        
        let set1 = Set.ofList [10; 8; 100; 1; 50]
        Assert.AreEqual(Set.maxElement set1, 100)
        
        let set2 = Set.ofList ["abcd"; "a"; "abc"; "ab"]
        Assert.AreEqual(Set.maxElement set2, "abcd")


    [<Test>]
    member this.IsProperSubset() =
        
        let set1 = Set.ofList [10; 8; 100]
        let set2 = Set.ofList [100]
        Assert.IsTrue(Set.isProperSubset set2 set1)
        Assert.IsTrue(Set.isProperSubset Set.empty set2)
        Assert.IsFalse(Set.isProperSubset Set.empty Set.empty)
        Assert.IsFalse(Set.isProperSubset set1 set2)

    [<Test>]
    member this.IsProperSuperset() =
        
        let set1 = Set.ofList [10; 8; 100]
        let set2 = Set.ofList [100; 8]
        Assert.IsTrue(Set.isProperSuperset set1 set2)
        Assert.IsTrue(Set.isProperSuperset set2 Set.empty)
        Assert.IsFalse(Set.isProperSuperset Set.empty Set.empty)
        Assert.IsFalse(Set.isProperSuperset set1 set1)
        Assert.IsFalse(Set.isProperSuperset set2 set1)
        
    // ----- Not associated with a module function -----

    [<Test>]
    member this.GeneralTest1() =
        
        // Returns a random permutation of integers between the two bounds.
        let randomPermutation lowerBound upperBound = 
            let items = System.Collections.Generic.List<_>([lowerBound .. upperBound])
            let rng = new Random()
            
            let randomPermutation = new System.Collections.Generic.List<int>()
            while items.Count > 0 do
                let idx = rng.Next() % items.Count
                let i = items.[idx]
                items.RemoveAt(idx)
                randomPermutation.Add(i)
            
            randomPermutation.ToArray()
        
        for i in 0..50 do
            let permutation = randomPermutation 0 i
            
            let set : Set<int> ref = ref Set.empty
            // Add permutation items to set in order
            Array.iter (fun i -> set := Set.add i !set) permutation
            // Check that the set equals the full list
            Assert.IsTrue(Set.toList !set = [0 .. i])
            // Remove items in permutation order, ensuring set is delt with correctly
            Array.iteri
                (fun idx i -> set := Set.remove i !set
                              // Verify all elements have been correctly removed
                              let removedElements = Array.sub permutation 0 (idx + 1) |> Set.ofSeq
                              let inter = Set.intersect !set removedElements
                              Assert.IsTrue(inter.Count = 0))
                permutation
        ()

    
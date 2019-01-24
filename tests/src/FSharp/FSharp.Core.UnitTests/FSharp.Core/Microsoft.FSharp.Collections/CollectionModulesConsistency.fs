// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.
[<NUnit.Framework.Category "FSharp.Core.Collections">]
[<NUnit.Framework.Category "Collections.Seq">]
[<NUnit.Framework.Category "Collections.List">]
[<NUnit.Framework.Category "Collections.Array">]
module FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections.CollectionModulesConsistency

open System
open System.Collections.Generic
open NUnit.Framework
open FsCheck
open Utils

let smallerSizeCheck testable = Check.One({ Config.QuickThrowOnFailure with EndSize = 25 }, testable)

/// helper function that creates labeled FsCheck properties for equality comparisons
let consistency name sqs ls arr =
    (sqs = arr) |@ (sprintf  "Seq.%s = '%A', Array.%s = '%A'" name sqs name arr) .&. 
    (ls  = arr) |@ (sprintf "List.%s = '%A', Array.%s = '%A'" name ls name arr)


let allPairs<'a when 'a : equality> (xs : list<'a>) (xs2 : list<'a>) =
    let s = xs |> Seq.allPairs xs2 |> Seq.toArray
    let l = xs |> List.allPairs xs2  |> List.toArray
    let a = xs |> List.toArray |> Array.allPairs (List.toArray xs2)    
    consistency "allPairs" s l a

[<Test>]
let ``allPairs is consistent`` () =
    smallerSizeCheck allPairs<int>
    smallerSizeCheck allPairs<string>
    smallerSizeCheck allPairs<NormalFloat>

let append<'a when 'a : equality> (xs : list<'a>) (xs2 : list<'a>) =
    let s = xs |> Seq.append xs2 |> Seq.toArray
    let l = xs |> List.append xs2 |> List.toArray
    let a = xs |> List.toArray |> Array.append (List.toArray xs2)
    consistency "append" s l a


[<Test>]
let ``append is consistent`` () =
    smallerSizeCheck append<int>
    smallerSizeCheck append<string>
    smallerSizeCheck append<NormalFloat>

let averageFloat (xs : NormalFloat []) =
    let xs = xs |> Array.map float
    let s = runAndCheckErrorType (fun () -> xs |> Seq.average)
    let l = runAndCheckErrorType (fun () -> xs |> List.ofArray |> List.average)
    let a = runAndCheckErrorType (fun () -> xs |> Array.average)    
    consistency "average" s l a

[<Test>]
let ``average is consistent`` () =
    smallerSizeCheck averageFloat

let averageBy (xs : float []) f =
    let xs = xs |> Array.map float
    let f x = (f x : NormalFloat) |> float
    let s = runAndCheckErrorType (fun () -> xs |> Seq.averageBy f)
    let l = runAndCheckErrorType (fun () -> xs |> List.ofArray |> List.averageBy f)
    let a = runAndCheckErrorType (fun () -> xs |> Array.averageBy f)
    consistency "averageBy" s l a


[<Test>]
let ``averageBy is consistent`` () =
    smallerSizeCheck averageBy

let contains<'a when 'a : equality> (xs : 'a []) x  =
    let s = xs |> Seq.contains x
    let l = xs |> List.ofArray |> List.contains x
    let a = xs |> Array.contains x
    consistency "contains" s l a


[<Test>]
let ``contains is consistent`` () =
    smallerSizeCheck contains<int>
    smallerSizeCheck contains<string>
    smallerSizeCheck contains<float>

let choose<'a when 'a : equality> (xs : 'a []) f  =
    let s = xs |> Seq.choose f |> Seq.toArray
    let l = xs |> List.ofArray |> List.choose f |> List.toArray
    let a = xs |> Array.choose f
    consistency "contains" s l a

[<Test>]
let ``choose is consistent`` () =
    smallerSizeCheck choose<int>
    smallerSizeCheck choose<string>
    smallerSizeCheck choose<float>

let chunkBySize<'a when 'a : equality> (xs : 'a []) size =
    let ls = List.ofArray xs
    if size <= 0 then 
        Prop.throws<ArgumentException,_> (lazy Seq.chunkBySize size xs) .&.
        Prop.throws<ArgumentException,_> (lazy Array.chunkBySize size xs) .&.
        Prop.throws<ArgumentException,_> (lazy List.chunkBySize size ls) 
    else
        let s = xs |> Seq.chunkBySize size |> Seq.map Seq.toArray |> Seq.toArray
        let l = ls |> List.chunkBySize size |> Seq.map Seq.toArray |> Seq.toArray
        let a = xs |> Array.chunkBySize size |> Seq.map Seq.toArray |> Seq.toArray
        consistency "chunkBySize" s l a


[<Test>]
let ``chunkBySize is consistent`` () =
    smallerSizeCheck chunkBySize<int>
    smallerSizeCheck chunkBySize<string>
    smallerSizeCheck chunkBySize<NormalFloat>

let collect<'a> (xs : 'a []) f  =
    let s = xs |> Seq.collect f |> Seq.toArray
    let l = xs |> List.ofArray |> List.collect (fun x -> f x |> List.ofArray) |> List.toArray
    let a = xs |> Array.collect f
    consistency "collect" s l a



[<Test>]
let ``collect is consistent`` () =
    smallerSizeCheck collect<int>
    smallerSizeCheck collect<string>
    smallerSizeCheck collect<float>

let compareWith<'a>(xs : 'a []) (xs2 : 'a []) f  =
    let s = (xs, xs2) ||> Seq.compareWith f
    let l = (List.ofArray xs, List.ofArray xs2) ||> List.compareWith f
    let a = (xs, xs2) ||> Array.compareWith f
    consistency "compareWith" s l a



[<Test>]
let ``compareWith is consistent`` () =
    smallerSizeCheck compareWith<int>
    smallerSizeCheck compareWith<string>
    smallerSizeCheck compareWith<float>
        
let concat<'a when 'a : equality> (xs : 'a [][]) =
    let s = xs |> Seq.concat |> Seq.toArray
    let l = xs |> List.ofArray |> List.map List.ofArray |> List.concat |> List.toArray
    let a = xs |> Array.concat
    consistency "concat" s l a

[<Test>]
let ``concat is consistent`` () =
    smallerSizeCheck concat<int>
    smallerSizeCheck concat<string>
    smallerSizeCheck concat<NormalFloat>

let countBy<'a> (xs : 'a []) f =
    let s = xs |> Seq.countBy f |> Seq.toArray
    let l = xs |> List.ofArray |> List.countBy f |> List.toArray
    let a = xs |> Array.countBy f
    consistency "countBy" s l a

[<Test>]
let ``countBy is consistent`` () =
    smallerSizeCheck countBy<int>
    smallerSizeCheck countBy<string>
    smallerSizeCheck countBy<float>

let distinct<'a when 'a : comparison> (xs : 'a []) =
    let s = xs |> Seq.distinct |> Seq.toArray
    let l = xs |> List.ofArray |> List.distinct |> List.toArray
    let a = xs |> Array.distinct 
    consistency "distinct" s l a

[<Test>]
let ``distinct is consistent`` () =
    smallerSizeCheck distinct<int>
    smallerSizeCheck distinct<string>
    smallerSizeCheck distinct<NormalFloat>

let distinctBy<'a when 'a : equality> (xs : 'a []) f =
    let s = xs |> Seq.distinctBy f |> Seq.toArray
    let l = xs |> List.ofArray |> List.distinctBy f |> List.toArray
    let a = xs |> Array.distinctBy f 
    consistency "distinctBy" s l a

[<Test>]
let ``distinctBy is consistent`` () =
    smallerSizeCheck distinctBy<int>
    smallerSizeCheck distinctBy<string>
    smallerSizeCheck distinctBy<NormalFloat>

let exactlyOne<'a when 'a : comparison> (xs : 'a []) =
    let s = runAndCheckErrorType (fun () -> xs |> Seq.exactlyOne)
    let l = runAndCheckErrorType (fun () -> xs |> List.ofArray |> List.exactlyOne)
    let a = runAndCheckErrorType (fun () -> xs |> Array.exactlyOne)
    consistency "exactlyOne" s l a

[<Test>]
let ``exactlyOne is consistent`` () =
    smallerSizeCheck exactlyOne<int>
    smallerSizeCheck exactlyOne<string>
    smallerSizeCheck exactlyOne<NormalFloat>

let tryExactlyOne<'a when 'a : comparison> (xs : 'a []) =
    let s = runAndCheckErrorType (fun () -> xs |> Seq.tryExactlyOne)
    let l = runAndCheckErrorType (fun () -> xs |> List.ofArray |> List.tryExactlyOne)
    let a = runAndCheckErrorType (fun () -> xs |> Array.tryExactlyOne)
    consistency "tryExactlyOne" s l a

[<Test>]
let ``tryExactlyOne is consistent`` () =
    smallerSizeCheck tryExactlyOne<int>
    smallerSizeCheck tryExactlyOne<string>
    smallerSizeCheck tryExactlyOne<NormalFloat>

let except<'a when 'a : equality> (xs : 'a []) (itemsToExclude: 'a []) =
    let s = xs |> Seq.except itemsToExclude |> Seq.toArray
    let l = xs |> List.ofArray |> List.except itemsToExclude |> List.toArray
    let a = xs |> Array.except itemsToExclude
    consistency "except" s l a

[<Test>]
let ``except is consistent`` () =
    smallerSizeCheck except<int>
    smallerSizeCheck except<string>
    smallerSizeCheck except<NormalFloat>

let exists<'a when 'a : equality> (xs : 'a []) f =
    let s = xs |> Seq.exists f
    let l = xs |> List.ofArray |> List.exists f
    let a = xs |> Array.exists f
    consistency "exists" s l a

[<Test>]
let ``exists is consistent`` () =
    smallerSizeCheck exists<int>
    smallerSizeCheck exists<string>
    smallerSizeCheck exists<NormalFloat>

let exists2<'a when 'a : equality> (xs':('a*'a) []) f =    
    let xs = Array.map fst xs'
    let xs2 = Array.map snd xs'
    let s = runAndCheckErrorType (fun () -> Seq.exists2 f xs xs2)
    let l = runAndCheckErrorType (fun () -> List.exists2 f (List.ofSeq xs) (List.ofSeq xs2))
    let a = runAndCheckErrorType (fun () -> Array.exists2 f (Array.ofSeq xs) (Array.ofSeq xs2))
    consistency "exists2" s l a
    
[<Test>]
let ``exists2 is consistent for collections with equal length`` () =
    smallerSizeCheck exists2<int>
    smallerSizeCheck exists2<string>
    smallerSizeCheck exists2<NormalFloat>

let filter<'a when 'a : equality> (xs : 'a []) predicate =
    let s = xs |> Seq.filter predicate
    let l = xs |> List.ofArray |> List.filter predicate
    let a = xs |> Array.filter predicate
    Seq.toArray s = a && List.toArray l = a

[<Test>]
let ``filter is consistent`` () =
    smallerSizeCheck filter<int>
    smallerSizeCheck filter<string>
    smallerSizeCheck filter<NormalFloat>

let find<'a when 'a : equality> (xs : 'a []) predicate =
    let s = run (fun () -> xs |> Seq.find predicate)
    let l = run (fun () -> xs |> List.ofArray |> List.find predicate)
    let a = run (fun () -> xs |> Array.find predicate)
    consistency "find" s l a

[<Test>]
let ``find is consistent`` () =
    smallerSizeCheck find<int>
    smallerSizeCheck find<string>
    smallerSizeCheck find<NormalFloat>

let findBack<'a when 'a : equality> (xs : 'a []) predicate =
    let s = run (fun () -> xs |> Seq.findBack predicate)
    let l = run (fun () -> xs |> List.ofArray |> List.findBack predicate)
    let a = run (fun () -> xs |> Array.findBack predicate)
    consistency "findBack" s l a

[<Test>]
let ``findBack is consistent`` () =
    smallerSizeCheck findBack<int>
    smallerSizeCheck findBack<string>
    smallerSizeCheck findBack<NormalFloat>

let findIndex<'a when 'a : equality> (xs : 'a []) predicate =
    let s = run (fun () -> xs |> Seq.findIndex predicate)
    let l = run (fun () -> xs |> List.ofArray |> List.findIndex predicate)
    let a = run (fun () -> xs |> Array.findIndex predicate)
    consistency "findIndex" s l a

[<Test>]
let ``findIndex is consistent`` () =
    smallerSizeCheck findIndex<int>
    smallerSizeCheck findIndex<string>
    smallerSizeCheck findIndex<NormalFloat>

let findIndexBack<'a when 'a : equality> (xs : 'a []) predicate =
    let s = run (fun () -> xs |> Seq.findIndexBack predicate)
    let l = run (fun () -> xs |> List.ofArray |> List.findIndexBack predicate)
    let a = run (fun () -> xs |> Array.findIndexBack predicate)
    consistency "findIndexBack" s l a

[<Test>]
let ``findIndexBack is consistent`` () =
    smallerSizeCheck findIndexBack<int>
    smallerSizeCheck findIndexBack<string>
    smallerSizeCheck findIndexBack<NormalFloat>

let fold<'a,'b when 'b : equality> (xs : 'a []) f (start:'b) =
    let s = run (fun () -> xs |> Seq.fold f start)
    let l = run (fun () -> xs |> List.ofArray |> List.fold f start)
    let a = run (fun () -> xs |> Array.fold f start)
    consistency "fold" s l a

[<Test>]
let ``fold is consistent`` () =
    smallerSizeCheck fold<int,int>
    smallerSizeCheck fold<string,string>
    smallerSizeCheck fold<float,int>
    smallerSizeCheck fold<float,string>

let fold2<'a,'b,'c when 'c : equality> (xs': ('a*'b)[]) f (start:'c) =
    let xs = xs' |> Array.map fst
    let xs2 = xs' |> Array.map snd
    let s = run (fun () -> Seq.fold2 f start xs xs2)
    let l = run (fun () -> List.fold2 f start (List.ofArray xs) (List.ofArray xs2))
    let a = run (fun () -> Array.fold2 f start xs xs2)
    consistency "fold2" s l a

[<Test>]
let ``fold2 is consistent`` () =
    smallerSizeCheck fold2<int,int,int>
    smallerSizeCheck fold2<string,string,string>
    smallerSizeCheck fold2<string,int,string>
    smallerSizeCheck fold2<string,float,int>
    smallerSizeCheck fold2<float,float,int>
    smallerSizeCheck fold2<float,float,string>

let foldBack<'a,'b when 'b : equality> (xs : 'a []) f (start:'b) =
    let s = run (fun () -> Seq.foldBack f xs start)
    let l = run (fun () -> List.foldBack f (xs |> List.ofArray) start)
    let a = run (fun () -> Array.foldBack f xs start)
    consistency "foldBack" s l a

[<Test>]
let ``foldBack is consistent`` () =
    smallerSizeCheck foldBack<int,int>
    smallerSizeCheck foldBack<string,string>
    smallerSizeCheck foldBack<float,int>
    smallerSizeCheck foldBack<float,string>

let foldBack2<'a,'b,'c when 'c : equality> (xs': ('a*'b)[]) f (start:'c) =
    let xs = xs' |> Array.map fst
    let xs2 = xs' |> Array.map snd
    let s = run (fun () -> Seq.foldBack2 f xs xs2 start)
    let l = run (fun () -> List.foldBack2 f (List.ofArray xs) (List.ofArray xs2) start)
    let a = run (fun () -> Array.foldBack2 f xs xs2 start)
    consistency "foldBack2" s l a

[<Test>]
let ``foldBack2 is consistent`` () =
    smallerSizeCheck foldBack2<int,int,int>
    smallerSizeCheck foldBack2<string,string,string>
    smallerSizeCheck foldBack2<string,int,string>
    smallerSizeCheck foldBack2<string,float,int>
    smallerSizeCheck foldBack2<float,float,int>
    smallerSizeCheck foldBack2<float,float,string>

let forall<'a when 'a : equality> (xs : 'a []) f =
    let s = xs |> Seq.forall f
    let l = xs |> List.ofArray |> List.forall f
    let a = xs |> Array.forall f
    consistency "forall" s l a

[<Test>]
let ``forall is consistent`` () =
    smallerSizeCheck forall<int>
    smallerSizeCheck forall<string>
    smallerSizeCheck forall<NormalFloat>

let forall2<'a when 'a : equality> (xs':('a*'a) []) f =    
    let xs = Array.map fst xs'
    let xs2 = Array.map snd xs'
    let s = runAndCheckErrorType (fun () -> Seq.forall2 f xs xs2)
    let l = runAndCheckErrorType (fun () -> List.forall2 f (List.ofSeq xs) (List.ofSeq xs2))
    let a = runAndCheckErrorType (fun () -> Array.forall2 f (Array.ofSeq xs) (Array.ofSeq xs2))
    consistency "forall2" s l a
    
[<Test>]
let ``forall2 is consistent for collections with equal length`` () =
    smallerSizeCheck forall2<int>
    smallerSizeCheck forall2<string>
    smallerSizeCheck forall2<NormalFloat>

let groupBy<'a when 'a : equality> (xs : 'a []) f =
    let s = run (fun () -> xs |> Seq.groupBy f |> Seq.toArray |> Array.map (fun (x,xs) -> x,xs |> Seq.toArray))
    let l = run (fun () -> xs |> List.ofArray |> List.groupBy f |> Seq.toArray |> Array.map (fun (x,xs) -> x,xs |> Seq.toArray))
    let a = run (fun () -> xs |> Array.groupBy f |> Array.map (fun (x,xs) -> x,xs |> Seq.toArray))
    consistency "groupBy" s l a

[<Test>]
let ``groupBy is consistent`` () =
    smallerSizeCheck groupBy<int>
    smallerSizeCheck groupBy<string>
    smallerSizeCheck groupBy<NormalFloat>

let head<'a when 'a : equality> (xs : 'a []) =
    let s = runAndCheckIfAnyError (fun () -> xs |> Seq.head)
    let l = runAndCheckIfAnyError (fun () -> xs |> List.ofArray |> List.head)
    let a = runAndCheckIfAnyError (fun () -> xs |> Array.head)
    consistency "head" s l a

[<Test>]
let ``head is consistent`` () =
    smallerSizeCheck head<int>
    smallerSizeCheck head<string>
    smallerSizeCheck head<NormalFloat>

let indexed<'a when 'a : equality> (xs : 'a []) =
    let s = xs |> Seq.indexed |> Seq.toArray
    let l = xs |> List.ofArray |> List.indexed |> List.toArray
    let a = xs |> Array.indexed
    consistency "indexed" s l a

[<Test>]
let ``indexed is consistent`` () =
    smallerSizeCheck indexed<int>
    smallerSizeCheck indexed<string>
    smallerSizeCheck indexed<NormalFloat>

let init<'a when 'a : equality> count f =
    let s = runAndCheckErrorType (fun () -> Seq.init count f |> Seq.toArray)
    let l = runAndCheckErrorType (fun () -> List.init count f |> Seq.toArray)
    let a = runAndCheckErrorType (fun () -> Array.init count f)
    consistency "init" s l a

[<Test>]
let ``init is consistent`` () =
    smallerSizeCheck init<int>
    smallerSizeCheck init<string>
    smallerSizeCheck init<NormalFloat>

let isEmpty<'a when 'a : equality> (xs : 'a []) =
    let s = xs |> Seq.isEmpty
    let l = xs |> List.ofArray |> List.isEmpty
    let a = xs |> Array.isEmpty
    consistency "isEmpty" s l a

[<Test>]
let ``isEmpty is consistent`` () =
    smallerSizeCheck isEmpty<int>
    smallerSizeCheck isEmpty<string>
    smallerSizeCheck isEmpty<NormalFloat>

let item<'a when 'a : equality> (xs : 'a []) index =
    let s = runAndCheckIfAnyError (fun () -> xs |> Seq.item index)
    let l = runAndCheckIfAnyError (fun () -> xs |> List.ofArray |> List.item index)
    let a = runAndCheckIfAnyError (fun () -> xs |> Array.item index)
    consistency "item" s l a

[<Test>]
let ``item is consistent`` () =
    smallerSizeCheck item<int>
    smallerSizeCheck item<string>
    smallerSizeCheck item<NormalFloat>

let iter<'a when 'a : equality> (xs : 'a []) f' =
    let list = System.Collections.Generic.List<'a>()
    let f x =
        list.Add x
        f' x

    let s = xs |> Seq.iter f
    let l = xs |> List.ofArray |> List.iter f
    let a =  xs |> Array.iter f

    let xs = Seq.toList xs
    list |> Seq.toList = (xs @ xs @ xs)

[<Test>]
let ``iter looks at every element exactly once and in order - consistenly over all collections`` () =
    smallerSizeCheck iter<int>
    smallerSizeCheck iter<string>
    smallerSizeCheck iter<NormalFloat>

let iter2<'a when 'a : equality> (xs' : ('a*'a) []) f' =
    let xs = xs' |> Array.map fst
    let xs2 = xs' |> Array.map snd
    let list = System.Collections.Generic.List<'a*'a>()
    let f x y =
        list.Add <| (x,y)
        f' x y

    let s = Seq.iter2 f xs xs2
    let l = List.iter2 f (xs |> List.ofArray) (xs2 |> List.ofArray)
    let a = Array.iter2 f xs xs2

    let xs = Seq.toList xs'
    list |> Seq.toList = (xs @ xs @ xs)

[<Test>]
let ``iter2 looks at every element exactly once and in order - consistenly over all collections when size is equal`` () =
    smallerSizeCheck iter2<int>
    smallerSizeCheck iter2<string>
    smallerSizeCheck iter2<NormalFloat>

let iteri<'a when 'a : equality> (xs : 'a []) f' =
    let list = System.Collections.Generic.List<'a>()
    let indices = System.Collections.Generic.List<int>()
    let f i x =
        list.Add x
        indices.Add i
        f' i x

    let s = xs |> Seq.iteri f
    let l = xs |> List.ofArray |> List.iteri f
    let a =  xs |> Array.iteri f

    let xs = Seq.toList xs
    list |> Seq.toList = (xs @ xs @ xs) &&
      indices |> Seq.toList = ([0..xs.Length-1] @ [0..xs.Length-1] @ [0..xs.Length-1])

[<Test>]
let ``iteri looks at every element exactly once and in order - consistenly over all collections`` () =
    smallerSizeCheck iteri<int>
    smallerSizeCheck iteri<string>
    smallerSizeCheck iteri<NormalFloat>

let iteri2<'a when 'a : equality> (xs' : ('a*'a) []) f' =
    let xs = xs' |> Array.map fst
    let xs2 = xs' |> Array.map snd
    let list = System.Collections.Generic.List<'a*'a>()
    let indices = System.Collections.Generic.List<int>()
    let f i x y =
        list.Add <| (x,y)
        indices.Add i
        f' x y

    let s = Seq.iteri2 f xs xs2
    let l = List.iteri2 f (xs |> List.ofArray) (xs2 |> List.ofArray)
    let a = Array.iteri2 f xs xs2

    let xs = Seq.toList xs'
    list |> Seq.toList = (xs @ xs @ xs) &&
      indices |> Seq.toList = ([0..xs.Length-1] @ [0..xs.Length-1] @ [0..xs.Length-1])

[<Test>]
let ``iteri2 looks at every element exactly once and in order - consistenly over all collections when size is equal`` () =
    smallerSizeCheck iteri2<int>
    smallerSizeCheck iteri2<string>
    smallerSizeCheck iteri2<NormalFloat>

let last<'a when 'a : equality> (xs : 'a []) =
    let s = runAndCheckIfAnyError (fun () -> xs |> Seq.last)
    let l = runAndCheckIfAnyError (fun () -> xs |> List.ofArray |> List.last)
    let a = runAndCheckIfAnyError (fun () -> xs |> Array.last)
    consistency "last" s l a

[<Test>]
let ``last is consistent`` () =
    smallerSizeCheck last<int>
    smallerSizeCheck last<string>
    smallerSizeCheck last<NormalFloat>

let length<'a when 'a : equality> (xs : 'a []) =
    let s = xs |> Seq.length
    let l = xs |> List.ofArray |> List.length
    let a = xs |> Array.length
    consistency "length" s l a

[<Test>]
let ``length is consistent`` () =
    smallerSizeCheck length<int>
    smallerSizeCheck length<string>
    smallerSizeCheck length<float>

let map<'a when 'a : equality> (xs : 'a []) f =
    let s = xs |> Seq.map f |> Seq.toArray
    let l = xs |> List.ofArray |> List.map f |> List.toArray
    let a = xs |> Array.map f
    consistency "map" s l a

[<Test>]
let ``map is consistent`` () =
    smallerSizeCheck map<int>
    smallerSizeCheck map<string>
    smallerSizeCheck map<float>

let map2<'a when 'a : equality> (xs' : ('a*'a) []) f' =
    let xs = xs' |> Array.map fst
    let xs2 = xs' |> Array.map snd
    let list = System.Collections.Generic.List<'a*'a>()
    let f x y =
        list.Add <| (x,y)
        f' x y

    let s = Seq.map2 f xs xs2
    let l = List.map2 f (xs |> List.ofArray) (xs2 |> List.ofArray)
    let a = Array.map2 f xs xs2

    let xs = Seq.toList xs'    
    Seq.toArray s = a && List.toArray l = a &&
      list |> Seq.toList = (xs @ xs @ xs)

[<Test>]
let ``map2 looks at every element exactly once and in order - consistenly over all collections when size is equal`` () =
    smallerSizeCheck map2<int>
    smallerSizeCheck map2<string>
    smallerSizeCheck map2<NormalFloat>

let map3<'a when 'a : equality> (xs' : ('a*'a*'a) []) f' =
    let xs = xs' |> Array.map  (fun (x,y,z) -> x)
    let xs2 = xs' |> Array.map (fun (x,y,z) -> y)
    let xs3 = xs' |> Array.map (fun (x,y,z) -> z)
    let list = System.Collections.Generic.List<'a*'a*'a>()
    let f x y z =
        list.Add <| (x,y,z)
        f' x y z

    let s = Seq.map3 f xs xs2 xs3
    let l = List.map3 f (xs |> List.ofArray) (xs2 |> List.ofArray) (xs3 |> List.ofArray)
    let a = Array.map3 f xs xs2 xs3

    let xs = Seq.toList xs'
    Seq.toArray s = a && List.toArray l = a &&
      list |> Seq.toList = (xs @ xs @ xs)

[<Test>]
let ``map3 looks at every element exactly once and in order - consistenly over all collections when size is equal`` () =
    smallerSizeCheck map3<int>
    smallerSizeCheck map3<string>
    smallerSizeCheck map3<NormalFloat>

let mapFold<'a when 'a : equality> (xs : 'a []) f start =
    let s,sr = xs |> Seq.mapFold f start
    let l,lr = xs |> List.ofArray |> List.mapFold f start
    let a,ar = xs |> Array.mapFold f start
    Seq.toArray s = a && List.toArray l = a &&
      sr = lr && sr = ar

[<Test>]
let ``mapFold is consistent`` () =
    smallerSizeCheck mapFold<int>
    smallerSizeCheck mapFold<string>
    smallerSizeCheck mapFold<NormalFloat>

let mapFoldBack<'a when 'a : equality> (xs : 'a []) f start =
    let s,sr = Seq.mapFoldBack f xs start
    let l,lr = List.mapFoldBack f (xs |> List.ofArray) start
    let a,ar = Array.mapFoldBack f xs start
    Seq.toArray s = a && List.toArray l = a &&
      sr = lr && sr = ar

[<Test>]
let ``mapFold2 is consistent`` () =
    smallerSizeCheck mapFoldBack<int>
    smallerSizeCheck mapFoldBack<string>
    smallerSizeCheck mapFoldBack<NormalFloat>

let mapi<'a when 'a : equality> (xs : 'a []) f =
    let s = xs |> Seq.mapi f
    let l = xs |> List.ofArray |> List.mapi f
    let a = xs |> Array.mapi f
    Seq.toArray s = a && List.toArray l = a

[<Test>]
let ``mapi is consistent`` () =
    smallerSizeCheck mapi<int>
    smallerSizeCheck mapi<string>
    smallerSizeCheck mapi<float>

let mapi2<'a when 'a : equality> (xs' : ('a*'a) []) f' =
    let xs = xs' |> Array.map fst
    let xs2 = xs' |> Array.map snd
    let list = System.Collections.Generic.List<'a*'a>()
    let indices = System.Collections.Generic.List<int>()
    let f i x y =
        indices.Add i
        list.Add <| (x,y)
        f' x y

    let s = Seq.mapi2 f xs xs2
    let l = List.mapi2 f (xs |> List.ofArray) (xs2 |> List.ofArray)
    let a = Array.mapi2 f xs xs2

    let xs = Seq.toList xs'    
    Seq.toArray s = a && List.toArray l = a &&
      list |> Seq.toList = (xs @ xs @ xs) &&
      (Seq.toList indices = [0..xs.Length-1] @ [0..xs.Length-1] @ [0..xs.Length-1])

[<Test>]
let ``mapi2 looks at every element exactly once and in order - consistenly over all collections when size is equal`` () =
    smallerSizeCheck mapi2<int>
    smallerSizeCheck mapi2<string>
    smallerSizeCheck mapi2<NormalFloat>

let max<'a when 'a : comparison> (xs : 'a []) =
    let s = runAndCheckIfAnyError (fun () -> xs |> Seq.max)
    let l = runAndCheckIfAnyError (fun () -> xs |> List.ofArray |> List.max)
    let a = runAndCheckIfAnyError (fun () -> xs |> Array.max)
    consistency "max" s l a

[<Test>]
let ``max is consistent`` () =
    smallerSizeCheck max<int>
    smallerSizeCheck max<string>
    smallerSizeCheck max<NormalFloat>

let maxBy<'a when 'a : comparison> (xs : 'a []) f =
    let s = runAndCheckIfAnyError (fun () -> xs |> Seq.maxBy f)
    let l = runAndCheckIfAnyError (fun () -> xs |> List.ofArray |> List.maxBy f)
    let a = runAndCheckIfAnyError (fun () -> xs |> Array.maxBy f)
    consistency "maxBy" s l a

[<Test>]
let ``maxBy is consistent`` () =
    smallerSizeCheck maxBy<int>
    smallerSizeCheck maxBy<string>
    smallerSizeCheck maxBy<NormalFloat>
 
let min<'a when 'a : comparison> (xs : 'a []) =
    let s = runAndCheckIfAnyError (fun () -> xs |> Seq.min)
    let l = runAndCheckIfAnyError (fun () -> xs |> List.ofArray |> List.min)
    let a = runAndCheckIfAnyError (fun () -> xs |> Array.min)
    consistency "min" s l a

[<Test>]
let ``min is consistent`` () =
    smallerSizeCheck min<int>
    smallerSizeCheck min<string>
    smallerSizeCheck min<NormalFloat>

let minBy<'a when 'a : comparison> (xs : 'a []) f =
    let s = runAndCheckIfAnyError (fun () -> xs |> Seq.minBy f)
    let l = runAndCheckIfAnyError (fun () -> xs |> List.ofArray |> List.minBy f)
    let a = runAndCheckIfAnyError (fun () -> xs |> Array.minBy f)
    consistency "minBy" s l a

[<Test>]
let ``minBy is consistent`` () =
    smallerSizeCheck minBy<int>
    smallerSizeCheck minBy<string>
    smallerSizeCheck minBy<NormalFloat>

let pairwise<'a when 'a : comparison> (xs : 'a []) =
    let s = run (fun () -> xs |> Seq.pairwise |> Seq.toArray)
    let l = run (fun () -> xs |> List.ofArray |> List.pairwise |> List.toArray)
    let a = run (fun () -> xs |> Array.pairwise)
    consistency "pairwise" s l a

[<Test>]
let ``pairwise is consistent`` () =
    smallerSizeCheck pairwise<int>
    smallerSizeCheck pairwise<string>
    smallerSizeCheck pairwise<NormalFloat>

let partition<'a when 'a : comparison> (xs : 'a []) f =
    // no seq version
    let l1,l2 = xs |> List.ofArray |> List.partition f
    let a1,a2 = xs |> Array.partition f
    List.toArray l1 = a1 &&
      List.toArray l2 = a2

[<Test>]
let ``partition is consistent`` () =
    smallerSizeCheck partition<int>
    smallerSizeCheck partition<string>
    smallerSizeCheck partition<NormalFloat>

let permute<'a when 'a : comparison> (xs' : list<int*'a>) =
    let xs = List.map snd xs'
 
    let permutations = 
        List.map fst xs'
        |> List.indexed
        |> List.sortBy snd
        |> List.map fst
        |> List.indexed
        |> dict

    let permutation x = permutations.[x]

    let s = run (fun () -> xs |> Seq.permute permutation |> Seq.toArray)
    let l = run (fun () -> xs |> List.permute permutation |> List.toArray)
    let a = run (fun () -> xs |> Array.ofSeq |> Array.permute permutation)
    consistency "partition" s l a

[<Test>]
let ``permute is consistent`` () =
    smallerSizeCheck permute<int>
    smallerSizeCheck permute<string>
    smallerSizeCheck permute<NormalFloat>

let pick<'a when 'a : comparison> (xs : 'a []) f =
    let s = run (fun () -> xs |> Seq.pick f)
    let l = run (fun () -> xs |> List.ofArray |> List.pick f)
    let a = run (fun () -> xs |> Array.pick f)
    consistency "pick" s l a

[<Test>]
let ``pick is consistent`` () =
    smallerSizeCheck pick<int>
    smallerSizeCheck pick<string>
    smallerSizeCheck pick<NormalFloat>

let reduce<'a when 'a : equality> (xs : 'a []) f =
    let s = runAndCheckErrorType (fun () -> xs |> Seq.reduce f)
    let l = runAndCheckErrorType (fun () -> xs |> List.ofArray |> List.reduce f)
    let a = runAndCheckErrorType (fun () -> xs |> Array.reduce f)
    consistency "reduce" s l a

[<Test>]
let ``reduce is consistent`` () =
    smallerSizeCheck reduce<int>
    smallerSizeCheck reduce<string>
    smallerSizeCheck reduce<NormalFloat>

let reduceBack<'a when 'a : equality> (xs : 'a []) f =
    let s = runAndCheckErrorType (fun () -> xs |> Seq.reduceBack f)
    let l = runAndCheckErrorType (fun () -> xs |> List.ofArray |> List.reduceBack f)
    let a = runAndCheckErrorType (fun () -> xs |> Array.reduceBack f)
    consistency "reduceBack" s l a

[<Test>]
let ``reduceBack is consistent`` () =
    smallerSizeCheck reduceBack<int>
    smallerSizeCheck reduceBack<string>
    smallerSizeCheck reduceBack<NormalFloat>

let replicate<'a when 'a : equality> x count =
    let s = runAndCheckIfAnyError (fun () -> Seq.replicate count x |> Seq.toArray)
    let l = runAndCheckIfAnyError (fun () -> List.replicate count x |> List.toArray)
    let a = runAndCheckIfAnyError (fun () -> Array.replicate count x)
    consistency "replicate" s l a

[<Test>]
let ``replicate is consistent`` () =
    smallerSizeCheck replicate<int>
    smallerSizeCheck replicate<string>
    smallerSizeCheck replicate<NormalFloat>

let rev<'a when 'a : equality> (xs : 'a []) =
    let s = Seq.rev xs |> Seq.toArray
    let l = xs |> List.ofArray |> List.rev |> List.toArray
    let a = Array.rev xs
    consistency "rev" s l a

[<Test>]
let ``rev is consistent`` () =
    smallerSizeCheck rev<int>
    smallerSizeCheck rev<string>
    smallerSizeCheck rev<NormalFloat>

let scan<'a,'b when 'b : equality> (xs : 'a []) f (start:'b) =
    let s = run (fun () -> xs |> Seq.scan f start |> Seq.toArray)
    let l = run (fun () -> xs |> List.ofArray |> List.scan f start |> Seq.toArray)
    let a = run (fun () -> xs |> Array.scan f start)
    consistency "scan" s l a

[<Test>]
let ``scan is consistent`` () =
    smallerSizeCheck scan<int,int>
    smallerSizeCheck scan<string,string>
    smallerSizeCheck scan<float,int>
    smallerSizeCheck scan<float,string>

let scanBack<'a,'b when 'b : equality> (xs : 'a []) f (start:'b) =
    let s = run (fun () -> Seq.scanBack f xs start |> Seq.toArray)
    let l = run (fun () -> List.scanBack f (xs |> List.ofArray) start |> Seq.toArray)
    let a = run (fun () -> Array.scanBack f xs start)
    consistency "scanback" s l a

[<Test>]
let ``scanBack is consistent`` () =
    smallerSizeCheck scanBack<int,int>
    smallerSizeCheck scanBack<string,string>
    smallerSizeCheck scanBack<float,int>
    smallerSizeCheck scanBack<float,string>

let singleton<'a when 'a : equality> (x : 'a) =
    let s = Seq.singleton x |> Seq.toArray
    let l = List.singleton x |> List.toArray
    let a = Array.singleton x
    consistency "singleton" s l a

[<Test>]
let ``singleton is consistent`` () =
    smallerSizeCheck singleton<int>
    smallerSizeCheck singleton<string>
    smallerSizeCheck singleton<NormalFloat>

let skip<'a when 'a : equality> (xs : 'a []) count =
    let s = runAndCheckIfAnyError (fun () -> Seq.skip count xs |> Seq.toArray)
    let l = runAndCheckIfAnyError (fun () -> List.skip count (Seq.toList xs) |> List.toArray)
    let a = runAndCheckIfAnyError (fun () -> Array.skip count xs)
    consistency "skip" s l a

[<Test>]
let ``skip is consistent`` () =
    smallerSizeCheck skip<int>
    smallerSizeCheck skip<string>
    smallerSizeCheck skip<NormalFloat>

let skipWhile<'a when 'a : equality> (xs : 'a []) f =
    let s = runAndCheckIfAnyError (fun () -> Seq.skipWhile f xs |> Seq.toArray)
    let l = runAndCheckIfAnyError (fun () -> List.skipWhile f (Seq.toList xs) |> List.toArray)
    let a = runAndCheckIfAnyError (fun () -> Array.skipWhile f xs)
    consistency "skipWhile" s l a

[<Test>]
let ``skipWhile is consistent`` () =
    smallerSizeCheck skipWhile<int>
    smallerSizeCheck skipWhile<string>
    smallerSizeCheck skipWhile<NormalFloat>

let sort<'a when 'a : comparison> (xs : 'a []) =
    let s = xs |> Seq.sort |> Seq.toArray
    let l = xs |> List.ofArray |> List.sort |> List.toArray
    let a = xs |> Array.sort
    consistency "sort" s l a

[<Test>]
let ``sort is consistent`` () =
    smallerSizeCheck sort<int>
    smallerSizeCheck sort<string>
    smallerSizeCheck sort<NormalFloat>

let sortBy<'a,'b when 'a : comparison and 'b : comparison> (xs : 'a []) (f:'a -> 'b) =
    let s = xs |> Seq.sortBy f
    let l = xs |> List.ofArray |> List.sortBy f
    let a = xs |> Array.sortBy f

    isSorted (Seq.map f s) && isSorted (Seq.map f l) && isSorted (Seq.map f a) &&
      haveSameElements s xs && haveSameElements l xs && haveSameElements a xs

[<Test>]
let ``sortBy actually sorts (but is inconsistent in regards of stability)`` () =
    smallerSizeCheck sortBy<int,int>
    smallerSizeCheck sortBy<int,string>
    smallerSizeCheck sortBy<string,string>
    smallerSizeCheck sortBy<string,int>
    smallerSizeCheck sortBy<NormalFloat,int>

let sortWith<'a,'b when 'a : comparison and 'b : comparison> (xs : 'a []) =
    let f x y = 
        if x = y then 0 else
        if x = Unchecked.defaultof<_> && y <> Unchecked.defaultof<_> then -1 else
        if y = Unchecked.defaultof<_> && x <> Unchecked.defaultof<_> then 1 else
        if x < y then -1 else 1

    let s = xs |> Seq.sortWith f
    let l = xs |> List.ofArray |> List.sortWith f
    let a = xs |> Array.sortWith f
    let isSorted sorted = sorted |> Seq.pairwise |> Seq.forall (fun (a,b) -> f a b <= 0 || a = b)

    isSorted s && isSorted l && isSorted a &&
        haveSameElements s xs && haveSameElements l xs && haveSameElements a xs

[<Test>]
let ``sortWith actually sorts (but is inconsistent in regards of stability)`` () =
    smallerSizeCheck sortWith<int,int>
    smallerSizeCheck sortWith<int,string>
    smallerSizeCheck sortWith<string,string>
    smallerSizeCheck sortWith<string,int>
    smallerSizeCheck sortWith<NormalFloat,int>

let sortDescending<'a when 'a : comparison> (xs : 'a []) =
    let s = xs |> Seq.sortDescending |> Seq.toArray
    let l = xs |> List.ofArray |> List.sortDescending |> List.toArray
    let a = xs |> Array.sortDescending
    consistency "sortDescending" s l a

[<Test>]
let ``sortDescending is consistent`` () =
    smallerSizeCheck sortDescending<int>
    smallerSizeCheck sortDescending<string>
    smallerSizeCheck sortDescending<NormalFloat>

let sortByDescending<'a,'b when 'a : comparison and 'b : comparison> (xs : 'a []) (f:'a -> 'b) =
    let s = xs |> Seq.sortByDescending f
    let l = xs |> List.ofArray |> List.sortByDescending f
    let a = xs |> Array.sortByDescending f

    isSorted (Seq.map f s |> Seq.rev) && isSorted (Seq.map f l |> Seq.rev) && isSorted (Seq.map f a |> Seq.rev) &&
      haveSameElements s xs && haveSameElements l xs && haveSameElements a xs

[<Test>]
let ``sortByDescending actually sorts (but is inconsistent in regards of stability)`` () =
    smallerSizeCheck sortByDescending<int,int>
    smallerSizeCheck sortByDescending<int,string>
    smallerSizeCheck sortByDescending<string,string>
    smallerSizeCheck sortByDescending<string,int>
    smallerSizeCheck sortByDescending<NormalFloat,int>

let sum (xs : int []) =
    let s = run (fun () -> xs |> Seq.sum)
    let l = run (fun () -> xs |> Array.toList |> List.sum)
    let a = run (fun () -> xs |> Array.sum)
    consistency "sum" s l a

[<Test>]
let ``sum is consistent`` () =
    smallerSizeCheck sum

let sumBy<'a> (xs : 'a []) (f:'a -> int) =
    let s = run (fun () -> xs |> Seq.sumBy f)
    let l = run (fun () -> xs |> Array.toList |> List.sumBy f)
    let a = run (fun () -> xs |> Array.sumBy f)
    consistency "sumBy" s l a

[<Test>]
let ``sumBy is consistent`` () =
    smallerSizeCheck sumBy<int>
    smallerSizeCheck sumBy<string>
    smallerSizeCheck sumBy<float>

let splitAt<'a when 'a : equality> (xs : 'a []) index =
    let ls = List.ofArray xs
    if index < 0 then 
        Prop.throws<ArgumentException,_> (lazy List.splitAt index ls) .&.
        Prop.throws<ArgumentException,_> (lazy Array.splitAt index xs) 
    elif index > xs.Length then
        Prop.throws<InvalidOperationException,_> (lazy List.splitAt index ls) .&.
        Prop.throws<InvalidOperationException,_> (lazy Array.splitAt index xs) 
    else
        // no seq version
        let l = run (fun () -> ls |> List.splitAt index |> fun (a,b) -> List.toArray a,List.toArray b)
        let a = run (fun () -> xs |> Array.splitAt index)
        (l = a) |@ "splitAt"

[<Test>]
let ``splitAt is consistent`` () =
    smallerSizeCheck splitAt<int>
    smallerSizeCheck splitAt<string>
    smallerSizeCheck splitAt<NormalFloat>

let splitInto<'a when 'a : equality> (xs : 'a []) count =
    let ls = List.ofArray xs
    if count < 1 then 
        Prop.throws<ArgumentException,_> (lazy List.splitInto count ls) .&.
        Prop.throws<ArgumentException,_> (lazy Array.splitInto count xs) .&.
        Prop.throws<ArgumentException,_> (lazy Seq.splitInto count xs) 
    else
        let s = run (fun () -> xs |> Seq.splitInto count |> Seq.map Seq.toArray |> Seq.toArray)
        let l = run (fun () -> ls |> List.splitInto count |> Seq.map Seq.toArray |> Seq.toArray)
        let a = run (fun () -> xs |> Array.splitInto count |> Seq.map Seq.toArray |> Seq.toArray)
        consistency "splitInto" s l a

[<Test>]
let ``splitInto is consistent`` () =
    smallerSizeCheck splitInto<int>
    smallerSizeCheck splitInto<string>
    smallerSizeCheck splitInto<NormalFloat>

let tail<'a when 'a : equality> (xs : 'a []) =
    let s = runAndCheckIfAnyError (fun () -> xs |> Seq.tail |> Seq.toArray)
    let l = runAndCheckIfAnyError (fun () -> xs |> List.ofArray |> List.tail |> Seq.toArray)
    let a = runAndCheckIfAnyError (fun () -> xs |> Array.tail)
    consistency "tail" s l a

[<Test>]
let ``tail is consistent`` () =
    smallerSizeCheck tail<int>
    smallerSizeCheck tail<string>
    smallerSizeCheck tail<NormalFloat>

let take<'a when 'a : equality> (xs : 'a []) count =
    let s = runAndCheckIfAnyError (fun () -> Seq.take count xs |> Seq.toArray)
    let l = runAndCheckIfAnyError (fun () -> List.take count (Seq.toList xs) |> List.toArray)
    let a = runAndCheckIfAnyError (fun () -> Array.take count xs)
    consistency "take" s l a

[<Test>]
let ``take is consistent`` () =
    smallerSizeCheck take<int>
    smallerSizeCheck take<string>
    smallerSizeCheck take<NormalFloat>

let takeWhile<'a when 'a : equality> (xs : 'a []) f =
    let s = runAndCheckIfAnyError (fun () -> Seq.takeWhile f xs |> Seq.toArray)
    let l = runAndCheckIfAnyError (fun () -> List.takeWhile f (Seq.toList xs) |> List.toArray)
    let a = runAndCheckIfAnyError (fun () -> Array.takeWhile f xs)
    consistency "takeWhile" s l a

[<Test>]
let ``takeWhile is consistent`` () =
    smallerSizeCheck takeWhile<int>
    smallerSizeCheck takeWhile<string>
    smallerSizeCheck takeWhile<NormalFloat>

let truncate<'a when 'a : equality> (xs : 'a []) count =
    let s = runAndCheckIfAnyError (fun () -> Seq.truncate count xs |> Seq.toArray)
    let l = runAndCheckIfAnyError (fun () -> List.truncate count (Seq.toList xs) |> List.toArray)
    let a = runAndCheckIfAnyError (fun () -> Array.truncate count xs)
    consistency "truncate" s l a

[<Test>]
let ``truncate is consistent`` () =
    smallerSizeCheck truncate<int>
    smallerSizeCheck truncate<string>
    smallerSizeCheck truncate<NormalFloat>

let tryFind<'a when 'a : equality> (xs : 'a []) predicate =
    let s = xs |> Seq.tryFind predicate
    let l = xs |> List.ofArray |> List.tryFind predicate
    let a = xs |> Array.tryFind predicate
    consistency "tryFind" s l a

[<Test>]
let ``tryFind is consistent`` () =
    smallerSizeCheck tryFind<int>
    smallerSizeCheck tryFind<string>
    smallerSizeCheck tryFind<NormalFloat>

let tryFindBack<'a when 'a : equality> (xs : 'a []) predicate =
    let s = xs |> Seq.tryFindBack predicate
    let l = xs |> List.ofArray |> List.tryFindBack predicate
    let a = xs |> Array.tryFindBack predicate
    consistency "tryFindBack" s l a

[<Test>]
let ``tryFindBack is consistent`` () =
    smallerSizeCheck tryFindBack<int>
    smallerSizeCheck tryFindBack<string>
    smallerSizeCheck tryFindBack<NormalFloat>

let tryFindIndex<'a when 'a : equality> (xs : 'a []) predicate =
    let s = xs |> Seq.tryFindIndex predicate
    let l = xs |> List.ofArray |> List.tryFindIndex predicate
    let a = xs |> Array.tryFindIndex predicate
    consistency "tryFindIndex" s l a

[<Test>]
let ``tryFindIndex is consistent`` () =
    smallerSizeCheck tryFindIndex<int>
    smallerSizeCheck tryFindIndex<string>
    smallerSizeCheck tryFindIndex<NormalFloat>

let tryFindIndexBack<'a when 'a : equality> (xs : 'a []) predicate =
    let s = xs |> Seq.tryFindIndexBack predicate
    let l = xs |> List.ofArray |> List.tryFindIndexBack predicate
    let a = xs |> Array.tryFindIndexBack predicate
    consistency "tryFindIndexBack" s l a

[<Test>]
let ``tryFindIndexBack is consistent`` () =
    smallerSizeCheck tryFindIndexBack<int>
    smallerSizeCheck tryFindIndexBack<string>
    smallerSizeCheck tryFindIndexBack<NormalFloat>

let tryHead<'a when 'a : equality> (xs : 'a []) =
    let s = xs |> Seq.tryHead
    let l = xs |> List.ofArray |> List.tryHead
    let a = xs |> Array.tryHead
    consistency "tryHead" s l a

[<Test>]
let ``tryHead is consistent`` () =
    smallerSizeCheck tryHead<int>
    smallerSizeCheck tryHead<string>
    smallerSizeCheck tryHead<NormalFloat>

let tryItem<'a when 'a : equality> (xs : 'a []) index =
    let s = xs |> Seq.tryItem index
    let l = xs |> List.ofArray |> List.tryItem index
    let a = xs |> Array.tryItem index
    consistency "tryItem" s l a

[<Test>]
let ``tryItem is consistent`` () =
    smallerSizeCheck tryItem<int>
    smallerSizeCheck tryItem<string>
    smallerSizeCheck tryItem<NormalFloat>

let tryLast<'a when 'a : equality> (xs : 'a []) =
    let s = xs |> Seq.tryLast
    let l = xs |> List.ofArray |> List.tryLast
    let a = xs |> Array.tryLast
    consistency "tryLast" s l a

[<Test>]
let ``tryLast is consistent`` () =
    smallerSizeCheck tryLast<int>
    smallerSizeCheck tryLast<string>
    smallerSizeCheck tryLast<NormalFloat>

let tryPick<'a when 'a : comparison> (xs : 'a []) f =
    let s = xs |> Seq.tryPick f
    let l = xs |> List.ofArray |> List.tryPick f
    let a = xs |> Array.tryPick f
    consistency "tryPick" s l a

[<Test>]
let ``tryPick is consistent`` () =
    smallerSizeCheck tryPick<int>
    smallerSizeCheck tryPick<string>
    smallerSizeCheck tryPick<NormalFloat>

let unfold<'a,'b when 'b : equality> f (start:'a) =
    let f() =
        let c = ref 0
        fun x -> 
            if !c > 100 then None else // prevent infinity seqs
            c := !c + 1
            f x
    let s : 'b [] = Seq.unfold (f()) start |> Seq.toArray
    let l = List.unfold (f()) start |> List.toArray
    let a = Array.unfold (f()) start
    consistency "unfold" s l a


[<Test>]
let ``unfold is consistent`` () =
    smallerSizeCheck unfold<int,int>


#if EXPENSIVE
[<Test; Category("Expensive"); Explicit>]
let ``unfold is consistent full`` () =
    smallerSizeCheck unfold<int,int>
    smallerSizeCheck unfold<string,string>
    smallerSizeCheck unfold<float,int>
    smallerSizeCheck unfold<float,string>
#endif

let unzip<'a when 'a : equality> (xs:('a*'a) []) =       
    // no seq version
    let l = runAndCheckErrorType (fun () -> List.unzip (List.ofSeq xs) |> fun (a,b) -> List.toArray a, List.toArray b)
    let a = runAndCheckErrorType (fun () -> Array.unzip xs)
    l = a
    
[<Test>]
let ``unzip is consistent`` () =
    smallerSizeCheck unzip<int>
    smallerSizeCheck unzip<string>
    smallerSizeCheck unzip<NormalFloat>

let unzip3<'a when 'a : equality> (xs:('a*'a*'a) []) =       
    // no seq version
    let l = runAndCheckErrorType (fun () -> List.unzip3 (List.ofSeq xs) |> fun (a,b,c) -> List.toArray a, List.toArray b, List.toArray c)
    let a = runAndCheckErrorType (fun () -> Array.unzip3 xs)
    l = a
    
[<Test>]
let ``unzip3 is consistent`` () =
    smallerSizeCheck unzip3<int>
    smallerSizeCheck unzip3<string>
    smallerSizeCheck unzip3<NormalFloat>

let where<'a when 'a : equality> (xs : 'a []) predicate =
    let s = xs |> Seq.where predicate |> Seq.toArray
    let l = xs |> List.ofArray |> List.where predicate |> List.toArray
    let a = xs |> Array.where predicate
    consistency "where" s l a

[<Test>]
let ``where is consistent`` () =
    smallerSizeCheck where<int>
    smallerSizeCheck where<string>
    smallerSizeCheck where<NormalFloat>

let windowed<'a when 'a : equality> (xs : 'a []) windowSize =
    let ls = List.ofArray xs
    if windowSize < 1 then 
        Prop.throws<ArgumentException,_> (lazy   Seq.windowed windowSize xs) .&.
        Prop.throws<ArgumentException,_> (lazy Array.windowed windowSize xs) .&.
        Prop.throws<ArgumentException,_> (lazy  List.windowed windowSize ls)
    else
        let s = run (fun () -> xs |> Seq.windowed windowSize |> Seq.toArray |> Array.map Seq.toArray)
        let l = run (fun () -> ls |> List.windowed windowSize |> List.toArray |> Array.map Seq.toArray)
        let a = run (fun () -> xs |> Array.windowed windowSize)
        consistency "windowed" s l a

[<Test>]
let ``windowed is consistent`` () =
    smallerSizeCheck windowed<int>
    smallerSizeCheck windowed<string>
    smallerSizeCheck windowed<NormalFloat>

let zip<'a when 'a : equality> (xs':('a*'a) []) =    
    let xs = Array.map fst xs'
    let xs2 = Array.map snd xs'
    let s = runAndCheckErrorType (fun () -> Seq.zip xs xs2 |> Seq.toArray)
    let l = runAndCheckErrorType (fun () -> List.zip (List.ofSeq xs) (List.ofSeq xs2) |> List.toArray)
    let a = runAndCheckErrorType (fun () -> Array.zip (Array.ofSeq xs) (Array.ofSeq xs2))
    consistency "zip" s l a
    
[<Test>]
let ``zip is consistent for collections with equal length`` () =
    smallerSizeCheck zip<int>
    smallerSizeCheck zip<string>
    smallerSizeCheck zip<NormalFloat>

let zip3<'a when 'a : equality> (xs':('a*'a*'a) []) =    
    let xs = Array.map (fun (x,y,z) -> x) xs'
    let xs2 = Array.map (fun (x,y,z) -> y) xs'
    let xs3 = Array.map (fun (x,y,z) -> z) xs'
    let s = runAndCheckErrorType (fun () -> Seq.zip3 xs xs2 xs3 |> Seq.toArray)
    let l = runAndCheckErrorType (fun () -> List.zip3 (List.ofSeq xs) (List.ofSeq xs2) (List.ofSeq xs3) |> List.toArray)
    let a = runAndCheckErrorType (fun () -> Array.zip3 (Array.ofSeq xs) (Array.ofSeq xs2) (Array.ofSeq xs3))
    consistency "zip3" s l a
    
[<Test>]
let ``zip3 is consistent for collections with equal length`` () =
    smallerSizeCheck zip3<int>
    smallerSizeCheck zip3<string>
    smallerSizeCheck zip3<NormalFloat>

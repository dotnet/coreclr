// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.
[<NUnit.Framework.Category "Collections.List">][<NUnit.Framework.Category "FSharp.Core.Collections">]
module FSharp.Core.UnitTests.FSharp_Core.Microsoft_FSharp_Collections.ListProperties

open System
open System.Collections.Generic
open NUnit.Framework
open FsCheck
open Utils

let chunkBySize_and_collect<'a when 'a : equality> (xs : 'a list) size =
    size > 0 ==> (lazy
        let a = List.chunkBySize size xs
        let b = List.collect id a
        b = xs)

[<Test>]
let ``chunkBySize is reversable with collect`` () =
    Check.QuickThrowOnFailure chunkBySize_and_collect<int>
    Check.QuickThrowOnFailure chunkBySize_and_collect<string>
    Check.QuickThrowOnFailure chunkBySize_and_collect<NormalFloat>

let windowed_and_length<'a when 'a : equality> (xs : 'a list) size =
    size > 0 ==> (lazy
        List.windowed size xs
        |> List.forall (fun x -> x.Length = size))


[<Test>]
let ``windowed returns list with correct length`` () =
    Check.QuickThrowOnFailure windowed_and_length<int>
    Check.QuickThrowOnFailure windowed_and_length<string>
    Check.QuickThrowOnFailure windowed_and_length<NormalFloat>

let windowed_and_order<'a when 'a : equality> (listsize:PositiveInt) size =
    size > 1 ==> (lazy
        let xs = [1..(int listsize)]

        List.windowed size xs
        |> List.forall (fun w -> w = List.sort w))

[<Test>]
let ``windowed returns succeeding elements`` () =
    Check.QuickThrowOnFailure windowed_and_order<int>
    Check.QuickThrowOnFailure windowed_and_order<string>
    Check.QuickThrowOnFailure windowed_and_order<NormalFloat>

let partition_and_sort<'a when 'a : comparison> (xs : 'a list) =
    let rec qsort xs = 
        match xs with
        | [] -> []
        | (x:'a)::xs -> 
            let smaller,larger = List.partition (fun y -> y <= x) xs
            qsort smaller @ [x] @ qsort larger

    qsort xs = (List.sort xs)

[<Test>]
let ``partition can be used to sort`` () =
    Check.QuickThrowOnFailure partition_and_sort<int>
    Check.QuickThrowOnFailure partition_and_sort<string>
    Check.QuickThrowOnFailure partition_and_sort<NormalFloat>

let windowed_and_pairwise<'a when 'a : equality> (xs : 'a list) =
    let a = List.windowed 2 xs
    let b = List.pairwise xs |> List.map (fun (x,y) -> [x;y])

    a = b

[<Test>]
let ``windowed 2 is like pairwise`` () =
    Check.QuickThrowOnFailure windowed_and_pairwise<int>
    Check.QuickThrowOnFailure windowed_and_pairwise<string>
    Check.QuickThrowOnFailure windowed_and_pairwise<NormalFloat>

[<Test>]
let ``chunkBySize produces chunks exactly of size `chunkSize`, except the last one, which can be smaller, but not empty``() =
    let prop (a: _ list) (PositiveInt chunkSize) =   
        match a |> List.chunkBySize chunkSize |> Seq.toList with
        | [] -> a = []
        | h :: [] -> h.Length <= chunkSize
        | chunks ->
            let lastChunk = chunks |> List.last
            let headChunks = chunks |> Seq.take (chunks.Length - 1) |> Seq.toList

            headChunks |> List.forall (List.length >> (=) chunkSize)
            &&
            lastChunk <> []
            &&
            lastChunk.Length <= chunkSize

    Check.QuickThrowOnFailure prop

let splitInto_and_collect<'a when 'a : equality> (xs : 'a list) count =
    count > 0 ==> (lazy
        let a = List.splitInto count xs
        let b = List.collect id a
        b = xs)

[<Test>]
let ``splitInto is reversable with collect`` () =
    Check.QuickThrowOnFailure splitInto_and_collect<int>
    Check.QuickThrowOnFailure splitInto_and_collect<string>
    Check.QuickThrowOnFailure splitInto_and_collect<NormalFloat>

let splitAt_and_append<'a when 'a : equality> (xs : 'a list) index =
    (index >= 0 && index <= xs.Length) ==> (lazy
        let a1,a2 = List.splitAt index xs
        let b = List.append a1 a2
        b = xs && a1.Length = index)

[<Test>]
let ``splitAt is reversable with append`` () =
    Check.QuickThrowOnFailure splitAt_and_append<int>
    Check.QuickThrowOnFailure splitAt_and_append<string>
    Check.QuickThrowOnFailure splitAt_and_append<NormalFloat>

let indexed_and_zip<'a when 'a : equality> (xs : 'a list) =
    let a = [0..xs.Length-1]
    let b = List.indexed xs
    b = List.zip a xs

[<Test>]
let ``indexed is adding correct indexes`` () =
    Check.QuickThrowOnFailure indexed_and_zip<int>
    Check.QuickThrowOnFailure indexed_and_zip<string>
    Check.QuickThrowOnFailure indexed_and_zip<NormalFloat>

let zip_and_zip3<'a when 'a : equality> (xs' : ('a*'a*'a) list) =
    let xs = List.map (fun (x,y,z) -> x) xs'
    let xs2 = List.map (fun (x,y,z) -> y) xs'
    let xs3 = List.map (fun (x,y,z) -> z) xs'

    let a = List.zip3 xs xs2 xs3
    let b = List.zip (List.zip xs xs2) xs3 |> List.map (fun ((a,b),c) -> a,b,c)
    a = b

[<Test>]
let ``two zips can be used for zip3`` () =
    Check.QuickThrowOnFailure zip_and_zip3<int>
    Check.QuickThrowOnFailure zip_and_zip3<string>
    Check.QuickThrowOnFailure zip_and_zip3<NormalFloat>

let zip_and_unzip<'a when 'a : equality> (xs' : ('a*'a) list) =
    let xs,xs2 = List.unzip xs'
    List.zip xs xs2 = xs'

[<Test>]
let ``zip and unzip are dual`` () =
    Check.QuickThrowOnFailure zip_and_unzip<int>
    Check.QuickThrowOnFailure zip_and_unzip<string>
    Check.QuickThrowOnFailure zip_and_unzip<NormalFloat>

let zip3_and_unzip3<'a when 'a : equality> (xs' : ('a*'a*'a) list) =
    let xs,xs2,xs3 = List.unzip3 xs'
    List.zip3 xs xs2 xs3 = xs'

[<Test>]
let ``zip3 and unzip3 are dual`` () =
    Check.QuickThrowOnFailure zip3_and_unzip3<int>
    Check.QuickThrowOnFailure zip3_and_unzip3<string>
    Check.QuickThrowOnFailure zip3_and_unzip3<NormalFloat>

[<Test>]
let ``splitInto produces chunks exactly `count` chunks with equal size (+/- 1)``() =
    let prop (a: _ list) (PositiveInt count') =
        let count = min a.Length count'
        match a |> List.splitInto count' |> Seq.toList with
        | [] -> a = []
        | h :: [] -> (a.Length = 1 || count = 1) && h = a
        | chunks ->
            let lastChunk = chunks |> List.last
            let lastLength = lastChunk |> List.length

            chunks.Length = count
            &&
            chunks |> List.forall (fun c -> List.length c = lastLength || List.length c = lastLength + 1)

    Check.QuickThrowOnFailure prop

let sort_and_sortby (xs : list<float>) (xs2 : list<float>) =
    let a = List.sortBy id xs |> Seq.toArray 
    let b = List.sort xs |> Seq.toArray
    let result = ref true
    for i in 0 .. a.Length - 1 do
        if a.[i] <> b.[i] then
            if System.Double.IsNaN a.[i] <> System.Double.IsNaN b.[i] then
                result := false
    !result 

[<Test>]
let ``sort behaves like sortby id`` () =   
    Check.QuickThrowOnFailure sort_and_sortby

let filter_and_except<'a when 'a : comparison>  (xs : list<'a>) (itemsToExclude : Set<'a>) =
    let a = List.filter (fun x -> Set.contains x itemsToExclude |> not) xs |> List.distinct
    let b = List.except itemsToExclude xs
    a = b

[<Test>]
let ``filter and except work similar`` () =   
    Check.QuickThrowOnFailure filter_and_except<int>
    Check.QuickThrowOnFailure filter_and_except<string>
    Check.QuickThrowOnFailure filter_and_except<NormalFloat>    

let filter_and_where<'a when 'a : comparison>  (xs : list<'a>) predicate =
    let a = List.filter predicate xs
    let b = List.where predicate xs
    a = b

[<Test>]
let ``filter and where work similar`` () =   
    Check.QuickThrowOnFailure filter_and_where<int>
    Check.QuickThrowOnFailure filter_and_where<string>
    Check.QuickThrowOnFailure filter_and_where<NormalFloat>

let find_and_pick<'a when 'a : comparison>  (xs : list<'a>) predicate =
    let a = runAndCheckIfAnyError (fun () -> List.find predicate xs)
    let b = runAndCheckIfAnyError (fun () -> List.pick (fun x -> if predicate x then Some x else None) xs)
    a = b

[<Test>]
let ``pick works like find`` () =   
    Check.QuickThrowOnFailure find_and_pick<int>
    Check.QuickThrowOnFailure find_and_pick<string>
    Check.QuickThrowOnFailure find_and_pick<NormalFloat>

let choose_and_pick<'a when 'a : comparison>  (xs : list<'a>) predicate =
    let a = runAndCheckIfAnyError (fun () -> List.choose predicate xs |> List.head)
    let b = runAndCheckIfAnyError (fun () -> List.pick predicate xs)
    a = b

[<Test>]
let ``pick works like choose + head`` () =   
    Check.QuickThrowOnFailure choose_and_pick<int>
    Check.QuickThrowOnFailure choose_and_pick<string>
    Check.QuickThrowOnFailure choose_and_pick<NormalFloat>

let head_and_tail<'a when 'a : comparison>  (xs : list<'a>) =
    xs <> [] ==> (lazy
        let h = List.head xs
        let t = List.tail xs
        xs = h :: t)

[<Test>]
let ``head and tail gives the list`` () =   
    Check.QuickThrowOnFailure head_and_tail<int>
    Check.QuickThrowOnFailure head_and_tail<string>
    Check.QuickThrowOnFailure head_and_tail<NormalFloat>

let tryHead_and_tail<'a when 'a : comparison>  (xs : list<'a>) =
    match xs with
    | [] -> List.tryHead xs = None
    | _ ->
        let h = (List.tryHead xs).Value
        let t = List.tail xs
        xs = h :: t

[<Test>]
let ``tryHead and tail gives the list`` () =   
    Check.QuickThrowOnFailure tryHead_and_tail<int>
    Check.QuickThrowOnFailure tryHead_and_tail<string>
    Check.QuickThrowOnFailure tryHead_and_tail<NormalFloat>

let skip_and_take<'a when 'a : comparison>  (xs : list<'a>) (count:NonNegativeInt) =
    let count = int count
    if xs <> [] && count <= xs.Length then
        let s = List.skip count xs
        let t = List.take count xs
        xs = t @ s
    else true

[<Test>]
let ``skip and take gives the list`` () =   
    Check.QuickThrowOnFailure skip_and_take<int>
    Check.QuickThrowOnFailure skip_and_take<string>
    Check.QuickThrowOnFailure skip_and_take<NormalFloat>

let truncate_and_take<'a when 'a : comparison>  (xs : list<'a>) (count:NonNegativeInt) =
    let count = int count
    if xs <> [] && count <= xs.Length then
        let a = List.take (min count xs.Length) xs
        let b = List.truncate count xs
        a = b
    else true

[<Test>]
let ``truncate and take work similar`` () =   
    Check.QuickThrowOnFailure truncate_and_take<int>
    Check.QuickThrowOnFailure truncate_and_take<string>
    Check.QuickThrowOnFailure truncate_and_take<NormalFloat>

let skipWhile_and_takeWhile<'a when 'a : comparison>  (xs : list<'a>) f =
    if xs <> [] then
        let s = List.skipWhile f xs
        let t = List.takeWhile f xs
        xs = t @ s
    else true

[<Test>]
let ``skipWhile and takeWhile gives the list`` () =   
    Check.QuickThrowOnFailure skipWhile_and_takeWhile<int>
    Check.QuickThrowOnFailure skipWhile_and_takeWhile<string>
    Check.QuickThrowOnFailure skipWhile_and_takeWhile<NormalFloat>

let find_and_exists<'a when 'a : comparison>  (xs : list<'a>) f =
    let a = 
        try
            List.find f xs |> ignore
            true
        with
        | _ -> false
    let b = List.exists f xs
    a = b

[<Test>]
let ``find and exists work similar`` () =   
    Check.QuickThrowOnFailure find_and_exists<int>
    Check.QuickThrowOnFailure find_and_exists<string>
    Check.QuickThrowOnFailure find_and_exists<NormalFloat>

let exists_and_forall<'a when 'a : comparison>  (xs : list<'a>) (F (_, predicate)) =
    let a = List.forall (predicate >> not) xs
    let b = List.exists predicate xs
    a = not b

[<Test>]
let ``exists and forall are dual`` () =   
    Check.QuickThrowOnFailure exists_and_forall<int>
    Check.QuickThrowOnFailure exists_and_forall<string>
    Check.QuickThrowOnFailure exists_and_forall<NormalFloat>

let head_and_isEmpty<'a when 'a : comparison>  (xs : list<'a>) =
    let a = 
        try
            List.head xs |> ignore
            true
        with
        | _ -> false
    let b = List.isEmpty xs

    a = not b

[<Test>]
let ``head fails when list isEmpty`` () =   
    Check.QuickThrowOnFailure head_and_isEmpty<int>
    Check.QuickThrowOnFailure head_and_isEmpty<string>
    Check.QuickThrowOnFailure head_and_isEmpty<NormalFloat>

let head_and_last<'a when 'a : comparison>  (xs : list<'a>) =
    let a = run (fun () -> xs |> List.rev |> List.last)
    let b = run (fun () -> List.head xs)

    a = b

[<Test>]
let ``head is the same as last of a reversed list`` () =   
    Check.QuickThrowOnFailure head_and_last<int>
    Check.QuickThrowOnFailure head_and_last<string>
    Check.QuickThrowOnFailure head_and_last<NormalFloat>

let head_and_item<'a when 'a : comparison>  (xs : list<'a>) =
    let a = runAndCheckErrorType (fun () -> xs |> List.item 0)
    let b = runAndCheckErrorType (fun () -> List.head xs)

    a = b

[<Test>]
let ``head is the same as item 0`` () =   
    Check.QuickThrowOnFailure head_and_item<int>
    Check.QuickThrowOnFailure head_and_item<string>
    Check.QuickThrowOnFailure head_and_item<NormalFloat>

let item_and_tryItem<'a when 'a : comparison>  (xs : list<'a>) pos =
    let a = runAndCheckErrorType (fun () -> xs |> List.item pos)
    let b = List.tryItem pos xs

    match a with
    | Success a -> b.Value = a
    | _ -> b = None

[<Test>]
let ``tryItem is safe item`` () =   
    Check.QuickThrowOnFailure item_and_tryItem<int>
    Check.QuickThrowOnFailure item_and_tryItem<string>
    Check.QuickThrowOnFailure item_and_tryItem<NormalFloat>

let pick_and_tryPick<'a when 'a : comparison>  (xs : list<'a>) f =
    let a = runAndCheckErrorType (fun () -> xs |> List.pick f)
    let b = List.tryPick f xs

    match a with
    | Success a -> b.Value = a
    | _ -> b = None

[<Test>]
let ``tryPick is safe pick`` () =   
    Check.QuickThrowOnFailure pick_and_tryPick<int>
    Check.QuickThrowOnFailure pick_and_tryPick<string>
    Check.QuickThrowOnFailure pick_and_tryPick<NormalFloat>

let last_and_tryLast<'a when 'a : comparison>  (xs : list<'a>) =
    let a = runAndCheckErrorType (fun () -> xs |> List.last)
    let b = List.tryLast xs

    match a with
    | Success a -> b.Value = a
    | _ -> b = None

[<Test>]
let ``tryLast is safe last`` () =   
    Check.QuickThrowOnFailure last_and_tryLast<int>
    Check.QuickThrowOnFailure last_and_tryLast<string>
    Check.QuickThrowOnFailure last_and_tryLast<NormalFloat>

let length_and_isEmpty<'a when 'a : comparison>  (xs : list<'a>) =
    let a = List.length xs = 0
    let b = List.isEmpty xs

    a = b

[<Test>]
let ``list isEmpty if and only if length is 0`` () =   
    Check.QuickThrowOnFailure length_and_isEmpty<int>
    Check.QuickThrowOnFailure length_and_isEmpty<string>
    Check.QuickThrowOnFailure length_and_isEmpty<NormalFloat>

let min_and_max (xs : list<int>) =
    let a = run (fun () -> List.min xs)
    let b = run (fun () -> xs |> List.map ((*) -1) |> List.max |> fun x -> -x)

    a = b

[<Test>]
let ``min is opposite of max`` () =   
    Check.QuickThrowOnFailure min_and_max

let minBy_and_maxBy (xs : list<int>) f =
    let a = run (fun () -> List.minBy f xs)
    let b = run (fun () -> xs |> List.map ((*) -1) |> List.maxBy f |> fun x -> -x)

    a = b

[<Test>]
let ``minBy is opposite of maxBy`` () =   
    Check.QuickThrowOnFailure minBy_and_maxBy

let minBy_and_min (xs : list<int>) =
    let a = run (fun () -> List.minBy id xs)
    let b = run (fun () -> xs |> List.min)

    a = b

[<Test>]
let ``minBy id is same as min`` () =   
    Check.QuickThrowOnFailure minBy_and_min

let min_and_sort<'a when 'a : comparison>  (xs : list<'a>) =
    let a = runAndCheckErrorType (fun () -> List.min xs)
    let b = runAndCheckErrorType (fun () -> xs |> List.sort |> List.head)

    a = b

[<Test>]
let ``head element after sort is min element`` () =   
    Check.QuickThrowOnFailure min_and_sort<int>
    Check.QuickThrowOnFailure min_and_sort<string>
    Check.QuickThrowOnFailure min_and_sort<NormalFloat>

let pairwise<'a when 'a : comparison>  (xs : list<'a>) =
    let xs' = List.pairwise xs 
    let f = xs' |> List.map fst
    let s = xs' |> List.map snd
    let a = List.length xs'
    let b = List.length xs

    if xs = [] then 
        xs' = []
    else 
        a = b - 1 &&
          f = (xs |> List.rev |> List.tail |> List.rev) && // all elements but last one
          s = (xs |> List.tail) // all elements but first one

[<Test>]
let ``pairwise works as expected`` () =   
    Check.QuickThrowOnFailure pairwise<int>
    Check.QuickThrowOnFailure pairwise<string>
    Check.QuickThrowOnFailure pairwise<NormalFloat>

let permute<'a when 'a : comparison>  (xs' : list<int*'a>) =
    let xs = List.map snd xs'
 
    let permutations = 
        List.map fst xs'
        |> List.indexed
        |> List.sortBy snd
        |> List.map fst
        |> List.indexed
        |> dict

    let permutation x = permutations.[x]


    match run (fun () -> xs |> List.indexed |> List.permute permutation) with
    | Success s ->         
        let originals = s |> List.map fst
        let rs = s |> List.map snd
        for o in originals do
            let x' = xs |> List.item o
            let x = rs |> List.item (permutation o)
            Assert.AreEqual(x',x)
        true
    | _ -> true

[<Test>]
let ``permute works as expected`` () =   
    Check.QuickThrowOnFailure permute<int>
    Check.QuickThrowOnFailure permute<string>
    Check.QuickThrowOnFailure permute<NormalFloat>

let mapi_and_map<'a when 'a : comparison>  (xs : list<'a>) f =
    let indices = System.Collections.Generic.List<int>()
    let f' i x =
        indices.Add i
        f x
    let a = List.map f xs
    let b = List.mapi f' xs

    a = b && (Seq.toList indices = [0..xs.Length-1])

[<Test>]
let ``mapi behaves like map with correct order`` () =   
    Check.QuickThrowOnFailure mapi_and_map<int>
    Check.QuickThrowOnFailure mapi_and_map<string>
    Check.QuickThrowOnFailure mapi_and_map<NormalFloat>

let reduce_and_fold<'a when 'a : comparison> (xs : list<'a>) seed (F (_, f)) =
    match xs with
    | [] -> List.fold f seed xs = seed
    | _ ->
        let ar = xs |> List.fold f seed
        let br = seed :: xs  |> List.reduce f
        ar = br

[<Test>]
let ``reduce works like fold with given seed`` () =
    Check.QuickThrowOnFailure reduce_and_fold<int>
    Check.QuickThrowOnFailure reduce_and_fold<string>
    Check.QuickThrowOnFailure reduce_and_fold<NormalFloat>

let scan_and_fold<'a when 'a : comparison> (xs : list<'a>) seed (F (_, f)) =
    let ar : 'a list = List.scan f seed xs
    let f' (l,c) x =
        let c' = f c x
        c'::l,c'

    let br,_ = List.fold f' ([seed],seed) xs
    ar = List.rev br


[<Test>]
let ``scan works like fold but returns intermediate values`` () =
    Check.QuickThrowOnFailure scan_and_fold<int>
    Check.QuickThrowOnFailure scan_and_fold<string>
    Check.QuickThrowOnFailure scan_and_fold<NormalFloat>

let scanBack_and_foldBack<'a when 'a : comparison> (xs : list<'a>) seed (F (_, f)) =
    let ar : 'a list = List.scanBack f xs seed
    let f' x (l,c) =
        let c' = f x c
        c'::l,c'

    let br,_ = List.foldBack f' xs ([seed],seed)
    ar = br


[<Test>]
let ``scanBack works like foldBack but returns intermediate values`` () =
    Check.QuickThrowOnFailure scanBack_and_foldBack<int>
    Check.QuickThrowOnFailure scanBack_and_foldBack<string>
    Check.QuickThrowOnFailure scanBack_and_foldBack<NormalFloat>

let reduceBack_and_foldBack<'a when 'a : comparison> (xs : list<'a>) seed (F (_, f)) =
    match xs with
    | [] -> List.foldBack f xs seed = seed
    | _ ->
        let ar = List.foldBack f xs seed
        let br = List.reduceBack f (xs @ [seed])
        ar = br

[<Test>]
let ``reduceBack works like foldBack with given seed`` () =
    Check.QuickThrowOnFailure reduceBack_and_foldBack<int>
    Check.QuickThrowOnFailure reduceBack_and_foldBack<string>
    Check.QuickThrowOnFailure reduceBack_and_foldBack<NormalFloat>

let replicate<'a when 'a : comparison> (x:'a) (count:NonNegativeInt) =
    let count = int count
    let xs = List.replicate count x
    xs.Length = count && List.forall ((=) x) xs

[<Test>]
let ``replicate creates n instances of the given element`` () =
    Check.QuickThrowOnFailure replicate<int>
    Check.QuickThrowOnFailure replicate<string>
    Check.QuickThrowOnFailure replicate<NormalFloat>

let singleton_and_replicate<'a when 'a : comparison> (x:'a) (count:NonNegativeInt) =
    let count = int count
    let xs = List.replicate count x
    let ys = [for i in 1..count -> List.singleton x] |> List.concat
    xs = ys

[<Test>]
let ``singleton can be used to replicate`` () =
    Check.QuickThrowOnFailure singleton_and_replicate<int>
    Check.QuickThrowOnFailure singleton_and_replicate<string>
    Check.QuickThrowOnFailure singleton_and_replicate<NormalFloat>

let mapFold_and_map_and_fold<'a when 'a : comparison> (xs : list<'a>) mapF foldF start =
    let f s x = 
        let x' = mapF x
        let s' = foldF s x'
        x',s'

    let a,ar = xs |> List.mapFold f start
    let b = xs |> List.map mapF 
    let br = b |> List.fold foldF start
    a = b && ar = br

[<Test>]
let ``mapFold works like map + fold`` () =   
    Check.QuickThrowOnFailure mapFold_and_map_and_fold<int>
    Check.QuickThrowOnFailure mapFold_and_map_and_fold<string>
    Check.QuickThrowOnFailure mapFold_and_map_and_fold<NormalFloat>

let mapFoldBack_and_map_and_foldBack<'a when 'a : comparison> (xs : list<'a>) mapF foldF start =
    let f x s = 
        let x' = mapF x
        let s' = foldF x' s
        x',s'

    let a,ar = List.mapFoldBack f xs start
    let b = xs |> List.map mapF 
    let br = List.foldBack foldF b start
    a = b && ar = br

[<Test>]
let ``mapFoldBack works like map + foldBack`` () =   
    Check.QuickThrowOnFailure mapFoldBack_and_map_and_foldBack<int>
    Check.QuickThrowOnFailure mapFoldBack_and_map_and_foldBack<string>
    Check.QuickThrowOnFailure mapFoldBack_and_map_and_foldBack<NormalFloat>

let findBack_and_exists<'a when 'a : comparison>  (xs : list<'a>) f =
    let a = 
        try
            List.findBack f xs |> ignore
            true
        with
        | _ -> false
    let b = List.exists f xs
    a = b

[<Test>]
let ``findBack and exists work similar`` () =   
    Check.QuickThrowOnFailure findBack_and_exists<int>
    Check.QuickThrowOnFailure findBack_and_exists<string>
    Check.QuickThrowOnFailure findBack_and_exists<NormalFloat>

let findBack_and_find<'a when 'a : comparison>  (xs : list<'a>) predicate =
    let a = run (fun () -> xs |> List.findBack predicate)
    let b = run (fun () -> xs |> List.rev |> List.find predicate)
    a = b

[<Test>]
let ``findBack and find work in reverse`` () =   
    Check.QuickThrowOnFailure findBack_and_find<int>
    Check.QuickThrowOnFailure findBack_and_find<string>
    Check.QuickThrowOnFailure findBack_and_find<NormalFloat>

let tryFindBack_and_tryFind<'a when 'a : comparison>  (xs : list<'a>) predicate =
    let a = xs |> List.tryFindBack predicate
    let b = xs |> List.rev |> List.tryFind predicate
    a = b

[<Test>]
let ``tryFindBack and tryFind work in reverse`` () =   
    Check.QuickThrowOnFailure tryFindBack_and_tryFind<int>
    Check.QuickThrowOnFailure tryFindBack_and_tryFind<string>
    Check.QuickThrowOnFailure tryFindBack_and_tryFind<NormalFloat>

let tryFindIndexBack_and_tryFindIndex<'a when 'a : comparison>  (xs : list<'a>) predicate =
    let a = xs |> List.tryFindIndexBack predicate
    let b = xs |> List.rev |> List.tryFindIndex predicate
    match a,b with
    | Some a, Some b -> a = (xs.Length - b - 1)
    | _ -> a = b

[<Test>]
let ``tryFindIndexBack and tryIndexFind work in reverse`` () =   
    Check.QuickThrowOnFailure tryFindIndexBack_and_tryFindIndex<int>
    Check.QuickThrowOnFailure tryFindIndexBack_and_tryFindIndex<string>
    Check.QuickThrowOnFailure tryFindIndexBack_and_tryFindIndex<NormalFloat>

let rev<'a when 'a : comparison>  (xs : list<'a>) =
    let list = System.Collections.Generic.List<_>()
    for x in xs do
        list.Insert(0,x)

    xs |> List.rev |> List.rev = xs && Seq.toList list = List.rev xs

[<Test>]
let ``rev reverses a list`` () =   
    Check.QuickThrowOnFailure rev<int>
    Check.QuickThrowOnFailure rev<string>
    Check.QuickThrowOnFailure rev<NormalFloat>

let findIndexBack_and_findIndex<'a when 'a : comparison>  (xs : list<'a>) (F (_, predicate)) =
    let a = run (fun () -> xs |> List.findIndex predicate)
    let b = run (fun () -> xs |> List.rev |> List.findIndexBack predicate)
    match a,b with
    | Success a, Success b -> a = (xs.Length - b - 1)
    | _ -> a = b

[<Test>]
let ``findIndexBack and findIndex work in reverse`` () =
    Check.QuickThrowOnFailure findIndexBack_and_findIndex<int>
    Check.QuickThrowOnFailure findIndexBack_and_findIndex<string>
    Check.QuickThrowOnFailure findIndexBack_and_findIndex<NormalFloat>

let skip_and_skipWhile<'a when 'a : comparison>  (xs : list<'a>) (count:NonNegativeInt) =
    let count = int count
    count <= xs.Length ==> (lazy 
        let ys = List.indexed xs
        let a = runAndCheckErrorType (fun () -> List.skip count ys)
        let b = runAndCheckErrorType (fun () -> List.skipWhile (fun (p,_) -> p < count) ys)

        a = b)

[<Test>]
let ``skip and skipWhile are consistent`` () =   
    Check.QuickThrowOnFailure skip_and_skipWhile<int>
    Check.QuickThrowOnFailure skip_and_skipWhile<string>
    Check.QuickThrowOnFailure skip_and_skipWhile<NormalFloat>

let distinct_works_like_set<'a when 'a : comparison> (xs : 'a list) =
    let a = List.distinct xs
    let b = Set.ofList xs

    let result = ref (a.Length = b.Count)
    for x in a do
        if Set.contains x b |> not then
            result := false

    for x in b do
        if List.exists ((=) x) a |> not then
            result := false
    !result

[<Test>]
let ``distinct creates same elements like a set`` () =
    Check.QuickThrowOnFailure distinct_works_like_set<int>
    Check.QuickThrowOnFailure distinct_works_like_set<string>
    Check.QuickThrowOnFailure distinct_works_like_set<NormalFloat>

let sort_and_sortDescending<'a when 'a : comparison>  (xs : list<'a>) =
    let a = run (fun () -> xs |> List.sort)
    let b = run (fun () -> xs |> List.sortDescending |> List.rev)
    a = b

[<Test>]
let ``sort and sortDescending work in reverse`` () =   
    Check.QuickThrowOnFailure sort_and_sortDescending<int>
    Check.QuickThrowOnFailure sort_and_sortDescending<string>
    Check.QuickThrowOnFailure sort_and_sortDescending<NormalFloat>

let sortByStable<'a when 'a : comparison> (xs : 'a []) =
    let indexed = xs |> Seq.indexed |> Seq.toList
    let sorted = indexed |> List.sortBy snd
    isStable sorted
    
[<Test>]
let ``List.sortBy is stable`` () =
    Check.QuickThrowOnFailure sortByStable<int>
    Check.QuickThrowOnFailure sortByStable<string>

let sortWithStable<'a when 'a : comparison> (xs : 'a []) =
    let indexed = xs |> Seq.indexed |> Seq.toList
    let sorted = indexed |> List.sortWith (fun x y -> compare (snd x) (snd y))
    isStable sorted
    
[<Test>]
let ``List.sortWithStable is stable`` () =
    Check.QuickThrowOnFailure sortWithStable<int>
    Check.QuickThrowOnFailure sortWithStable<string>

let distinctByStable<'a when 'a : comparison> (xs : 'a []) =
    let indexed = xs |> Seq.indexed |> Seq.toList
    let sorted = indexed |> List.distinctBy snd
    isStable sorted
    
[<Test>]
let ``List.distinctBy is stable`` () =
    Check.QuickThrowOnFailure distinctByStable<int>
    Check.QuickThrowOnFailure distinctByStable<string>
    
[<Test>]
let ``List.sum calculates the sum`` () =
    let sum (xs : int list) =
        let s = List.sum xs
        let r = ref 0
        for x in xs do r := !r + x    
        s = !r
    Check.QuickThrowOnFailure sum

let sumBy<'a> (xs : 'a list) (f:'a -> int) =
    let s = xs |> List.map f |> List.sum
    let r = List.sumBy f xs
    r = s

[<Test>]
let ``List.sumBy calculates the sum of the mapped list`` () =
    Check.QuickThrowOnFailure sumBy<int>
    Check.QuickThrowOnFailure sumBy<string> 
    Check.QuickThrowOnFailure sumBy<float>

let allPairsCount<'a, 'b> (xs : 'a list) (ys : 'b list) =
    let pairs = List.allPairs xs ys
    pairs.Length = xs.Length * ys.Length

[<Test>]
let ``List.allPairs produces the correct number of pairs`` () =
    Check.QuickThrowOnFailure allPairsCount<int, int>
    Check.QuickThrowOnFailure allPairsCount<string, string>
    Check.QuickThrowOnFailure allPairsCount<float, float>

let allPairsFst<'a, 'b when 'a : equality> (xs : 'a list) (ys : 'b list) =
    let pairsFst = List.allPairs xs ys |> List.map fst
    let check = xs |> List.collect (List.replicate ys.Length)
    pairsFst = check

[<Test>]
let ``List.allPairs first elements are correct`` () =
    Check.QuickThrowOnFailure allPairsFst<int, int>
    Check.QuickThrowOnFailure allPairsFst<string, string>
    Check.QuickThrowOnFailure allPairsFst<NormalFloat, NormalFloat>

let allPairsSnd<'a, 'b when 'b : equality> (xs : 'a list) (ys : 'b list) =
    let pairsSnd = List.allPairs xs ys |> List.map snd
    let check = [ for i in 1 .. xs.Length do yield! ys ]
    pairsSnd = check

[<Test>]
let ``List.allPairs second elements are correct`` () =
    Check.QuickThrowOnFailure allPairsFst<int, int>
    Check.QuickThrowOnFailure allPairsFst<string, string>
    Check.QuickThrowOnFailure allPairsFst<NormalFloat, NormalFloat>

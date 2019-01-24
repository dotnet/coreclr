// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

open System.Threading

type MyForm(Tests) as this =
    inherit System.Windows.Forms.Form()
    do 
        this.Load.Add(fun _ ->
            Tests()
            ThreadPool.QueueUserWorkItem(fun _ -> Thread.Sleep(1000); this.Invoke(new System.Action(fun _ -> this.Close())) |> ignore) |> ignore
        )

[<EntryPoint>]
let Main(args) =
    let orig = Thread.CurrentThread.ManagedThreadId
    let ev = new Event<_>()
    let pub = ev.Publish 

    let SWCandContThrowsTimeout(a, timeout:int) =
        let events = ResizeArray()
        let add(s:string) =
            events.Add(s, Thread.CurrentThread.ManagedThreadId=orig)
        use uhe = System.AppDomain.CurrentDomain.UnhandledException.Subscribe(fun e -> add "unhandled")
        use fe = pub.Subscribe(fun s -> add s)
        try
            Async.StartWithContinuations( 
                a, 
                (fun _ -> add "ok"; failwith "boom"), 
                (fun _ -> add "error"), 
                (fun _ -> add "cancel" ) 
            )
        with
            e -> add("caught:"+e.Message)
        Thread.Sleep(timeout)
        events.ToArray()

    let SWCandContThrows a =
        SWCandContThrowsTimeout(a, 500)

    let EmptyParallel() =
        printf "EmptyParallel "
        let r = SWCandContThrows(Async.Parallel [])
        printfn "%A" r

    let NonEmptyParallel() =
        printf "NonEmptyParallel "
        let r = SWCandContThrows(Async.Parallel [async.Return 0])
        printfn "%A" r

    let ParallelSeqArgumentThrows() =
        printf "ParallelSeqArgumentThrows "
        let r = SWCandContThrows(Async.Parallel (seq { yield async.Return 0; failwith "zap" }))
        printfn "%A" r

    let SleepReturn(n) =
        printf "Sleep%dReturn " n
        let a =
            async { do! Async.Sleep n
                    return 0 }
        let r = SWCandContThrows(a)
        printfn "%A" r

    let Return() =
        printf "Return "
        let r = SWCandContThrows(async.Return 0)
        printfn "%A" r

    let FromContinuations() =
        printf "FromContinuationsSuccess "
        let r = SWCandContThrows(Async.FromContinuations(fun (cont, _, _) -> cont 10))
        printfn "%A" r
        printf "FromContinuationsError "
        let r = SWCandContThrows(Async.FromContinuations(fun (_, econt, _) -> econt(new System.Exception("err"))))
        printfn "%A" r
        printf "FromContinuationsCancel "
        let r = SWCandContThrows(Async.FromContinuations(fun (_, _, ccont) -> ccont(new System.OperationCanceledException())))
        printfn "%A" r

    let FromContinuationsThrows() =
        printf "FromContinuationsThrows "
        let r = SWCandContThrows(Async.FromContinuations(fun (cont, _, _) -> failwith "zing"))
        printfn "%A" r

    let FromContinuationsSchedulesFuture() =
        printf "FromContinuationsSchedulesFutureSuccess "
        let r = SWCandContThrowsTimeout(Async.FromContinuations(fun (cont, _, _) -> 
                     ThreadPool.QueueUserWorkItem(fun _ -> Thread.Sleep(500); cont 10) |> ignore),
                     1000)
        Thread.Sleep(1500)
        printfn "%A" r
        printf "FromContinuationsSchedulesFutureError "
        let r = SWCandContThrowsTimeout(Async.FromContinuations(fun (_, econt, _) -> 
                     ThreadPool.QueueUserWorkItem(fun _ -> Thread.Sleep(500); econt(new System.Exception("err"))) |> ignore),
                     1000)
        Thread.Sleep(1500)
        printfn "%A" r
        printf "FromContinuationsSchedulesFutureCancel "
        let r = SWCandContThrowsTimeout(Async.FromContinuations(fun (_, _, ccont) -> 
                     ThreadPool.QueueUserWorkItem(fun _ -> Thread.Sleep(500); ccont(new System.OperationCanceledException())) |> ignore),
                     1000)
        Thread.Sleep(1500)
        printfn "%A" r

    let FromContinuationsSchedulesFutureAndThrowsSlowly() =
        printf "FromContinuationsSchedulesFutureSuccessAndThrowsSlowly "
        let r = SWCandContThrowsTimeout(Async.FromContinuations(fun (cont, _, _) -> 
                     ThreadPool.QueueUserWorkItem(fun _ -> cont 10) |> ignore
                     Thread.Sleep(500)
                     failwith "pow"),
                     1000)
        Thread.Sleep(1500)
        printfn "%A" r
        printf "FromContinuationsSchedulesFutureErrorAndThrowsSlowly "
        let r = SWCandContThrowsTimeout(Async.FromContinuations(fun (_, econt, _) -> 
                     ThreadPool.QueueUserWorkItem(fun _ -> econt(new System.Exception("err"))) |> ignore
                     Thread.Sleep(500)
                     failwith "pow"),
                     1000)
        Thread.Sleep(1500)
        printfn "%A" r
        printf "FromContinuationsSchedulesFutureCancelAndThrowsSlowly "
        let r = SWCandContThrowsTimeout(Async.FromContinuations(fun (_, _, ccont) -> 
                     ThreadPool.QueueUserWorkItem(fun _ -> ccont(new System.OperationCanceledException())) |> ignore
                     Thread.Sleep(500)
                     failwith "pow"),
                     1000)
        Thread.Sleep(1500)
        printfn "%A" r

    let FromContinuationsSchedulesFutureAndThrowsQuickly() =
        printf "FromContinuationsSchedulesFutureSuccessAndThrowsQuickly "
        let r = SWCandContThrowsTimeout(Async.FromContinuations(fun (cont, _, _) -> 
                     ThreadPool.QueueUserWorkItem(fun _ -> Thread.Sleep(500); cont 10) |> ignore
                     failwith "pow"),
                     1000)
        Thread.Sleep(1500)
        printfn "%A" r
        printf "FromContinuationsSchedulesFutureErrorAndThrowsQuickly "
        let r = SWCandContThrowsTimeout(Async.FromContinuations(fun (_, econt, _) -> 
                     ThreadPool.QueueUserWorkItem(fun _ -> Thread.Sleep(500); econt(new System.Exception("err"))) |> ignore
                     failwith "pow"),
                     1000)
        Thread.Sleep(1500)
        printfn "%A" r
        printf "FromContinuationsSchedulesFutureCancelAndThrowsQuickly "
        let r = SWCandContThrowsTimeout(Async.FromContinuations(fun (_, _, ccont) -> 
                     ThreadPool.QueueUserWorkItem(fun _ -> Thread.Sleep(500); ccont(new System.OperationCanceledException())) |> ignore
                     failwith "pow"),
                     1000)
        Thread.Sleep(1500)
        printfn "%A" r

    let AwaitWaitHandleAlreadySignaled(n) =
        printf "AwaitWaitHandleAlreadySignaled%d " n
        let mre = new ManualResetEvent(true)
        let r = SWCandContThrows(Async.AwaitWaitHandle(mre,n))
        printfn "%A" r

    let Tests() =
        printfn ""
        EmptyParallel()
        NonEmptyParallel()
        ParallelSeqArgumentThrows()
        SleepReturn(1)
        SleepReturn(0)   // ensure we don't ever 'optimize' sleep(0) and change threading behavior
        Return()
        FromContinuations()
        FromContinuationsThrows()
        FromContinuationsSchedulesFuture()
        FromContinuationsSchedulesFutureAndThrowsQuickly()
        FromContinuationsSchedulesFutureAndThrowsSlowly()
        AwaitWaitHandleAlreadySignaled(0)
        AwaitWaitHandleAlreadySignaled(1)

    Tests()
    0


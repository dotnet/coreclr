// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// Various tests for the:
// Microsoft.FSharp.Core.Operators module

namespace SystematicUnitTests.FSharp_Core.Microsoft_FSharp_Core

open System
open SystematicUnitTests.LibraryTestFx
open NUnit.Framework
open Microsoft.FSharp.Core.Operators.Checked

[<TestFixture>]
type OperatorsModule1() =

    [<Test>]
    member this.Checkedbyte() =
        // int type   
        let intByte = Operators.Checked.byte 100
        Assert.AreEqual(intByte,(byte)100)
        
        // char type  
        let charByte = Operators.Checked.byte '0'
        Assert.AreEqual(charByte,(byte)48)
        
        // boundary value
        let boundByte = Operators.Checked.byte 255.0
        Assert.AreEqual(boundByte, (byte)255)
        
        // overflow exception
        try 
            let overflowByte = Operators.Checked.byte 256.0
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        
        
       

    [<Test>]
    member this.Checkedchar() =

        // number
        let numberChar = Operators.Checked.char 48 
        Assert.AreEqual(numberChar,'0')
        
        // letter
        let letterChar = Operators.Checked.char 65 
        Assert.AreEqual(letterChar,'A')
        
        // boundary value
        let boundchar = Operators.Checked.char 126
        Assert.AreEqual(boundchar, '~')
        
        // overflow exception
        try 
            let overflowchar = Operators.Checked.char (System.Int64.MaxValue+(int64)2)
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        
    [<Test>]
    member this.CheckedInt() =

        // char
        let charInt = Operators.Checked.int '0' 
        Assert.AreEqual(charInt,48)
        
        // float
        let floatInt = Operators.Checked.int 10.0 
        Assert.AreEqual(floatInt,10)
        
        
        // boundary value
        let boundInt = Operators.Checked.int 32767.0
        Assert.AreEqual(boundInt, (int)32767)
        
        // overflow exception
        try 
            let overflowint = Operators.Checked.int 2147483648.0
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
                  
        ()   
        
    [<Test>]
    member this.CheckedInt16() =

        // char
        let charInt16 = Operators.Checked.int16 '0' 
        Assert.AreEqual(charInt16,(int16)48)
        
        // float
        let floatInt16 = Operators.Checked.int16 10.0 
        Assert.AreEqual(floatInt16,(int16)10)
        
        // boundary value
        let boundInt16 = Operators.Checked.int16 32767.0
        Assert.AreEqual(boundInt16, (int16)32767)
        
        // overflow exception
        try 
            let overflowint16 = Operators.Checked.int16 32768.0
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        ()   
        
    [<Test>]
    member this.CheckedInt32() =

        // char
        let charInt32 = Operators.Checked.int32 '0' 
        Assert.AreEqual(charInt32,(int32)48)
        
        // float
        let floatInt32 = Operators.Checked.int32 10.0
        Assert.AreEqual(floatInt32,(int32)10)
        
        // boundary value
        let boundInt32 = Operators.Checked.int32 2147483647.0
        Assert.AreEqual(boundInt32, (int32)2147483647)
        
        // overflow exception
        try 
            let overflowint32 = Operators.Checked.int32 2147483648.0
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
          
        ()   
        
    [<Test>]
    member this.CheckedInt64() =

        // char
        let charInt64 = Operators.Checked.int64 '0' 
        Assert.AreEqual(charInt64,(int64)48)
        
        // float
        let floatInt64 = Operators.Checked.int64 10.0
        Assert.AreEqual(floatInt64,(int64)10)
        
        // boundary value
        let boundInt64 = Operators.Checked.int64 9223372036854775807I
        let a  = 9223372036854775807L
        Assert.AreEqual(boundInt64, 9223372036854775807L)
        
        // overflow exception
        try 
            let overflowint64 = Operators.Checked.int64 (System.Double.MaxValue+2.0)
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        ()
        
    [<Test>]
    member this.CheckedNativeint() =

        // char
        let charnativeint = Operators.Checked.nativeint '0' 
        Assert.AreEqual(charnativeint,(nativeint)48)
        
        // float
        let floatnativeint = Operators.Checked.nativeint 10.0
        Assert.AreEqual(floatnativeint,(nativeint)10)
        
        // boundary value
        let boundnativeint = Operators.Checked.nativeint 32767.0
        Assert.AreEqual(boundnativeint, (nativeint)32767)
        
        // overflow exception
        try 
            let overflownativeint = Operators.Checked.nativeint 2147483648.0
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        ()
        
    [<Test>]
    member this.Checkedsbyte() =

        // char
        let charsbyte = Operators.Checked.sbyte '0' 
        Assert.AreEqual(charsbyte,(sbyte)48)
        
        // float
        let floatsbyte = Operators.Checked.sbyte -10.0
        Assert.AreEqual(floatsbyte,(sbyte)(-10))
        
        // boundary value
        let boundsbyte = Operators.Checked.sbyte -127.0
        Assert.AreEqual(boundsbyte, (sbyte)(-127))
        
        // overflow exception
        try 
            let overflowsbyte = Operators.Checked.sbyte -256.0
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        
        ()
        
    [<Test>]
    member this.Checkeduint16() =

        // char
        let charuint16 = Operators.Checked.uint16 '0'
        Assert.AreEqual(charuint16,(uint16)48)
        
        // float
        let floatuint16 = Operators.Checked.uint16 10.0
        Assert.AreEqual(floatuint16,(uint16)(10))
        
        // boundary value
        let bounduint16 = Operators.Checked.uint16 65535.0
        Assert.AreEqual(bounduint16, (uint16)(65535))
        
        // overflow exception
        try 
            let overflowuint16 = Operators.Checked.uint16 65536.0
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        ()
        
    [<Test>]
    member this.Checkeduint32() =

        // char
        let charuint32 = Operators.Checked.uint32 '0'
        Assert.AreEqual(charuint32,(uint32)48)
        
        // float
        let floatuint32 = Operators.Checked.uint32 10.0
        Assert.AreEqual(floatuint32,(uint32)(10))
        
        // boundary value
        let bounduint32 = Operators.Checked.uint32 429496729.0
        Assert.AreEqual(bounduint32, (uint32)(429496729))
        
        
        // overflow exception
        try 
            let overflowuint32 = Operators.Checked.uint32 uint32.MaxValue+1u
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        ()
        
    [<Test>]
    member this.Checkeduint64() =

        // char
        let charuint64 = Operators.Checked.uint64 '0'
        Assert.AreEqual(charuint64,(uint64)48)
        
        // float
        let floatuint64 = Operators.Checked.uint64 10.0
        Assert.AreEqual(floatuint64,(uint64)(10))
        
        // boundary value
        let bounduint64 = Operators.Checked.uint64 429496729.0
        Assert.AreEqual(bounduint64, (uint64)(429496729))
        
        // overflow exception
        try 
            let overflowuint64 = Operators.Checked.uint64 System.UInt64.MaxValue+1UL
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        ()
        
    [<Test>]
    member this.Checkedunativeint() =

        // char
        let charunativeint = Operators.Checked.unativeint '0' 
        Assert.AreEqual(charunativeint,(unativeint)48)
        
        // float
        let floatunativeint = Operators.Checked.unativeint 10.0
        Assert.AreEqual(floatunativeint,(unativeint)10)
        
        // boundary value
        let boundunativeint = Operators.Checked.unativeint 65353.0
        Assert.AreEqual(boundunativeint, (unativeint)65353)
        
        // overflow exception
        try 
            let overflowuint64 = Operators.Checked.uint64 System.UInt64.MaxValue+1UL
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        ()
        
    [<Test>]
    member this.KeyValue() =
        
        
        let funcKeyValue x =
            match x with
            | Operators.KeyValue(a) -> a
        
        // string int
        let stringint = funcKeyValue ( new System.Collections.Generic.KeyValuePair<string,int>("string",1))
        Assert.AreEqual(stringint,("string",1))
        
        // float char
        let floatchar = funcKeyValue ( new System.Collections.Generic.KeyValuePair<float,char>(1.0,'a'))
        Assert.AreEqual(floatchar,(1.0,'a'))
        
        // null
        let nullresult = funcKeyValue ( new System.Collections.Generic.KeyValuePair<string,char>(null,' '))
        let (nullstring:string,blankchar:char) = nullresult
        
        CheckThrowsNullRefException(fun () -> nullstring.ToString() |> ignore)
        
        
        ()
        
    [<Test>]
    member this.OptimizedRangesGetArraySlice() =

        
        let param1 = Some(1)
        let param2 = Some(2)
            
        // int
        let intslice = Operators.OperatorIntrinsics.GetArraySlice [|1;2;3;4;5;6|] param1 param2
        Assert.AreEqual(intslice,[|2;3|])
        
        // string
        let stringslice = Operators.OperatorIntrinsics.GetArraySlice [|"1";"2";"3"|] param1 param2
        Assert.AreEqual(stringslice,[|"2";"3"|])
        
        // null
        let stringslice = Operators.OperatorIntrinsics.GetArraySlice [|null;null;null|] param1 param2
        Assert.AreEqual(stringslice,[|null;null|])
        
        ()
    
    [<Test>]
    member this.OptimizedRangesGetArraySlice2D() =

        
        let param1D1 = Some(0)
        let param1D2 = Some(1)
        let param2D1 = Some(0)
        let param2D2 = Some(1)
            
        // int
        let intArray2D = Array2D.init 2 3 (fun i j -> i*100+j)
        let intslice = Operators.OperatorIntrinsics.GetArraySlice2D intArray2D param1D1 param1D2 param2D1 param2D2
        
        Assert.AreEqual(intslice.[1,1],101)
         
        // string
        let stringArray2D = Array2D.init 2 3 (fun i j -> (i*100+j).ToString())
        let stringslice = Operators.OperatorIntrinsics.GetArraySlice2D stringArray2D param1D1 param1D2 param2D1 param2D2
        Assert.AreEqual(stringslice.[1,1],(101).ToString())
        
        // null
        let nullArray2D = Array2D.init 2 3 (fun i j -> null)
        let nullslice = Operators.OperatorIntrinsics.GetArraySlice2D nullArray2D param1D1 param1D2 param2D1 param2D2
        Assert.AreEqual(nullslice.[1,1],null)
        
        ()
    
    [<Test>]
    member this.OptimizedRangesGetStringSlice() =
        let param1 = Some(4)
        let param2 = Some(6)
            
        // string
        let stringslice = Operators.OperatorIntrinsics.GetStringSlice "abcdefg" param1 param2
        Assert.AreEqual(stringslice,"efg")
        
        // null
        CheckThrowsNullRefException(fun () -> Operators.OperatorIntrinsics.GetStringSlice null param1 param2 |> ignore)
        ()
    
        
    [<Test>]
    member this.OptimizedRangesSetArraySlice() =
        let param1 = Some(1)
        let param2 = Some(2)
            
        // int
        let intArray1 = [|1;2;3|]
        let intArray2 = [|4;5;6|]
        Operators.OperatorIntrinsics.SetArraySlice intArray1 param1 param2 intArray2
        Assert.AreEqual(intArray1,[|1;4;5|])
        
        // string
        let stringArray1 = [|"1";"2";"3"|]
        let stringArray2 = [|"4";"5";"6"|]
        Operators.OperatorIntrinsics.SetArraySlice stringArray1 param1 param2 stringArray2
        Assert.AreEqual(stringArray1,[|"1";"4";"5"|])
        
        // null
        let nullArray1 = [|null;null;null|]
        let nullArray2 = [|null;null;null|]
        Operators.OperatorIntrinsics.SetArraySlice nullArray1  param1 param2 nullArray2
        CheckThrowsNullRefException(fun () -> nullArray1.[0].ToString() |> ignore)
        ()
        
    [<Test>]
    member this.OptimizedRangesSetArraySlice2D() =
        let param1D1 = Some(0)
        let param1D2 = Some(1)
        let param2D1 = Some(0)
        let param2D2 = Some(1)
            
        // int
        let intArray1 = Array2D.init 2 3 (fun i j -> i*10+j)
        let intArray2 = Array2D.init 2 3 (fun i j -> i*100+j)
        Operators.OperatorIntrinsics.SetArraySlice2D intArray1 param1D1 param1D2 param2D1 param2D2 intArray2
        Assert.AreEqual(intArray1.[1,1],101)
        
        // string
        let stringArray2D1 = Array2D.init 2 3 (fun i j -> (i*10+j).ToString())
        let stringArray2D2 = Array2D.init 2 3 (fun i j -> (i*100+j).ToString())
        Operators.OperatorIntrinsics.SetArraySlice2D stringArray2D1 param1D1 param1D2 param2D1 param2D2 stringArray2D2
        Assert.AreEqual(stringArray2D1.[1,1],(101).ToString())
        
        // null
        let nullArray2D1 = Array2D.init 2 3 (fun i j -> null)
        let nullArray2D2 = Array2D.init 2 3 (fun i j -> null)
        Operators.OperatorIntrinsics.SetArraySlice2D nullArray2D1 param1D1 param1D2 param2D1 param2D2 nullArray2D2
        CheckThrowsNullRefException(fun () -> nullArray2D1.[0,0].ToString()  |> ignore)
        ()
        
    [<Test>]
    member this.OptimizedRangesSetArraySlice3D() =
        let intArray1 = Array3D.init 2 3 4 (fun i j k -> i*10+j)
        let intArray2 = Array3D.init 2 3 4 (fun i j k -> i*100+j)
        Operators.OperatorIntrinsics.SetArraySlice3D intArray1 (Some 0) (Some 1) (Some 0) (Some 1) (Some 0) (Some 1) intArray2
        Assert.AreEqual(intArray1.[1,1,1],101)
        ()

    [<Test>]
    member this.OptimizedRangesSetArraySlice4D() =
        let intArray1 = Array4D.init 2 3 4 5 (fun i j k l -> i*10+j)
        let intArray2 = Array4D.init 2 3 4 5 (fun i j k l -> i*100+j)
        Operators.OperatorIntrinsics.SetArraySlice4D intArray1 (Some 0) (Some 1) (Some 0) (Some 1) (Some 0) (Some 1) intArray2
        Assert.AreEqual(intArray1.[1,1,1,1],101)
        ()
        
    [<Test>]
    member this.Uncheckeddefaultof () =
        
        // int
        let intdefault = Operators.Unchecked.defaultof<int>
        Assert.AreEqual(intdefault, 0)
      
        // string
        let stringdefault = Operators.Unchecked.defaultof<string>
        CheckThrowsNullRefException(fun () -> stringdefault.ToString() |> ignore)
        
        // null
        let structdefault = Operators.Unchecked.defaultof<DateTime>
        Assert.AreEqual( structdefault.Day,1)
        
        ()
        
    [<Test>]
    member this.abs () =
        
        // int
        let intabs = Operators.abs (-7)
        Assert.AreEqual(intabs, 7)
      
        // float 
        let floatabs = Operators.abs (-100.0)
        Assert.AreEqual(floatabs, 100.0)
        
        // decimal
        let decimalabs = Operators.abs (-1000M)
        Assert.AreEqual(decimalabs, 1000M)
        
        ()
        
    [<Test>]
    member this.acos () =
        
        // min value
        let minacos = Operators.acos (0.0)
        Assert.AreEqual(minacos, 1.5707963267948966)
      
        // normal value
        let normalacos = Operators.acos (0.3)
        Assert.AreEqual(normalacos, 1.2661036727794992)
      
        // max value
        let maxacos = Operators.acos (1.0)
        Assert.AreEqual(maxacos, 0.0)
        ()
        
    [<Test>]
    member this.asin () =
        
        // min value
        let minasin = Operators.asin (0.0)
        Assert.AreEqual(minasin, 0)
      
        // normal value
        let normalasin = Operators.asin (0.5)
        Assert.AreEqual(normalasin, 0.52359877559829893)
      
        // max value
        let maxasin = Operators.asin (1.0)
        Assert.AreEqual(maxasin, 1.5707963267948966)
        ()
        
   
        
    [<Test>]
    member this.atan () =
        
        // min value
        let minatan = Operators.atan (0.0)
        Assert.AreEqual(minatan, 0)
      
        // normal value
        let normalatan = Operators.atan (1.0)
        Assert.AreEqual(normalatan, 0.78539816339744828)
      
        // biggish  value
        let maxatan = Operators.atan (infinity)
        Assert.AreEqual(maxatan, 1.5707963267948966)
        ()
       
    [<Test>]
    member this.atan2 () =
        
        // min value
        let minatan2 = Operators.atan2 (0.0) (1.0)
        Assert.AreEqual(minatan2, 0)
      
        // normal value
        let normalatan2 = Operators.atan2 (1.0) (1.0)
        Assert.AreEqual(normalatan2, 0.78539816339744828)
      
        // biggish  value
        let maxatan2 = Operators.atan2 (1.0) (0.0)
        Assert.AreEqual(maxatan2, 1.5707963267948966)
        ()
        
    [<Test>]
    member this.box () =
        
        // int value
        let intbox = Operators.box 1
        Assert.AreEqual(intbox, 1)
      
        // string value
        let stringlbox = Operators.box "string"
        Assert.AreEqual(stringlbox, "string")
      
        // null  value
        let nullbox = Operators.box null
        CheckThrowsNullRefException(fun () -> nullbox.ToString()  |> ignore)
        ()
        
    [<Test>]
    member this.byte() =
        // int type   
        let intByte = Operators.byte 100
        Assert.AreEqual(intByte,(byte)100)
        
        // char type  
        let charByte = Operators.byte '0'
        Assert.AreEqual(charByte,(byte)48)
        
        // boundary value
        let boundByte = Operators.byte 255.0
        Assert.AreEqual(boundByte, (byte)255)
        
        // overflow exception
        try 
            let overflowbyte = Operators.byte (System.Int64.MaxValue*(int64)2)
            Assert.Fail("Expectt overflow exception but not.")
        with
            | :? System.OverflowException -> ()
            | _ -> Assert.Fail("Expectt overflow exception but not.")
        
    [<Test>]
    member this.ceil() =
        // min value   
        let minceil = Operators.ceil 0.1
        Assert.AreEqual(minceil,1.0)
        
        // normal value  
        let normalceil = Operators.ceil 100.0
        Assert.AreEqual(normalceil,100.0)
        
        // max value
        let maxceil = Operators.ceil 1.7E+308
        Assert.AreEqual(maxceil, 1.7E+308)
        
    [<Test>]
    member this.char() =
        // int type   
        let intchar = Operators.char 48
        Assert.AreEqual(intchar,'0')
        
        // string type  
        let stringchar = Operators.char " "
        Assert.AreEqual(stringchar, ' ')
       
    [<Test>]
    member this.compare() =
        // int type   
        let intcompare = Operators.compare 100 101
        Assert.AreEqual(intcompare,-1)
        
        // char type  
        let charcompare = Operators.compare '0' '1'
        Assert.AreEqual(charcompare,-1)
        
        // null value
        let boundcompare = Operators.compare null null
        Assert.AreEqual(boundcompare, 0)
   
        
    [<Test>]
    member this.cos () =
        
        // min value
        let mincos = Operators.cos (0.0)
        Assert.AreEqual(mincos, 1)
      
        // normal value
        let normalcos = Operators.cos (1.0)
        Assert.AreEqual(normalcos, 0.54030230586813977)
        
        // biggish  value
        let maxcos = Operators.cos (1.57)
        Assert.AreEqual(maxcos, 0.00079632671073326335)
        ()
        
    [<Test>]
    member this.cosh () =
        
        // min value
        let mincosh = Operators.cosh (0.0)
        Assert.AreEqual(mincosh, 1.0)
      
        // normal value
        let normalcosh = Operators.cosh (1.0)
        Assert.AreEqual(normalcosh, 1.5430806348152437)
        
        // biggish  value
        let maxcosh = Operators.cosh (1.57)
        Assert.AreEqual(maxcosh, 2.5073466880660993)
        
        
        ()
        
    
        
    [<Test>]
    member this.decimal () =
        
        // int value
        let mindecimal = Operators.decimal (1)
        Assert.AreEqual(mindecimal, 1)
       
        // float  value
        let maxdecimal = Operators.decimal (1.0)
        Assert.AreEqual(maxdecimal, 1)
        ()
        
    [<Test>]
    member this.decr() =
        // zero   
        let zeroref = ref 0
        Operators.decr zeroref
        Assert.AreEqual(zeroref,(ref -1))
        
        //  big number
        let bigref = ref 32767
        Operators.decr bigref
        Assert.AreEqual(bigref,(ref 32766))
        
        // normal value
        let normalref = ref 100
        Operators.decr (normalref)
        Assert.AreEqual(normalref,(ref 99))
        
    [<Test>]
    member this.defaultArg() =
        // zero   
        let zeroOption = Some(0)
        let intdefaultArg = Operators.defaultArg zeroOption 2
        Assert.AreEqual(intdefaultArg,0)
        
        //  big number
        let bigOption = Some(32767)
        let bigdefaultArg = Operators.defaultArg bigOption 32766
        Assert.AreEqual(bigdefaultArg,32767)
        
        // normal value
        let normalOption = Some(100)
        let normalfaultArg = Operators.defaultArg normalOption 100
        Assert.AreEqual(normalfaultArg, 100)
        
    [<Test>]
    member this.double() =
        // int type   
        let intdouble = Operators.double 100
        Assert.AreEqual(intdouble,100.0)
        
        // char type  
        let chardouble = Operators.double '0'
        Assert.AreEqual(chardouble,48)
        ()
       
    [<Test>]
    member this.enum() =
        // zero   
        let intarg : int32 = 0
        let intenum = Operators.enum<System.ConsoleColor> intarg
        Assert.AreEqual(intenum,System.ConsoleColor.Black)
        
        //  big number
        let bigarg : int32 = 15
        let charenum = Operators.enum<System.ConsoleColor> bigarg
        Assert.AreEqual(charenum,System.ConsoleColor.White)
        
        // normal value
        let normalarg : int32 = 9
        let boundenum = Operators.enum<System.ConsoleColor> normalarg
        Assert.AreEqual(boundenum, System.ConsoleColor.Blue)
        
#if IGNORED_TESTS
Ignore(    [<Test;Ignore("See FSB #3826 ? Need way to validate Operators.exit function.")>]
    member this.exit() =
        // zero  
        try 
            let intexit = Operators.exit 1
            ()
        with
            | _ -> ()
        //Assert.AreEqual(intexit,-1)
        
        //  big number
        let charexit = Operators.exit 32767
        //Assert.AreEqual(charexit,-1)
        
        // normal value
        let boundexit = Operators.exit 100
        Assert.AreEqual(boundexit, 0)
#endif

    [<Test>]
    member this.exp() =
        // zero   
        let zeroexp = Operators.exp 0.0
        Assert.AreEqual(zeroexp,1.0)
        
        //  big number
        let bigexp = Operators.exp 32767.0
        Assert.AreEqual(bigexp,infinity)
        
        // normal value
        let normalexp = Operators.exp 100.0
        Assert.AreEqual(normalexp, 2.6881171418161356E+43)
        
    [<Test>]
    member this.failwith() =
        try 
            let _ = Operators.failwith "failwith"
            Assert.Fail("Expect fail but not.")
            ()
        with
            | Failure("failwith") -> ()
            |_ -> Assert.Fail("Throw unexpected exception")
        
        
    [<Test>]
    member this.float() =
        // int type   
        let intfloat = Operators.float 100
        Assert.AreEqual(intfloat,(float)100)
        
        // char type  
        let charfloat = Operators.float '0'
        Assert.AreEqual(charfloat,(float)48)
      
        ()
       
        
    [<Test>]
    member this.float32() =
        // int type   
        let intfloat32 = Operators.float32 100
        Assert.AreEqual(intfloat32,(float32)100)
        
        // char type  
        let charfloat32 = Operators.float32 '0'
        Assert.AreEqual(charfloat32,(float32)48)
     
        ()
       
        
    [<Test>]
    member this.floor() =
        // float type   
        let intfloor = Operators.floor 100.0
        Assert.AreEqual(intfloor,100)
        
        // float32 type  
        let charfloor = Operators.floor ((float32)100.0)
        Assert.AreEqual(charfloor,100)
    
    [<Test>]
    member this.fst() =
        // int type   
        let intfst = Operators.fst (100,101)
        Assert.AreEqual(intfst,100)
        
        // char type  
        let charfst = Operators.fst ('0','1')
        Assert.AreEqual(charfst,'0')
        
        // null value
        let boundfst = Operators.fst (null,null)
        Assert.AreEqual(boundfst, null)
        
    [<Test>]
    member this.hash() =
        // int type   
        let inthash = Operators.hash 100
        Assert.AreEqual(inthash,100)
        
        // char type  
        let charhash = Operators.hash '0'
        Assert.AreEqual(charhash,3145776)
        
        // string value
        let boundhash = Operators.hash "A"
        Assert.AreEqual(boundhash, -842352673)
        
    [<Test>]
    member this.id() =
        // int type   
        let intid = Operators.id 100
        Assert.AreEqual(intid,100)
        
        // char type  
        let charid = Operators.id '0'
        Assert.AreEqual(charid,'0')
        
        // string value
        let boundid = Operators.id "A"
        Assert.AreEqual(boundid, "A")
        
        
    [<Test>]
    member this.ignore() =         
        // value type 
        let result = Operators.ignore 10
        Assert.AreEqual(result,null)
        
        // reference type
        let result = Operators.ignore "A"
        Assert.AreEqual(result,null) 
        
        ()

#if IGNORED_TESTS
    [<Test; Ignore( "[FSharp Bugs 1.0] #3842 - OverflowException does not pop up on Operators.int int16 int 32 int64 ")>]
    member this.incr() =         
        // legit value 
        let result = ref 10
        Operators.incr result
        Assert.AreEqual(!result,11)
        
        // overflow
        let result = ref (Operators.Checked.int System.Int32.MaxValue)
        CheckThrowsOverflowException(fun() -> Operators.incr result |> ignore)
        
        ()
#endif

    [<Test>]
    member this.infinity() =         
        
        let inf = Operators.infinity
        let result = inf > System.Double.MaxValue
        Assert.IsTrue(result)
        
        // arithmetic operation
        let result = infinity + 3.0
        Assert.AreEqual(result,infinity)
        let result = infinity - 3.0
        Assert.AreEqual(result,infinity)
        let result = infinity * 3.0
        Assert.AreEqual(result,infinity)
        let result = infinity / 3.0
        Assert.AreEqual(result,infinity)
        let result = infinity / 3.0
        Assert.AreEqual(result,infinity)
        
        
        ()
        
    [<Test>]
    member this.infinityf() =         
        
        let inf = Operators.infinityf
        let result = inf > System.Single.MaxValue
        Assert.IsTrue(result)
        
        // arithmetic operation
        let result = infinityf + 3.0f
        Assert.AreEqual(result,infinity)
        let result = infinityf - 3.0f
        Assert.AreEqual(result,infinity)
        let result = infinityf * 3.0f
        Assert.AreEqual(result,infinity)
        let result = infinityf / 3.0f
        Assert.AreEqual(result,infinity)
        let result = infinityf / 3.0f
        Assert.AreEqual(result,infinityf)
        
        ()
        
    
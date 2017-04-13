
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.5.22220.0



// Metadata version: v4.0.30319
.assembly extern System.Private.CoreLib
{
  .publickeytoken = (7C EC 85 D7 BE A7 79 8E )                         // |.....y.
  .ver 4:0:0:0
}
.assembly Program
{
  .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                                           63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [System.Private.CoreLib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [System.Private.CoreLib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 07 01 00 00 00 00 ) 

  .custom instance void class ConsoleApplication21.SingleAttribute`1<int32>::.ctor() = ( 01 00 00 00 ) 
  .custom instance void class ConsoleApplication21.SingleAttribute`1<bool>::.ctor() = ( 01 00 00 00 ) 
  .custom instance void class ConsoleApplication21.MultiAttribute`1<int32>::.ctor() = ( 01 00 00 00 ) 
  .custom instance void class ConsoleApplication21.MultiAttribute`1<int32>::.ctor(!0) = ( 01 00 01 00 00 00 00 00 ) 
  .custom instance void class ConsoleApplication21.MultiAttribute`1<int32>::.ctor() = ( 01 00 01 00 54 08 05 56 61 6C 75 65 02 00 00 00 ) // ....T..Value....
  .custom instance void class ConsoleApplication21.MultiAttribute`1<bool>::.ctor() = ( 01 00 00 00 ) 
  .custom instance void class ConsoleApplication21.MultiAttribute`1<bool>::.ctor(!0) = ( 01 00 01 00 00 ) 
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module Program.exe
// MVID: {3E5F1449-4307-4694-8DAD-AA515DE368D9}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x00000237F8750000


// =============== CLASS MEMBERS DECLARATION ===================

.class private auto ansi beforefieldinit ConsoleApplication21.SingleAttribute`1<T>
       extends [System.Private.CoreLib]System.Attribute
{
  .custom instance void [System.Private.CoreLib]System.AttributeUsageAttribute::.ctor(valuetype [System.Private.CoreLib]System.AttributeTargets) = ( 01 00 85 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                                                     4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [System.Private.CoreLib]System.Attribute::.ctor()
    IL_0006:  nop
    IL_0007:  ret
  } // end of method SingleAttribute`1::.ctor

} // end of class ConsoleApplication21.SingleAttribute`1

.class private auto ansi beforefieldinit ConsoleApplication21.MultiAttribute`1<T>
       extends [System.Private.CoreLib]System.Attribute
{
  .custom instance void [System.Private.CoreLib]System.AttributeUsageAttribute::.ctor(valuetype [System.Private.CoreLib]System.AttributeTargets) = ( 01 00 85 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                                                     4D 75 6C 74 69 70 6C 65 01 )                      // Multiple.
  .field private !T '<Value>k__BackingField'
  .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [System.Private.CoreLib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [System.Private.CoreLib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 ) 
  .method public hidebysig specialname instance !T 
          get_Value() cil managed
  {
    .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      !0 class ConsoleApplication21.MultiAttribute`1<!T>::'<Value>k__BackingField'
    IL_0006:  ret
  } // end of method MultiAttribute`1::get_Value

  .method public hidebysig specialname instance void 
          set_Value(!T 'value') cil managed
  {
    .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      !0 class ConsoleApplication21.MultiAttribute`1<!T>::'<Value>k__BackingField'
    IL_0007:  ret
  } // end of method MultiAttribute`1::set_Value

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [System.Private.CoreLib]System.Attribute::.ctor()
    IL_0006:  nop
    IL_0007:  nop
    IL_0008:  ret
  } // end of method MultiAttribute`1::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(!T 'value') cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [System.Private.CoreLib]System.Attribute::.ctor()
    IL_0006:  nop
    IL_0007:  nop
    IL_0008:  ldarg.0
    IL_0009:  ldarg.1
    IL_000a:  call       instance void class ConsoleApplication21.MultiAttribute`1<!T>::set_Value(!0)
    IL_000f:  nop
    IL_0010:  ret
  } // end of method MultiAttribute`1::.ctor

  .property instance !T Value()
  {
    .get instance !T ConsoleApplication21.MultiAttribute`1::get_Value()
    .set instance void ConsoleApplication21.MultiAttribute`1::set_Value(!T)
  } // end of property MultiAttribute`1::Value
} // end of class ConsoleApplication21.MultiAttribute`1

.class private auto ansi beforefieldinit ConsoleApplication21.Program
       extends [System.Private.CoreLib]System.Object
{
  .custom instance void class ConsoleApplication21.SingleAttribute`1<int32>::.ctor() = ( 01 00 00 00 ) 
  .custom instance void class ConsoleApplication21.SingleAttribute`1<bool>::.ctor() = ( 01 00 00 00 ) 
  .custom instance void class ConsoleApplication21.MultiAttribute`1<int32>::.ctor() = ( 01 00 00 00 ) 
  .custom instance void class ConsoleApplication21.MultiAttribute`1<int32>::.ctor(!0) = ( 01 00 01 00 00 00 00 00 ) 
  .custom instance void class ConsoleApplication21.MultiAttribute`1<int32>::.ctor() = ( 01 00 01 00 54 08 05 56 61 6C 75 65 02 00 00 00 ) // ....T..Value....
  .custom instance void class ConsoleApplication21.MultiAttribute`1<bool>::.ctor() = ( 01 00 00 00 ) 
  .custom instance void class ConsoleApplication21.MultiAttribute`1<bool>::.ctor(!0) = ( 01 00 01 00 00 ) 
  .custom instance void class ConsoleApplication21.MultiAttribute`1<bool>::.ctor() = ( 01 00 01 00 54 02 05 56 61 6C 75 65 01 )          // ....T..Value.
  .custom instance void class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>::.ctor() = ( 01 00 00 00 ) 
  .class auto ansi nested private beforefieldinit Derive
         extends ConsoleApplication21.Program
  {
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       8 (0x8)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void ConsoleApplication21.Program::.ctor()
      IL_0006:  nop
      IL_0007:  ret
    } // end of method Derive::.ctor

  } // end of class Derive

  .class auto ansi serializable sealed nested private beforefieldinit '<>c'
         extends [System.Private.CoreLib]System.Object
  {
    .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public static initonly class ConsoleApplication21.Program/'<>c' '<>9'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_0'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_1'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_2'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_3'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_4'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_5'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_6'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_7'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_8'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_9'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_10'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_11'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_12'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_13'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_14'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_15'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_16'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_17'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_18'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_19'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_20'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_21'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_22'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_23'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_24'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_25'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_26'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_27'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_28'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_29'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_30'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_31'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_32'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_33'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_34'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_35'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_36'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_37'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_38'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_39'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_40'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_41'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_42'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_43'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_44'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_45'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_46'
    .field public static class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> '<>9__5_47'
    .method private hidebysig specialname rtspecialname static 
            void  .cctor() cil managed
    {
      // Code size       11 (0xb)
      .maxstack  8
      IL_0000:  newobj     instance void ConsoleApplication21.Program/'<>c'::.ctor()
      IL_0005:  stsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
      IL_000a:  ret
    } // end of method '<>c'::.cctor

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       8 (0x8)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [System.Private.CoreLib]System.Object::.ctor()
      IL_0006:  nop
      IL_0007:  ret
    } // end of method '<>c'::.ctor

    .method assembly hidebysig instance bool 
            '<Main>b__5_0'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<int32>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_0'

    .method assembly hidebysig instance bool 
            '<Main>b__5_1'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<bool>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_1'

    .method assembly hidebysig instance bool 
            '<Main>b__5_2'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_2'

    .method assembly hidebysig instance bool 
            '<Main>b__5_3'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.1
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_3'

    .method assembly hidebysig instance bool 
            '<Main>b__5_4'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.2
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_4'

    .method assembly hidebysig instance bool 
            '<Main>b__5_5'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_5'

    .method assembly hidebysig instance bool 
            '<Main>b__5_6'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ret
    } // end of method '<>c'::'<Main>b__5_6'

    .method assembly hidebysig instance bool 
            '<Main>b__5_7'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ret
    } // end of method '<>c'::'<Main>b__5_7'

    .method assembly hidebysig instance bool 
            '<Main>b__5_8'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       23 (0x17)
      .maxstack  2
      .locals init (valuetype [System.Private.CoreLib]System.Nullable`1<bool> V_0)
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>::get_Value()
      IL_000b:  stloc.0
      IL_000c:  ldloca.s   V_0
      IL_000e:  call       instance bool valuetype [System.Private.CoreLib]System.Nullable`1<bool>::get_HasValue()
      IL_0013:  ldc.i4.0
      IL_0014:  ceq
      IL_0016:  ret
    } // end of method '<>c'::'<Main>b__5_8'

    .method assembly hidebysig instance bool 
            '<Main>b__5_9'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<int32>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_9'

    .method assembly hidebysig instance bool 
            '<Main>b__5_10'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<bool>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_10'

    .method assembly hidebysig instance bool 
            '<Main>b__5_11'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_11'

    .method assembly hidebysig instance bool 
            '<Main>b__5_12'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.1
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_12'

    .method assembly hidebysig instance bool 
            '<Main>b__5_13'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.2
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_13'

    .method assembly hidebysig instance bool 
            '<Main>b__5_14'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_14'

    .method assembly hidebysig instance bool 
            '<Main>b__5_15'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ret
    } // end of method '<>c'::'<Main>b__5_15'

    .method assembly hidebysig instance bool 
            '<Main>b__5_16'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ret
    } // end of method '<>c'::'<Main>b__5_16'

    .method assembly hidebysig instance bool 
            '<Main>b__5_17'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       23 (0x17)
      .maxstack  2
      .locals init (valuetype [System.Private.CoreLib]System.Nullable`1<bool> V_0)
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>::get_Value()
      IL_000b:  stloc.0
      IL_000c:  ldloca.s   V_0
      IL_000e:  call       instance bool valuetype [System.Private.CoreLib]System.Nullable`1<bool>::get_HasValue()
      IL_0013:  ldc.i4.0
      IL_0014:  ceq
      IL_0016:  ret
    } // end of method '<>c'::'<Main>b__5_17'

    .method assembly hidebysig instance bool 
            '<Main>b__5_18'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<int32>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_18'

    .method assembly hidebysig instance bool 
            '<Main>b__5_19'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<bool>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_19'

    .method assembly hidebysig instance bool 
            '<Main>b__5_20'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_20'

    .method assembly hidebysig instance bool 
            '<Main>b__5_21'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.1
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_21'

    .method assembly hidebysig instance bool 
            '<Main>b__5_22'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.2
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_22'

    .method assembly hidebysig instance bool 
            '<Main>b__5_23'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_23'

    .method assembly hidebysig instance bool 
            '<Main>b__5_24'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ret
    } // end of method '<>c'::'<Main>b__5_24'

    .method assembly hidebysig instance bool 
            '<Main>b__5_25'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<int32>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_25'

    .method assembly hidebysig instance bool 
            '<Main>b__5_26'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<bool>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_26'

    .method assembly hidebysig instance bool 
            '<Main>b__5_27'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_27'

    .method assembly hidebysig instance bool 
            '<Main>b__5_28'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.1
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_28'

    .method assembly hidebysig instance bool 
            '<Main>b__5_29'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.2
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_29'

    .method assembly hidebysig instance bool 
            '<Main>b__5_30'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_30'

    .method assembly hidebysig instance bool 
            '<Main>b__5_31'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ret
    } // end of method '<>c'::'<Main>b__5_31'

    .method assembly hidebysig instance bool 
            '<Main>b__5_32'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<int32>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_32'

    .method assembly hidebysig instance bool 
            '<Main>b__5_33'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<int32>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_33'

    .method assembly hidebysig instance bool 
            '<Main>b__5_34'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<bool>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_34'

    .method assembly hidebysig instance bool 
            '<Main>b__5_35'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.SingleAttribute`1<bool>
      IL_0006:  ldnull
      IL_0007:  cgt.un
      IL_0009:  ret
    } // end of method '<>c'::'<Main>b__5_35'

    .method assembly hidebysig instance bool 
            '<Main>b__5_36'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_36'

    .method assembly hidebysig instance bool 
            '<Main>b__5_37'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.1
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_37'

    .method assembly hidebysig instance bool 
            '<Main>b__5_38'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.2
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_38'

    .method assembly hidebysig instance bool 
            '<Main>b__5_39'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_39'

    .method assembly hidebysig instance bool 
            '<Main>b__5_40'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.1
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_40'

    .method assembly hidebysig instance bool 
            '<Main>b__5_41'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<int32>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<int32>::get_Value()
      IL_000b:  ldc.i4.2
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_41'

    .method assembly hidebysig instance bool 
            '<Main>b__5_42'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_42'

    .method assembly hidebysig instance bool 
            '<Main>b__5_43'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ret
    } // end of method '<>c'::'<Main>b__5_43'

    .method assembly hidebysig instance bool 
            '<Main>b__5_44'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       15 (0xf)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ldc.i4.0
      IL_000c:  ceq
      IL_000e:  ret
    } // end of method '<>c'::'<Main>b__5_44'

    .method assembly hidebysig instance bool 
            '<Main>b__5_45'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<bool>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<bool>::get_Value()
      IL_000b:  ret
    } // end of method '<>c'::'<Main>b__5_45'

    .method assembly hidebysig instance bool 
            '<Main>b__5_46'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       23 (0x17)
      .maxstack  2
      .locals init (valuetype [System.Private.CoreLib]System.Nullable`1<bool> V_0)
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>::get_Value()
      IL_000b:  stloc.0
      IL_000c:  ldloca.s   V_0
      IL_000e:  call       instance bool valuetype [System.Private.CoreLib]System.Nullable`1<bool>::get_HasValue()
      IL_0013:  ldc.i4.0
      IL_0014:  ceq
      IL_0016:  ret
    } // end of method '<>c'::'<Main>b__5_46'

    .method assembly hidebysig instance bool 
            '<Main>b__5_47'(class [System.Private.CoreLib]System.Attribute a) cil managed
    {
      // Code size       23 (0x17)
      .maxstack  2
      .locals init (valuetype [System.Private.CoreLib]System.Nullable`1<bool> V_0)
      IL_0000:  ldarg.1
      IL_0001:  isinst     class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>
      IL_0006:  callvirt   instance !0 class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>::get_Value()
      IL_000b:  stloc.0
      IL_000c:  ldloca.s   V_0
      IL_000e:  call       instance bool valuetype [System.Private.CoreLib]System.Nullable`1<bool>::get_HasValue()
      IL_0013:  ldc.i4.0
      IL_0014:  ceq
      IL_0016:  ret
    } // end of method '<>c'::'<Main>b__5_47'

  } // end of class '<>c'

  .field private int32 '<Property>k__BackingField'
  .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .custom instance void [System.Private.CoreLib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [System.Private.CoreLib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 ) 
  .method public hidebysig specialname instance int32 
          get_Property() cil managed
  {
    .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldfld      int32 ConsoleApplication21.Program::'<Property>k__BackingField'
    IL_0006:  ret
  } // end of method Program::get_Property

  .method public hidebysig specialname instance void 
          set_Property(int32 'value') cil managed
  {
    .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      int32 ConsoleApplication21.Program::'<Property>k__BackingField'
    IL_0007:  ret
  } // end of method Program::set_Property

  .method private hidebysig static void  Main(string[] args) cil managed
  {
    .entrypoint
    // Code size       3414 (0xd56)
    .maxstack  3
    .locals init (class [System.Private.CoreLib]System.Reflection.Assembly V_0,
             class [System.Private.CoreLib]System.Reflection.TypeInfo V_1,
             class [System.Private.CoreLib]System.Reflection.PropertyInfo V_2,
             class [System.Private.CoreLib]System.Reflection.TypeInfo V_3,
             class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<class [System.Private.CoreLib]System.Attribute> V_4,
             class [System.Private.CoreLib]System.Collections.IEnumerator V_5,
             class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<class [System.Private.CoreLib]System.Attribute> V_6,
             class [System.Private.CoreLib]System.Collections.IEnumerator V_7,
             class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<class [System.Private.CoreLib]System.Attribute> V_8,
             class [System.Private.CoreLib]System.Collections.IEnumerator V_9,
             class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<class ConsoleApplication21.SingleAttribute`1<int32>> V_10,
             class [System.Private.CoreLib]System.Collections.IEnumerator V_11,
             class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<class ConsoleApplication21.SingleAttribute`1<bool>> V_12,
             class [System.Private.CoreLib]System.Collections.IEnumerator V_13,
             class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<class ConsoleApplication21.MultiAttribute`1<int32>> V_14,
             class [System.Private.CoreLib]System.Collections.IEnumerator V_15,
             class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<class ConsoleApplication21.MultiAttribute`1<bool>> V_16,
             class [System.Private.CoreLib]System.Collections.IEnumerator V_17,
             class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>> V_18,
             class [System.Private.CoreLib]System.Collections.IEnumerator V_19)
    IL_0000:  nop
    IL_0001:  ldtoken    ConsoleApplication21.Program
    IL_0006:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_000b:  call       class [System.Private.CoreLib]System.Reflection.TypeInfo [System.Private.CoreLib]System.Reflection.IntrospectionExtensions::GetTypeInfo(class [System.Private.CoreLib]System.Type)
    IL_0010:  callvirt   instance class [System.Private.CoreLib]System.Reflection.Assembly [System.Private.CoreLib]System.Type::get_Assembly()
    IL_0015:  stloc.0
    IL_0016:  ldloc.0
    IL_0017:  call       !!0 [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttribute<class ConsoleApplication21.SingleAttribute`1<int32>>(class [System.Private.CoreLib]System.Reflection.Assembly)
    IL_001c:  ldnull
    IL_001d:  cgt.un
    IL_001f:  ldc.i4.s   66
    IL_0021:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0026:  nop
    IL_0027:  ldloc.0
    IL_0028:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_002d:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0032:  ldc.i4.1
    IL_0033:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_0038:  ldnull
    IL_0039:  cgt.un
    IL_003b:  ldc.i4.s   67
    IL_003d:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0042:  nop
    IL_0043:  ldloc.0
    IL_0044:  call       !!0 [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttribute<class ConsoleApplication21.SingleAttribute`1<bool>>(class [System.Private.CoreLib]System.Reflection.Assembly)
    IL_0049:  ldnull
    IL_004a:  cgt.un
    IL_004c:  ldc.i4.s   68
    IL_004e:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0053:  nop
    IL_0054:  ldloc.0
    IL_0055:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_005a:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_005f:  ldc.i4.1
    IL_0060:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_0065:  ldnull
    IL_0066:  cgt.un
    IL_0068:  ldc.i4.s   69
    IL_006a:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_006f:  nop
    IL_0070:  ldloc.0
    IL_0071:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_0076:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_007b:  call       bool [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::IsDefined(class [System.Private.CoreLib]System.Reflection.Assembly,
                                                                                                             class [System.Private.CoreLib]System.Type)
    IL_0080:  ldc.i4.s   70
    IL_0082:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0087:  nop
    IL_0088:  ldloc.0
    IL_0089:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_008e:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0093:  ldc.i4.1
    IL_0094:  callvirt   instance bool [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::IsDefined(class [System.Private.CoreLib]System.Type,
                                                                                                                     bool)
    IL_0099:  ldc.i4.s   71
    IL_009b:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_00a0:  nop
    IL_00a1:  ldloc.0
    IL_00a2:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_00a7:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_00ac:  call       bool [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::IsDefined(class [System.Private.CoreLib]System.Reflection.Assembly,
                                                                                                             class [System.Private.CoreLib]System.Type)
    IL_00b1:  ldc.i4.s   72
    IL_00b3:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_00b8:  nop
    IL_00b9:  ldloc.0
    IL_00ba:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_00bf:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_00c4:  ldc.i4.1
    IL_00c5:  callvirt   instance bool [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::IsDefined(class [System.Private.CoreLib]System.Type,
                                                                                                                     bool)
    IL_00ca:  ldc.i4.s   73
    IL_00cc:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_00d1:  nop
    IL_00d2:  ldtoken    ConsoleApplication21.Program
    IL_00d7:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_00dc:  call       class [System.Private.CoreLib]System.Reflection.TypeInfo [System.Private.CoreLib]System.Reflection.IntrospectionExtensions::GetTypeInfo(class [System.Private.CoreLib]System.Type)
    IL_00e1:  stloc.1
    IL_00e2:  ldloc.1
    IL_00e3:  call       !!0 [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttribute<class ConsoleApplication21.SingleAttribute`1<int32>>(class [System.Private.CoreLib]System.Reflection.MemberInfo)
    IL_00e8:  ldnull
    IL_00e9:  cgt.un
    IL_00eb:  ldc.i4.s   76
    IL_00ed:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_00f2:  nop
    IL_00f3:  ldloc.1
    IL_00f4:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_00f9:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_00fe:  ldc.i4.1
    IL_00ff:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_0104:  ldnull
    IL_0105:  cgt.un
    IL_0107:  ldc.i4.s   77
    IL_0109:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_010e:  nop
    IL_010f:  ldloc.1
    IL_0110:  call       !!0 [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttribute<class ConsoleApplication21.SingleAttribute`1<bool>>(class [System.Private.CoreLib]System.Reflection.MemberInfo)
    IL_0115:  ldnull
    IL_0116:  cgt.un
    IL_0118:  ldc.i4.s   78
    IL_011a:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_011f:  nop
    IL_0120:  ldloc.1
    IL_0121:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_0126:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_012b:  ldc.i4.1
    IL_012c:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_0131:  ldnull
    IL_0132:  cgt.un
    IL_0134:  ldc.i4.s   79
    IL_0136:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_013b:  nop
    IL_013c:  ldloc.1
    IL_013d:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_0142:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0147:  call       bool [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::IsDefined(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                             class [System.Private.CoreLib]System.Type)
    IL_014c:  ldc.i4.s   80
    IL_014e:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0153:  nop
    IL_0154:  ldloc.1
    IL_0155:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_015a:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_015f:  ldc.i4.1
    IL_0160:  callvirt   instance bool [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::IsDefined(class [System.Private.CoreLib]System.Type,
                                                                                                                     bool)
    IL_0165:  ldc.i4.s   81
    IL_0167:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_016c:  nop
    IL_016d:  ldloc.1
    IL_016e:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_0173:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0178:  call       bool [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::IsDefined(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                             class [System.Private.CoreLib]System.Type)
    IL_017d:  ldc.i4.s   82
    IL_017f:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0184:  nop
    IL_0185:  ldloc.1
    IL_0186:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_018b:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0190:  ldc.i4.1
    IL_0191:  callvirt   instance bool [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::IsDefined(class [System.Private.CoreLib]System.Type,
                                                                                                                     bool)
    IL_0196:  ldc.i4.s   83
    IL_0198:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_019d:  nop
    IL_019e:  ldtoken    ConsoleApplication21.Program
    IL_01a3:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_01a8:  call       class [System.Private.CoreLib]System.Reflection.TypeInfo [System.Private.CoreLib]System.Reflection.IntrospectionExtensions::GetTypeInfo(class [System.Private.CoreLib]System.Type)
    IL_01ad:  ldstr      "Property"
    IL_01b2:  callvirt   instance class [System.Private.CoreLib]System.Reflection.PropertyInfo [System.Private.CoreLib]System.Type::GetProperty(string)
    IL_01b7:  stloc.2
    IL_01b8:  ldloc.2
    IL_01b9:  call       !!0 [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttribute<class ConsoleApplication21.SingleAttribute`1<int32>>(class [System.Private.CoreLib]System.Reflection.MemberInfo)
    IL_01be:  ldnull
    IL_01bf:  cgt.un
    IL_01c1:  ldc.i4.s   86
    IL_01c3:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_01c8:  nop
    IL_01c9:  ldloc.2
    IL_01ca:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_01cf:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_01d4:  ldc.i4.1
    IL_01d5:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_01da:  ldnull
    IL_01db:  cgt.un
    IL_01dd:  ldc.i4.s   87
    IL_01df:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_01e4:  nop
    IL_01e5:  ldloc.2
    IL_01e6:  call       !!0 [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttribute<class ConsoleApplication21.SingleAttribute`1<bool>>(class [System.Private.CoreLib]System.Reflection.MemberInfo)
    IL_01eb:  ldnull
    IL_01ec:  cgt.un
    IL_01ee:  ldc.i4.s   88
    IL_01f0:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_01f5:  nop
    IL_01f6:  ldloc.2
    IL_01f7:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_01fc:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0201:  ldc.i4.1
    IL_0202:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_0207:  ldnull
    IL_0208:  cgt.un
    IL_020a:  ldc.i4.s   89
    IL_020c:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0211:  nop
    IL_0212:  ldloc.2
    IL_0213:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_0218:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_021d:  call       bool [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::IsDefined(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                             class [System.Private.CoreLib]System.Type)
    IL_0222:  ldc.i4.s   90
    IL_0224:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0229:  nop
    IL_022a:  ldloc.2
    IL_022b:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_0230:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0235:  ldc.i4.1
    IL_0236:  callvirt   instance bool [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::IsDefined(class [System.Private.CoreLib]System.Type,
                                                                                                                     bool)
    IL_023b:  ldc.i4.s   91
    IL_023d:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0242:  nop
    IL_0243:  ldloc.2
    IL_0244:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_0249:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_024e:  call       bool [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::IsDefined(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                             class [System.Private.CoreLib]System.Type)
    IL_0253:  ldc.i4.s   92
    IL_0255:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_025a:  nop
    IL_025b:  ldloc.2
    IL_025c:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_0261:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0266:  ldc.i4.1
    IL_0267:  callvirt   instance bool [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::IsDefined(class [System.Private.CoreLib]System.Type,
                                                                                                                     bool)
    IL_026c:  ldc.i4.s   93
    IL_026e:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0273:  nop
    IL_0274:  ldtoken    ConsoleApplication21.Program/Derive
    IL_0279:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_027e:  call       class [System.Private.CoreLib]System.Reflection.TypeInfo [System.Private.CoreLib]System.Reflection.IntrospectionExtensions::GetTypeInfo(class [System.Private.CoreLib]System.Type)
    IL_0283:  stloc.3
    IL_0284:  ldloc.3
    IL_0285:  ldc.i4.0
    IL_0286:  call       !!0 [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttribute<class ConsoleApplication21.SingleAttribute`1<int32>>(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                          bool)
    IL_028b:  ldnull
    IL_028c:  ceq
    IL_028e:  ldc.i4.s   96
    IL_0290:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0295:  nop
    IL_0296:  ldloc.3
    IL_0297:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_029c:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_02a1:  ldc.i4.1
    IL_02a2:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_02a7:  ldnull
    IL_02a8:  cgt.un
    IL_02aa:  ldc.i4.s   97
    IL_02ac:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_02b1:  nop
    IL_02b2:  ldloc.3
    IL_02b3:  ldc.i4.0
    IL_02b4:  call       !!0 [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttribute<class ConsoleApplication21.SingleAttribute`1<bool>>(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                         bool)
    IL_02b9:  ldnull
    IL_02ba:  ceq
    IL_02bc:  ldc.i4.s   98
    IL_02be:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_02c3:  nop
    IL_02c4:  ldloc.3
    IL_02c5:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_02ca:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_02cf:  ldc.i4.1
    IL_02d0:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_02d5:  ldnull
    IL_02d6:  cgt.un
    IL_02d8:  ldc.i4.s   99
    IL_02da:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_02df:  nop
    IL_02e0:  ldloc.3
    IL_02e1:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_02e6:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_02eb:  ldc.i4.0
    IL_02ec:  call       bool [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::IsDefined(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                             class [System.Private.CoreLib]System.Type,
                                                                                                             bool)
    IL_02f1:  ldc.i4.0
    IL_02f2:  ceq
    IL_02f4:  ldc.i4.s   100
    IL_02f6:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_02fb:  nop
    IL_02fc:  ldloc.3
    IL_02fd:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_0302:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0307:  ldc.i4.0
    IL_0308:  call       bool [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::IsDefined(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                             class [System.Private.CoreLib]System.Type,
                                                                                                             bool)
    IL_030d:  ldc.i4.0
    IL_030e:  ceq
    IL_0310:  ldc.i4.s   101
    IL_0312:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0317:  nop
    IL_0318:  ldloc.3
    IL_0319:  ldc.i4.1
    IL_031a:  call       !!0 [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttribute<class ConsoleApplication21.SingleAttribute`1<int32>>(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                          bool)
    IL_031f:  ldnull
    IL_0320:  cgt.un
    IL_0322:  ldc.i4.s   103
    IL_0324:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0329:  nop
    IL_032a:  ldloc.3
    IL_032b:  ldc.i4.1
    IL_032c:  call       !!0 [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttribute<class ConsoleApplication21.SingleAttribute`1<bool>>(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                         bool)
    IL_0331:  ldnull
    IL_0332:  cgt.un
    IL_0334:  ldc.i4.s   104
    IL_0336:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_033b:  nop
    IL_033c:  ldloc.3
    IL_033d:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_0342:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0347:  ldc.i4.1
    IL_0348:  call       bool [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::IsDefined(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                             class [System.Private.CoreLib]System.Type,
                                                                                                             bool)
    IL_034d:  ldc.i4.s   105
    IL_034f:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0354:  nop
    IL_0355:  ldloc.3
    IL_0356:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_035b:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0360:  ldc.i4.1
    IL_0361:  callvirt   instance bool [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::IsDefined(class [System.Private.CoreLib]System.Type,
                                                                                                                     bool)
    IL_0366:  ldc.i4.s   106
    IL_0368:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_036d:  nop
    IL_036e:  ldloc.3
    IL_036f:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_0374:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0379:  ldc.i4.1
    IL_037a:  call       bool [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::IsDefined(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                             class [System.Private.CoreLib]System.Type,
                                                                                                             bool)
    IL_037f:  ldc.i4.s   107
    IL_0381:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0386:  nop
    IL_0387:  ldloc.3
    IL_0388:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_038d:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0392:  ldc.i4.1
    IL_0393:  callvirt   instance bool [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::IsDefined(class [System.Private.CoreLib]System.Type,
                                                                                                                     bool)
    IL_0398:  ldc.i4.s   108
    IL_039a:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_039f:  nop
    IL_03a0:  ldloc.1
    IL_03a1:  ldc.i4.1
    IL_03a2:  call       class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class [System.Private.CoreLib]System.Attribute> [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttributes(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                                                                                         bool)
    IL_03a7:  callvirt   instance class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<!0> class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class [System.Private.CoreLib]System.Attribute>::GetEnumerator()
    IL_03ac:  stloc.s    V_4
    IL_03ae:  ldloc.s    V_4
    IL_03b0:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_0'
    IL_03b5:  dup
    IL_03b6:  brtrue.s   IL_03cf

    IL_03b8:  pop
    IL_03b9:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_03be:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_0'(class [System.Private.CoreLib]System.Attribute)
    IL_03c4:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_03c9:  dup
    IL_03ca:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_0'
    IL_03cf:  ldc.i4.s   111
    IL_03d1:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_03d6:  nop
    IL_03d7:  ldloc.s    V_4
    IL_03d9:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_1'
    IL_03de:  dup
    IL_03df:  brtrue.s   IL_03f8

    IL_03e1:  pop
    IL_03e2:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_03e7:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_1'(class [System.Private.CoreLib]System.Attribute)
    IL_03ed:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_03f2:  dup
    IL_03f3:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_1'
    IL_03f8:  ldc.i4.s   112
    IL_03fa:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_03ff:  nop
    IL_0400:  ldloc.s    V_4
    IL_0402:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_2'
    IL_0407:  dup
    IL_0408:  brtrue.s   IL_0421

    IL_040a:  pop
    IL_040b:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0410:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_2'(class [System.Private.CoreLib]System.Attribute)
    IL_0416:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_041b:  dup
    IL_041c:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_2'
    IL_0421:  ldc.i4.s   113
    IL_0423:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0428:  nop
    IL_0429:  ldloc.s    V_4
    IL_042b:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_3'
    IL_0430:  dup
    IL_0431:  brtrue.s   IL_044a

    IL_0433:  pop
    IL_0434:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0439:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_3'(class [System.Private.CoreLib]System.Attribute)
    IL_043f:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0444:  dup
    IL_0445:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_3'
    IL_044a:  ldc.i4.s   114
    IL_044c:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0451:  nop
    IL_0452:  ldloc.s    V_4
    IL_0454:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_4'
    IL_0459:  dup
    IL_045a:  brtrue.s   IL_0473

    IL_045c:  pop
    IL_045d:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0462:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_4'(class [System.Private.CoreLib]System.Attribute)
    IL_0468:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_046d:  dup
    IL_046e:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_4'
    IL_0473:  ldc.i4.s   115
    IL_0475:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_047a:  nop
    IL_047b:  ldloc.s    V_4
    IL_047d:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_5'
    IL_0482:  dup
    IL_0483:  brtrue.s   IL_049c

    IL_0485:  pop
    IL_0486:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_048b:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_5'(class [System.Private.CoreLib]System.Attribute)
    IL_0491:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0496:  dup
    IL_0497:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_5'
    IL_049c:  ldc.i4.s   116
    IL_049e:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_04a3:  nop
    IL_04a4:  ldloc.s    V_4
    IL_04a6:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_6'
    IL_04ab:  dup
    IL_04ac:  brtrue.s   IL_04c5

    IL_04ae:  pop
    IL_04af:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_04b4:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_6'(class [System.Private.CoreLib]System.Attribute)
    IL_04ba:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_04bf:  dup
    IL_04c0:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_6'
    IL_04c5:  ldc.i4.s   117
    IL_04c7:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_04cc:  nop
    IL_04cd:  ldloc.s    V_4
    IL_04cf:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_7'
    IL_04d4:  dup
    IL_04d5:  brtrue.s   IL_04ee

    IL_04d7:  pop
    IL_04d8:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_04dd:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_7'(class [System.Private.CoreLib]System.Attribute)
    IL_04e3:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_04e8:  dup
    IL_04e9:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_7'
    IL_04ee:  ldc.i4.s   118
    IL_04f0:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_04f5:  nop
    IL_04f6:  ldloc.s    V_4
    IL_04f8:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_8'
    IL_04fd:  dup
    IL_04fe:  brtrue.s   IL_0517

    IL_0500:  pop
    IL_0501:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0506:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_8'(class [System.Private.CoreLib]System.Attribute)
    IL_050c:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0511:  dup
    IL_0512:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_8'
    IL_0517:  ldc.i4.s   119
    IL_0519:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_051e:  nop
    IL_051f:  ldloc.1
    IL_0520:  ldc.i4.1
    IL_0521:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(bool)
    IL_0526:  callvirt   instance class [System.Private.CoreLib]System.Collections.IEnumerator [System.Private.CoreLib]System.Array::GetEnumerator()
    IL_052b:  stloc.s    V_5
    IL_052d:  ldloc.s    V_5
    IL_052f:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_9'
    IL_0534:  dup
    IL_0535:  brtrue.s   IL_054e

    IL_0537:  pop
    IL_0538:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_053d:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_9'(class [System.Private.CoreLib]System.Attribute)
    IL_0543:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0548:  dup
    IL_0549:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_9'
    IL_054e:  ldc.i4.s   122
    IL_0550:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0555:  nop
    IL_0556:  ldloc.s    V_5
    IL_0558:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_10'
    IL_055d:  dup
    IL_055e:  brtrue.s   IL_0577

    IL_0560:  pop
    IL_0561:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0566:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_10'(class [System.Private.CoreLib]System.Attribute)
    IL_056c:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0571:  dup
    IL_0572:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_10'
    IL_0577:  ldc.i4.s   123
    IL_0579:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_057e:  nop
    IL_057f:  ldloc.s    V_5
    IL_0581:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_11'
    IL_0586:  dup
    IL_0587:  brtrue.s   IL_05a0

    IL_0589:  pop
    IL_058a:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_058f:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_11'(class [System.Private.CoreLib]System.Attribute)
    IL_0595:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_059a:  dup
    IL_059b:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_11'
    IL_05a0:  ldc.i4.s   124
    IL_05a2:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_05a7:  nop
    IL_05a8:  ldloc.s    V_5
    IL_05aa:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_12'
    IL_05af:  dup
    IL_05b0:  brtrue.s   IL_05c9

    IL_05b2:  pop
    IL_05b3:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_05b8:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_12'(class [System.Private.CoreLib]System.Attribute)
    IL_05be:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_05c3:  dup
    IL_05c4:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_12'
    IL_05c9:  ldc.i4.s   125
    IL_05cb:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_05d0:  nop
    IL_05d1:  ldloc.s    V_5
    IL_05d3:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_13'
    IL_05d8:  dup
    IL_05d9:  brtrue.s   IL_05f2

    IL_05db:  pop
    IL_05dc:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_05e1:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_13'(class [System.Private.CoreLib]System.Attribute)
    IL_05e7:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_05ec:  dup
    IL_05ed:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_13'
    IL_05f2:  ldc.i4.s   126
    IL_05f4:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_05f9:  nop
    IL_05fa:  ldloc.s    V_5
    IL_05fc:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_14'
    IL_0601:  dup
    IL_0602:  brtrue.s   IL_061b

    IL_0604:  pop
    IL_0605:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_060a:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_14'(class [System.Private.CoreLib]System.Attribute)
    IL_0610:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0615:  dup
    IL_0616:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_14'
    IL_061b:  ldc.i4.s   127
    IL_061d:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0622:  nop
    IL_0623:  ldloc.s    V_5
    IL_0625:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_15'
    IL_062a:  dup
    IL_062b:  brtrue.s   IL_0644

    IL_062d:  pop
    IL_062e:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0633:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_15'(class [System.Private.CoreLib]System.Attribute)
    IL_0639:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_063e:  dup
    IL_063f:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_15'
    IL_0644:  ldc.i4     0x80
    IL_0649:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_064e:  nop
    IL_064f:  ldloc.s    V_5
    IL_0651:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_16'
    IL_0656:  dup
    IL_0657:  brtrue.s   IL_0670

    IL_0659:  pop
    IL_065a:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_065f:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_16'(class [System.Private.CoreLib]System.Attribute)
    IL_0665:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_066a:  dup
    IL_066b:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_16'
    IL_0670:  ldc.i4     0x81
    IL_0675:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_067a:  nop
    IL_067b:  ldloc.s    V_5
    IL_067d:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_17'
    IL_0682:  dup
    IL_0683:  brtrue.s   IL_069c

    IL_0685:  pop
    IL_0686:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_068b:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_17'(class [System.Private.CoreLib]System.Attribute)
    IL_0691:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0696:  dup
    IL_0697:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_17'
    IL_069c:  ldc.i4     0x82
    IL_06a1:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_06a6:  nop
    IL_06a7:  ldloc.3
    IL_06a8:  ldc.i4.0
    IL_06a9:  call       class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class [System.Private.CoreLib]System.Attribute> [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttributes(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                                                                                         bool)
    IL_06ae:  callvirt   instance class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<!0> class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class [System.Private.CoreLib]System.Attribute>::GetEnumerator()
    IL_06b3:  stloc.s    V_6
    IL_06b5:  ldloc.s    V_6
    IL_06b7:  callvirt   instance bool [System.Private.CoreLib]System.Collections.IEnumerator::MoveNext()
    IL_06bc:  ldc.i4.0
    IL_06bd:  ceq
    IL_06bf:  ldc.i4     0x85
    IL_06c4:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_06c9:  nop
    IL_06ca:  ldloc.3
    IL_06cb:  ldc.i4.0
    IL_06cc:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(bool)
    IL_06d1:  callvirt   instance class [System.Private.CoreLib]System.Collections.IEnumerator [System.Private.CoreLib]System.Array::GetEnumerator()
    IL_06d6:  stloc.s    V_7
    IL_06d8:  ldloc.s    V_7
    IL_06da:  callvirt   instance bool [System.Private.CoreLib]System.Collections.IEnumerator::MoveNext()
    IL_06df:  ldc.i4.0
    IL_06e0:  ceq
    IL_06e2:  ldc.i4     0x88
    IL_06e7:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_06ec:  nop
    IL_06ed:  ldloc.3
    IL_06ee:  ldc.i4.1
    IL_06ef:  call       class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class [System.Private.CoreLib]System.Attribute> [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttributes(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                                                                                         bool)
    IL_06f4:  callvirt   instance class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<!0> class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class [System.Private.CoreLib]System.Attribute>::GetEnumerator()
    IL_06f9:  stloc.s    V_8
    IL_06fb:  ldloc.s    V_8
    IL_06fd:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_18'
    IL_0702:  dup
    IL_0703:  brtrue.s   IL_071c

    IL_0705:  pop
    IL_0706:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_070b:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_18'(class [System.Private.CoreLib]System.Attribute)
    IL_0711:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0716:  dup
    IL_0717:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_18'
    IL_071c:  ldc.i4     0x8b
    IL_0721:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0726:  nop
    IL_0727:  ldloc.s    V_8
    IL_0729:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_19'
    IL_072e:  dup
    IL_072f:  brtrue.s   IL_0748

    IL_0731:  pop
    IL_0732:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0737:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_19'(class [System.Private.CoreLib]System.Attribute)
    IL_073d:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0742:  dup
    IL_0743:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_19'
    IL_0748:  ldc.i4     0x8c
    IL_074d:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0752:  nop
    IL_0753:  ldloc.s    V_8
    IL_0755:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_20'
    IL_075a:  dup
    IL_075b:  brtrue.s   IL_0774

    IL_075d:  pop
    IL_075e:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0763:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_20'(class [System.Private.CoreLib]System.Attribute)
    IL_0769:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_076e:  dup
    IL_076f:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_20'
    IL_0774:  ldc.i4     0x8d
    IL_0779:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_077e:  nop
    IL_077f:  ldloc.s    V_8
    IL_0781:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_21'
    IL_0786:  dup
    IL_0787:  brtrue.s   IL_07a0

    IL_0789:  pop
    IL_078a:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_078f:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_21'(class [System.Private.CoreLib]System.Attribute)
    IL_0795:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_079a:  dup
    IL_079b:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_21'
    IL_07a0:  ldc.i4     0x8e
    IL_07a5:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_07aa:  nop
    IL_07ab:  ldloc.s    V_8
    IL_07ad:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_22'
    IL_07b2:  dup
    IL_07b3:  brtrue.s   IL_07cc

    IL_07b5:  pop
    IL_07b6:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_07bb:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_22'(class [System.Private.CoreLib]System.Attribute)
    IL_07c1:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_07c6:  dup
    IL_07c7:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_22'
    IL_07cc:  ldc.i4     0x8f
    IL_07d1:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_07d6:  nop
    IL_07d7:  ldloc.s    V_8
    IL_07d9:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_23'
    IL_07de:  dup
    IL_07df:  brtrue.s   IL_07f8

    IL_07e1:  pop
    IL_07e2:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_07e7:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_23'(class [System.Private.CoreLib]System.Attribute)
    IL_07ed:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_07f2:  dup
    IL_07f3:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_23'
    IL_07f8:  ldc.i4     0x90
    IL_07fd:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0802:  nop
    IL_0803:  ldloc.s    V_8
    IL_0805:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_24'
    IL_080a:  dup
    IL_080b:  brtrue.s   IL_0824

    IL_080d:  pop
    IL_080e:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0813:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_24'(class [System.Private.CoreLib]System.Attribute)
    IL_0819:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_081e:  dup
    IL_081f:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_24'
    IL_0824:  ldc.i4     0x91
    IL_0829:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_082e:  nop
    IL_082f:  ldloc.3
    IL_0830:  ldc.i4.1
    IL_0831:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(bool)
    IL_0836:  callvirt   instance class [System.Private.CoreLib]System.Collections.IEnumerator [System.Private.CoreLib]System.Array::GetEnumerator()
    IL_083b:  stloc.s    V_9
    IL_083d:  ldloc.s    V_9
    IL_083f:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_25'
    IL_0844:  dup
    IL_0845:  brtrue.s   IL_085e

    IL_0847:  pop
    IL_0848:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_084d:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_25'(class [System.Private.CoreLib]System.Attribute)
    IL_0853:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0858:  dup
    IL_0859:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_25'
    IL_085e:  ldc.i4     0x94
    IL_0863:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0868:  nop
    IL_0869:  ldloc.s    V_9
    IL_086b:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_26'
    IL_0870:  dup
    IL_0871:  brtrue.s   IL_088a

    IL_0873:  pop
    IL_0874:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0879:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_26'(class [System.Private.CoreLib]System.Attribute)
    IL_087f:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0884:  dup
    IL_0885:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_26'
    IL_088a:  ldc.i4     0x95
    IL_088f:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0894:  nop
    IL_0895:  ldloc.s    V_9
    IL_0897:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_27'
    IL_089c:  dup
    IL_089d:  brtrue.s   IL_08b6

    IL_089f:  pop
    IL_08a0:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_08a5:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_27'(class [System.Private.CoreLib]System.Attribute)
    IL_08ab:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_08b0:  dup
    IL_08b1:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_27'
    IL_08b6:  ldc.i4     0x96
    IL_08bb:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_08c0:  nop
    IL_08c1:  ldloc.s    V_9
    IL_08c3:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_28'
    IL_08c8:  dup
    IL_08c9:  brtrue.s   IL_08e2

    IL_08cb:  pop
    IL_08cc:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_08d1:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_28'(class [System.Private.CoreLib]System.Attribute)
    IL_08d7:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_08dc:  dup
    IL_08dd:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_28'
    IL_08e2:  ldc.i4     0x97
    IL_08e7:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_08ec:  nop
    IL_08ed:  ldloc.s    V_9
    IL_08ef:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_29'
    IL_08f4:  dup
    IL_08f5:  brtrue.s   IL_090e

    IL_08f7:  pop
    IL_08f8:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_08fd:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_29'(class [System.Private.CoreLib]System.Attribute)
    IL_0903:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0908:  dup
    IL_0909:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_29'
    IL_090e:  ldc.i4     0x98
    IL_0913:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0918:  nop
    IL_0919:  ldloc.s    V_9
    IL_091b:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_30'
    IL_0920:  dup
    IL_0921:  brtrue.s   IL_093a

    IL_0923:  pop
    IL_0924:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0929:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_30'(class [System.Private.CoreLib]System.Attribute)
    IL_092f:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0934:  dup
    IL_0935:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_30'
    IL_093a:  ldc.i4     0x99
    IL_093f:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0944:  nop
    IL_0945:  ldloc.s    V_9
    IL_0947:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_31'
    IL_094c:  dup
    IL_094d:  brtrue.s   IL_0966

    IL_094f:  pop
    IL_0950:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0955:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_31'(class [System.Private.CoreLib]System.Attribute)
    IL_095b:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0960:  dup
    IL_0961:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_31'
    IL_0966:  ldc.i4     0x9a
    IL_096b:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0970:  nop
    IL_0971:  ldloc.1
    IL_0972:  ldc.i4.1
    IL_0973:  call       class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<!!0> [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttributes<class ConsoleApplication21.SingleAttribute`1<int32>>(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                                                                                                   bool)
    IL_0978:  callvirt   instance class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<!0> class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class ConsoleApplication21.SingleAttribute`1<int32>>::GetEnumerator()
    IL_097d:  stloc.s    V_10
    IL_097f:  ldloc.s    V_10
    IL_0981:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_32'
    IL_0986:  dup
    IL_0987:  brtrue.s   IL_09a0

    IL_0989:  pop
    IL_098a:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_098f:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_32'(class [System.Private.CoreLib]System.Attribute)
    IL_0995:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_099a:  dup
    IL_099b:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_32'
    IL_09a0:  ldc.i4     0x9d
    IL_09a5:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_09aa:  nop
    IL_09ab:  ldloc.1
    IL_09ac:  ldtoken    class ConsoleApplication21.SingleAttribute`1<int32>
    IL_09b1:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_09b6:  ldc.i4.1
    IL_09b7:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_09bc:  callvirt   instance class [System.Private.CoreLib]System.Collections.IEnumerator [System.Private.CoreLib]System.Array::GetEnumerator()
    IL_09c1:  stloc.s    V_11
    IL_09c3:  ldloc.s    V_11
    IL_09c5:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_33'
    IL_09ca:  dup
    IL_09cb:  brtrue.s   IL_09e4

    IL_09cd:  pop
    IL_09ce:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_09d3:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_33'(class [System.Private.CoreLib]System.Attribute)
    IL_09d9:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_09de:  dup
    IL_09df:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_33'
    IL_09e4:  ldc.i4     0xa0
    IL_09e9:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_09ee:  nop
    IL_09ef:  ldloc.1
    IL_09f0:  call       class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<!!0> [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttributes<class ConsoleApplication21.SingleAttribute`1<bool>>(class [System.Private.CoreLib]System.Reflection.MemberInfo)
    IL_09f5:  callvirt   instance class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<!0> class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class ConsoleApplication21.SingleAttribute`1<bool>>::GetEnumerator()
    IL_09fa:  stloc.s    V_12
    IL_09fc:  ldloc.s    V_12
    IL_09fe:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_34'
    IL_0a03:  dup
    IL_0a04:  brtrue.s   IL_0a1d

    IL_0a06:  pop
    IL_0a07:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0a0c:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_34'(class [System.Private.CoreLib]System.Attribute)
    IL_0a12:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0a17:  dup
    IL_0a18:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_34'
    IL_0a1d:  ldc.i4     0xa3
    IL_0a22:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0a27:  nop
    IL_0a28:  ldloc.1
    IL_0a29:  ldtoken    class ConsoleApplication21.SingleAttribute`1<bool>
    IL_0a2e:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0a33:  ldc.i4.1
    IL_0a34:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_0a39:  callvirt   instance class [System.Private.CoreLib]System.Collections.IEnumerator [System.Private.CoreLib]System.Array::GetEnumerator()
    IL_0a3e:  stloc.s    V_13
    IL_0a40:  ldloc.s    V_13
    IL_0a42:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_35'
    IL_0a47:  dup
    IL_0a48:  brtrue.s   IL_0a61

    IL_0a4a:  pop
    IL_0a4b:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0a50:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_35'(class [System.Private.CoreLib]System.Attribute)
    IL_0a56:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0a5b:  dup
    IL_0a5c:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_35'
    IL_0a61:  ldc.i4     0xa6
    IL_0a66:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0a6b:  nop
    IL_0a6c:  ldloc.1
    IL_0a6d:  ldc.i4.1
    IL_0a6e:  call       class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<!!0> [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttributes<class ConsoleApplication21.MultiAttribute`1<int32>>(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                                                                                                  bool)
    IL_0a73:  callvirt   instance class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<!0> class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class ConsoleApplication21.MultiAttribute`1<int32>>::GetEnumerator()
    IL_0a78:  stloc.s    V_14
    IL_0a7a:  ldloc.s    V_14
    IL_0a7c:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_36'
    IL_0a81:  dup
    IL_0a82:  brtrue.s   IL_0a9b

    IL_0a84:  pop
    IL_0a85:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0a8a:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_36'(class [System.Private.CoreLib]System.Attribute)
    IL_0a90:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0a95:  dup
    IL_0a96:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_36'
    IL_0a9b:  ldc.i4     0xa9
    IL_0aa0:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0aa5:  nop
    IL_0aa6:  ldloc.s    V_14
    IL_0aa8:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_37'
    IL_0aad:  dup
    IL_0aae:  brtrue.s   IL_0ac7

    IL_0ab0:  pop
    IL_0ab1:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0ab6:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_37'(class [System.Private.CoreLib]System.Attribute)
    IL_0abc:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0ac1:  dup
    IL_0ac2:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_37'
    IL_0ac7:  ldc.i4     0xaa
    IL_0acc:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0ad1:  nop
    IL_0ad2:  ldloc.s    V_14
    IL_0ad4:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_38'
    IL_0ad9:  dup
    IL_0ada:  brtrue.s   IL_0af3

    IL_0adc:  pop
    IL_0add:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0ae2:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_38'(class [System.Private.CoreLib]System.Attribute)
    IL_0ae8:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0aed:  dup
    IL_0aee:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_38'
    IL_0af3:  ldc.i4     0xab
    IL_0af8:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0afd:  nop
    IL_0afe:  ldloc.1
    IL_0aff:  ldtoken    class ConsoleApplication21.MultiAttribute`1<int32>
    IL_0b04:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0b09:  ldc.i4.1
    IL_0b0a:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_0b0f:  callvirt   instance class [System.Private.CoreLib]System.Collections.IEnumerator [System.Private.CoreLib]System.Array::GetEnumerator()
    IL_0b14:  stloc.s    V_15
    IL_0b16:  ldloc.s    V_15
    IL_0b18:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_39'
    IL_0b1d:  dup
    IL_0b1e:  brtrue.s   IL_0b37

    IL_0b20:  pop
    IL_0b21:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0b26:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_39'(class [System.Private.CoreLib]System.Attribute)
    IL_0b2c:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0b31:  dup
    IL_0b32:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_39'
    IL_0b37:  ldc.i4     0xae
    IL_0b3c:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0b41:  nop
    IL_0b42:  ldloc.s    V_15
    IL_0b44:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_40'
    IL_0b49:  dup
    IL_0b4a:  brtrue.s   IL_0b63

    IL_0b4c:  pop
    IL_0b4d:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0b52:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_40'(class [System.Private.CoreLib]System.Attribute)
    IL_0b58:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0b5d:  dup
    IL_0b5e:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_40'
    IL_0b63:  ldc.i4     0xaf
    IL_0b68:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0b6d:  nop
    IL_0b6e:  ldloc.s    V_15
    IL_0b70:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_41'
    IL_0b75:  dup
    IL_0b76:  brtrue.s   IL_0b8f

    IL_0b78:  pop
    IL_0b79:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0b7e:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_41'(class [System.Private.CoreLib]System.Attribute)
    IL_0b84:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0b89:  dup
    IL_0b8a:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_41'
    IL_0b8f:  ldc.i4     0xb0
    IL_0b94:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0b99:  nop
    IL_0b9a:  ldloc.1
    IL_0b9b:  ldc.i4.1
    IL_0b9c:  call       class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<!!0> [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttributes<class ConsoleApplication21.MultiAttribute`1<bool>>(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                                                                                                 bool)
    IL_0ba1:  callvirt   instance class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<!0> class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class ConsoleApplication21.MultiAttribute`1<bool>>::GetEnumerator()
    IL_0ba6:  stloc.s    V_16
    IL_0ba8:  ldloc.s    V_16
    IL_0baa:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_42'
    IL_0baf:  dup
    IL_0bb0:  brtrue.s   IL_0bc9

    IL_0bb2:  pop
    IL_0bb3:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0bb8:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_42'(class [System.Private.CoreLib]System.Attribute)
    IL_0bbe:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0bc3:  dup
    IL_0bc4:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_42'
    IL_0bc9:  ldc.i4     0xb3
    IL_0bce:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0bd3:  nop
    IL_0bd4:  ldloc.s    V_16
    IL_0bd6:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_43'
    IL_0bdb:  dup
    IL_0bdc:  brtrue.s   IL_0bf5

    IL_0bde:  pop
    IL_0bdf:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0be4:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_43'(class [System.Private.CoreLib]System.Attribute)
    IL_0bea:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0bef:  dup
    IL_0bf0:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_43'
    IL_0bf5:  ldc.i4     0xb4
    IL_0bfa:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0bff:  nop
    IL_0c00:  ldloc.1
    IL_0c01:  ldtoken    class ConsoleApplication21.MultiAttribute`1<bool>
    IL_0c06:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0c0b:  ldc.i4.1
    IL_0c0c:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_0c11:  callvirt   instance class [System.Private.CoreLib]System.Collections.IEnumerator [System.Private.CoreLib]System.Array::GetEnumerator()
    IL_0c16:  stloc.s    V_17
    IL_0c18:  ldloc.s    V_17
    IL_0c1a:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_44'
    IL_0c1f:  dup
    IL_0c20:  brtrue.s   IL_0c39

    IL_0c22:  pop
    IL_0c23:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0c28:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_44'(class [System.Private.CoreLib]System.Attribute)
    IL_0c2e:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0c33:  dup
    IL_0c34:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_44'
    IL_0c39:  ldc.i4     0xb7
    IL_0c3e:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0c43:  nop
    IL_0c44:  ldloc.s    V_17
    IL_0c46:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_45'
    IL_0c4b:  dup
    IL_0c4c:  brtrue.s   IL_0c65

    IL_0c4e:  pop
    IL_0c4f:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0c54:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_45'(class [System.Private.CoreLib]System.Attribute)
    IL_0c5a:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0c5f:  dup
    IL_0c60:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_45'
    IL_0c65:  ldc.i4     0xb8
    IL_0c6a:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0c6f:  nop
    IL_0c70:  ldloc.1
    IL_0c71:  ldc.i4.1
    IL_0c72:  call       class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<!!0> [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttributes<class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>>(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                                                                                                                                                      bool)
    IL_0c77:  callvirt   instance class [System.Private.CoreLib]System.Collections.Generic.IEnumerator`1<!0> class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>>::GetEnumerator()
    IL_0c7c:  stloc.s    V_18
    IL_0c7e:  ldloc.s    V_18
    IL_0c80:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_46'
    IL_0c85:  dup
    IL_0c86:  brtrue.s   IL_0c9f

    IL_0c88:  pop
    IL_0c89:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0c8e:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_46'(class [System.Private.CoreLib]System.Attribute)
    IL_0c94:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0c99:  dup
    IL_0c9a:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_46'
    IL_0c9f:  ldc.i4     0xbb
    IL_0ca4:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0ca9:  nop
    IL_0caa:  ldloc.1
    IL_0cab:  ldtoken    class ConsoleApplication21.MultiAttribute`1<valuetype [System.Private.CoreLib]System.Nullable`1<bool>>
    IL_0cb0:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0cb5:  ldc.i4.1
    IL_0cb6:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_0cbb:  callvirt   instance class [System.Private.CoreLib]System.Collections.IEnumerator [System.Private.CoreLib]System.Array::GetEnumerator()
    IL_0cc0:  stloc.s    V_19
    IL_0cc2:  ldloc.s    V_19
    IL_0cc4:  ldsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_47'
    IL_0cc9:  dup
    IL_0cca:  brtrue.s   IL_0ce3

    IL_0ccc:  pop
    IL_0ccd:  ldsfld     class ConsoleApplication21.Program/'<>c' ConsoleApplication21.Program/'<>c'::'<>9'
    IL_0cd2:  ldftn      instance bool ConsoleApplication21.Program/'<>c'::'<Main>b__5_47'(class [System.Private.CoreLib]System.Attribute)
    IL_0cd8:  newobj     instance void class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool>::.ctor(object,
                                                                                                                                               native int)
    IL_0cdd:  dup
    IL_0cde:  stsfld     class [System.Private.CoreLib]System.Func`2<class [System.Private.CoreLib]System.Attribute,bool> ConsoleApplication21.Program/'<>c'::'<>9__5_47'
    IL_0ce3:  ldc.i4     0xbe
    IL_0ce8:  call       void ConsoleApplication21.Program::AssertNext<class [System.Private.CoreLib]System.Attribute>(class [System.Private.CoreLib]System.Collections.IEnumerator,
                                                                                                                       class [System.Private.CoreLib]System.Func`2<!!0,bool>,
                                                                                                                       int32)
    IL_0ced:  nop
    IL_0cee:  ldloc.1
    IL_0cef:  ldtoken    ConsoleApplication21.MultiAttribute`1
    IL_0cf4:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0cf9:  ldc.i4.0
    IL_0cfa:  call       class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class [System.Private.CoreLib]System.Attribute> [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttributes(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                                                                                         class [System.Private.CoreLib]System.Type,
                                                                                                                                                                                                                                         bool)
    IL_0cff:  ldnull
    IL_0d00:  ceq
    IL_0d02:  ldc.i4     0xc0
    IL_0d07:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0d0c:  nop
    IL_0d0d:  ldloc.1
    IL_0d0e:  ldtoken    ConsoleApplication21.MultiAttribute`1
    IL_0d13:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0d18:  ldc.i4.1
    IL_0d19:  call       class [System.Private.CoreLib]System.Collections.Generic.IEnumerable`1<class [System.Private.CoreLib]System.Attribute> [System.Private.CoreLib]System.Reflection.CustomAttributeExtensions::GetCustomAttributes(class [System.Private.CoreLib]System.Reflection.MemberInfo,
                                                                                                                                                                                                                                         class [System.Private.CoreLib]System.Type,
                                                                                                                                                                                                                                         bool)
    IL_0d1e:  ldnull
    IL_0d1f:  ceq
    IL_0d21:  ldc.i4     0xc1
    IL_0d26:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0d2b:  nop
    IL_0d2c:  ldloc.1
    IL_0d2d:  ldtoken    ConsoleApplication21.MultiAttribute`1
    IL_0d32:  call       class [System.Private.CoreLib]System.Type [System.Private.CoreLib]System.Type::GetTypeFromHandle(valuetype [System.Private.CoreLib]System.RuntimeTypeHandle)
    IL_0d37:  ldc.i4.1
    IL_0d38:  callvirt   instance object[] [System.Private.CoreLib]System.Reflection.ICustomAttributeProvider::GetCustomAttributes(class [System.Private.CoreLib]System.Type,
                                                                                                                                   bool)
    IL_0d3d:  callvirt   instance class [System.Private.CoreLib]System.Collections.IEnumerator [System.Private.CoreLib]System.Array::GetEnumerator()
    IL_0d42:  callvirt   instance bool [System.Private.CoreLib]System.Collections.IEnumerator::MoveNext()
    IL_0d47:  ldc.i4.0
    IL_0d48:  ceq
    IL_0d4a:  ldc.i4     0xc2
    IL_0d4f:  call       void ConsoleApplication21.Program::Assert(bool,
                                                                   int32)
    IL_0d54:  nop
    IL_0d55:  ret
  } // end of method Program::Main

  .method private hidebysig static void  Assert(bool condition,
                                                [opt] int32 line) cil managed
  {
    .param [2] = int32(0x00000000)
    .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CallerLineNumberAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       33 (0x21)
    .maxstack  2
    .locals init (bool V_0)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldc.i4.0
    IL_0003:  ceq
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  brfalse.s  IL_0020

    IL_0009:  nop
    IL_000a:  ldstr      "Error in line: {0}"
    IL_000f:  ldarg.1
    IL_0010:  box        [System.Private.CoreLib]System.Int32
    IL_0015:  call       string [System.Private.CoreLib]System.String::Format(string,
                                                                              object)
    IL_001a:  newobj     instance void [System.Private.CoreLib]System.Exception::.ctor(string)
    IL_001f:  throw

    IL_0020:  ret
  } // end of method Program::Assert

  .method private hidebysig static void  AssertNext<T>(class [System.Private.CoreLib]System.Collections.IEnumerator source,
                                                       class [System.Private.CoreLib]System.Func`2<!!T,bool> func,
                                                       [opt] int32 line) cil managed
  {
    .param [3] = int32(0x00000000)
    .custom instance void [System.Private.CoreLib]System.Runtime.CompilerServices.CallerLineNumberAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       56 (0x38)
    .maxstack  2
    .locals init (bool V_0)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance bool [System.Private.CoreLib]System.Collections.IEnumerator::MoveNext()
    IL_0007:  pop
    IL_0008:  ldarg.1
    IL_0009:  ldarg.0
    IL_000a:  callvirt   instance object [System.Private.CoreLib]System.Collections.IEnumerator::get_Current()
    IL_000f:  unbox.any  !!T
    IL_0014:  callvirt   instance !1 class [System.Private.CoreLib]System.Func`2<!!T,bool>::Invoke(!0)
    IL_0019:  ldc.i4.0
    IL_001a:  ceq
    IL_001c:  stloc.0
    IL_001d:  ldloc.0
    IL_001e:  brfalse.s  IL_0037

    IL_0020:  nop
    IL_0021:  ldstr      "Error in line: {0}"
    IL_0026:  ldarg.2
    IL_0027:  box        [System.Private.CoreLib]System.Int32
    IL_002c:  call       string [System.Private.CoreLib]System.String::Format(string,
                                                                              object)
    IL_0031:  newobj     instance void [System.Private.CoreLib]System.Exception::.ctor(string)
    IL_0036:  throw

    IL_0037:  ret
  } // end of method Program::AssertNext

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [System.Private.CoreLib]System.Object::.ctor()
    IL_0006:  nop
    IL_0007:  ret
  } // end of method Program::.ctor

  .property instance int32 Property()
  {
    .custom instance void class ConsoleApplication21.SingleAttribute`1<int32>::.ctor() = ( 01 00 00 00 ) 
    .custom instance void class ConsoleApplication21.SingleAttribute`1<bool>::.ctor() = ( 01 00 00 00 ) 
    .custom instance void class ConsoleApplication21.MultiAttribute`1<int32>::.ctor() = ( 01 00 00 00 ) 
    .custom instance void class ConsoleApplication21.MultiAttribute`1<int32>::.ctor(!0) = ( 01 00 01 00 00 00 00 00 ) 
    .custom instance void class ConsoleApplication21.MultiAttribute`1<int32>::.ctor() = ( 01 00 01 00 54 08 05 56 61 6C 75 65 02 00 00 00 ) // ....T..Value....
    .custom instance void class ConsoleApplication21.MultiAttribute`1<bool>::.ctor() = ( 01 00 00 00 ) 
    .custom instance void class ConsoleApplication21.MultiAttribute`1<bool>::.ctor(!0) = ( 01 00 01 00 00 ) 
    .get instance int32 ConsoleApplication21.Program::get_Property()
    .set instance void ConsoleApplication21.Program::set_Property(int32)
  } // end of property Program::Property
} // end of class ConsoleApplication21.Program


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file Program.res

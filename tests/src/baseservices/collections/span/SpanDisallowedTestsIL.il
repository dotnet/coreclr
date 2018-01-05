// Metadata version: v4.0.30319
.assembly extern System.Runtime
{
  .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )                         // .?_....:
  .ver 4:2:0:0
}
.assembly extern System.Runtime.Extensions
{
  .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )                         // .?_....:
  .ver 4:2:0:0
}
.assembly extern System.Memory
{
  .publickeytoken = (CC 7B 13 FF CD 2D DD 51 )                         // .{...-.Q
  .ver 4:0:0:0
}
.assembly SpanDisallowedTestsIL
{
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                                   63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [System.Runtime]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [System.Runtime]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 02 00 00 00 00 00 ) 

  .permissionset reqmin
             = {[System.Runtime.Extensions]System.Security.Permissions.SecurityPermissionAttribute = {property bool 'SkipVerification' = bool(true)}}
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module SpanDisallowedTestsIL.dll
// MVID: {9C8CFD63-A6E0-450F-BF45-64D80B0EDFA9}
.custom instance void [System.Runtime]System.Security.UnverifiableCodeAttribute::.ctor() = ( 01 00 00 00 ) 
.imagebase 0x10000000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x000001F2BA8E0000


// =============== CLASS MEMBERS DECLARATION ===================

.class public abstract auto ansi sealed beforefieldinit SpanDisallowedTests
       extends [System.Runtime]System.Object
{
  .class abstract auto ansi sealed nested private beforefieldinit ClassStaticSpanTest_SpanContainer
         extends [System.Runtime]System.Object
  {
    .field public static valuetype [System.Memory]System.Span`1<int32> s_span
  } // end of class ClassStaticSpanTest_SpanContainer

  .class auto ansi sealed nested private beforefieldinit ClassInstanceSpanTest_SpanContainer
         extends [System.Runtime]System.Object
  {
    .field public valuetype [System.Memory]System.Span`1<int32> _span
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [System.Runtime]System.Object::.ctor()
      IL_0006:  ret
    } // end of method ClassInstanceSpanTest_SpanContainer::.ctor

  } // end of class ClassInstanceSpanTest_SpanContainer

  .class auto ansi sealed nested private beforefieldinit GenericClassInstanceSpanTest_SpanContainer`1<T>
         extends [System.Runtime]System.Object
  {
    .field public !T _t
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [System.Runtime]System.Object::.ctor()
      IL_0006:  ret
    } // end of method GenericClassInstanceSpanTest_SpanContainer`1::.ctor

  } // end of class GenericClassInstanceSpanTest_SpanContainer`1

  .class interface abstract auto ansi nested private GenericInterfaceOfSpanTest_ISpanContainer`1<T>
  {
    .method public hidebysig newslot abstract virtual 
            instance !T  Foo(!T t) cil managed
    {
    } // end of method GenericInterfaceOfSpanTest_ISpanContainer`1::Foo

  } // end of class GenericInterfaceOfSpanTest_ISpanContainer`1

  .class sequential ansi sealed nested private beforefieldinit GenericStructInstanceSpanTest_SpanContainer`1<T>
         extends [System.Runtime]System.ValueType
  {
    .field public !T _t
  } // end of class GenericStructInstanceSpanTest_SpanContainer`1

  .class auto ansi sealed nested private GenericDelegateOfSpan_Delegate`1<T>
         extends [System.Runtime]System.MulticastDelegate
  {
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(object 'object',
                                 native int 'method') runtime managed
    {
    } // end of method GenericDelegateOfSpan_Delegate`1::.ctor

    .method public hidebysig newslot virtual 
            instance !T  Invoke(!T span) runtime managed
    {
    } // end of method GenericDelegateOfSpan_Delegate`1::Invoke

    .method public hidebysig newslot virtual 
            instance class [System.Runtime]System.IAsyncResult 
            BeginInvoke(!T span,
                        class [System.Runtime]System.AsyncCallback callback,
                        object 'object') runtime managed
    {
    } // end of method GenericDelegateOfSpan_Delegate`1::BeginInvoke

    .method public hidebysig newslot virtual 
            instance !T  EndInvoke(class [System.Runtime]System.IAsyncResult result) runtime managed
    {
    } // end of method GenericDelegateOfSpan_Delegate`1::EndInvoke

  } // end of class GenericDelegateOfSpan_Delegate`1

  .class auto ansi serializable sealed nested private beforefieldinit '<>c'
         extends [System.Runtime]System.Object
  {
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public static initonly class SpanDisallowedTests/'<>c' '<>9'
    .field public static class [System.Runtime]System.Action '<>9__0_0'
    .field public static class [System.Runtime]System.Action '<>9__3_0'
    .field public static class [System.Runtime]System.Action '<>9__6_0'
    .field public static class [System.Runtime]System.Action '<>9__9_0'
    .field public static class [System.Runtime]System.Action '<>9__12_0'
    .field public static class [System.Runtime]System.Action '<>9__15_0'
    .field public static class [System.Runtime]System.Action '<>9__18_0'
    .field public static class [System.Runtime]System.Action '<>9__20_0'
    .method private hidebysig specialname rtspecialname static 
            void  .cctor() cil managed
    {
      // Code size       11 (0xb)
      .maxstack  8
      IL_0000:  newobj     instance void SpanDisallowedTests/'<>c'::.ctor()
      IL_0005:  stsfld     class SpanDisallowedTests/'<>c' SpanDisallowedTests/'<>c'::'<>9'
      IL_000a:  ret
    } // end of method '<>c'::.cctor

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [System.Runtime]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c'::.ctor

    .method assembly hidebysig instance void 
            '<ClassStaticSpanTest>b__0_0'() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  call       void SpanDisallowedTests::ClassStaticSpanTestCore()
      IL_0005:  ret
    } // end of method '<>c'::'<ClassStaticSpanTest>b__0_0'

    .method assembly hidebysig instance void 
            '<ClassInstanceSpanTest>b__3_0'() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  call       void SpanDisallowedTests::ClassInstanceSpanTestCore()
      IL_0005:  ret
    } // end of method '<>c'::'<ClassInstanceSpanTest>b__3_0'

    .method assembly hidebysig instance void 
            '<GenericClassInstanceSpanTest>b__6_0'() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  call       void SpanDisallowedTests::GenericClassInstanceSpanTestCore()
      IL_0005:  ret
    } // end of method '<>c'::'<GenericClassInstanceSpanTest>b__6_0'

    .method assembly hidebysig instance void 
            '<GenericInterfaceOfSpanTest>b__9_0'() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  call       void SpanDisallowedTests::GenericInterfaceOfSpanTestCore()
      IL_0005:  ret
    } // end of method '<>c'::'<GenericInterfaceOfSpanTest>b__9_0'

    .method assembly hidebysig instance void 
            '<GenericStructInstanceSpanTest>b__12_0'() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  call       void SpanDisallowedTests::GenericStructInstanceSpanTestCore()
      IL_0005:  ret
    } // end of method '<>c'::'<GenericStructInstanceSpanTest>b__12_0'

    .method assembly hidebysig instance void 
            '<GenericDelegateOfSpanTest>b__15_0'() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  call       void SpanDisallowedTests::GenericDelegateOfSpanTestCore()
      IL_0005:  ret
    } // end of method '<>c'::'<GenericDelegateOfSpanTest>b__15_0'

    .method assembly hidebysig instance void 
            '<ArrayOfSpanTest>b__18_0'() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  call       void SpanDisallowedTests::ArrayOfSpanTestCore()
      IL_0005:  ret
    } // end of method '<>c'::'<ArrayOfSpanTest>b__18_0'

    .method assembly hidebysig instance void 
            '<BoxSpanTest>b__20_0'() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  call       void SpanDisallowedTests::BoxSpanTestCore()
      IL_0005:  ret
    } // end of method '<>c'::'<BoxSpanTest>b__20_0'

  } // end of class '<>c'

  .method public hidebysig static void  ClassStaticSpanTest() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  8
    IL_0000:  ldsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__0_0'
    IL_0005:  dup
    IL_0006:  brtrue.s   IL_001f

    IL_0008:  pop
    IL_0009:  ldsfld     class SpanDisallowedTests/'<>c' SpanDisallowedTests/'<>c'::'<>9'
    IL_000e:  ldftn      instance void SpanDisallowedTests/'<>c'::'<ClassStaticSpanTest>b__0_0'()
    IL_0014:  newobj     instance void [System.Runtime]System.Action::.ctor(object,
                                                                            native int)
    IL_0019:  dup
    IL_001a:  stsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__0_0'
    IL_001f:  call       void Assert::Throws<class [System.Runtime]System.TypeLoadException>(class [System.Runtime]System.Action)
    IL_0024:  ret
  } // end of method SpanDisallowedTests::ClassStaticSpanTest

  .method private hidebysig static void  ClassStaticSpanTestCore() cil managed noinlining
  {
    // Code size       25 (0x19)
    .maxstack  8
    IL_0000:  ldc.i4.2
    IL_0001:  newarr     [System.Runtime]System.Int32
    IL_0006:  dup
    IL_0007:  ldc.i4.0
    IL_0008:  ldc.i4.1
    IL_0009:  stelem.i4
    IL_000a:  dup
    IL_000b:  ldc.i4.1
    IL_000c:  ldc.i4.2
    IL_000d:  stelem.i4
    IL_000e:  newobj     instance void valuetype [System.Memory]System.Span`1<int32>::.ctor(!0[])
    IL_0013:  stsfld     valuetype [System.Memory]System.Span`1<int32> SpanDisallowedTests/ClassStaticSpanTest_SpanContainer::s_span
    IL_0018:  ret
  } // end of method SpanDisallowedTests::ClassStaticSpanTestCore

  .method public hidebysig static void  ClassInstanceSpanTest() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  8
    IL_0000:  ldsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__3_0'
    IL_0005:  dup
    IL_0006:  brtrue.s   IL_001f

    IL_0008:  pop
    IL_0009:  ldsfld     class SpanDisallowedTests/'<>c' SpanDisallowedTests/'<>c'::'<>9'
    IL_000e:  ldftn      instance void SpanDisallowedTests/'<>c'::'<ClassInstanceSpanTest>b__3_0'()
    IL_0014:  newobj     instance void [System.Runtime]System.Action::.ctor(object,
                                                                            native int)
    IL_0019:  dup
    IL_001a:  stsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__3_0'
    IL_001f:  call       void Assert::Throws<class [System.Runtime]System.TypeLoadException>(class [System.Runtime]System.Action)
    IL_0024:  ret
  } // end of method SpanDisallowedTests::ClassInstanceSpanTest

  .method private hidebysig static void  ClassInstanceSpanTestCore() cil managed noinlining
  {
    // Code size       32 (0x20)
    .maxstack  5
    .locals init (class SpanDisallowedTests/ClassInstanceSpanTest_SpanContainer V_0)
    IL_0000:  newobj     instance void SpanDisallowedTests/ClassInstanceSpanTest_SpanContainer::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldc.i4.2
    IL_0008:  newarr     [System.Runtime]System.Int32
    IL_000d:  dup
    IL_000e:  ldc.i4.0
    IL_000f:  ldc.i4.1
    IL_0010:  stelem.i4
    IL_0011:  dup
    IL_0012:  ldc.i4.1
    IL_0013:  ldc.i4.2
    IL_0014:  stelem.i4
    IL_0015:  newobj     instance void valuetype [System.Memory]System.Span`1<int32>::.ctor(!0[])
    IL_001a:  stfld      valuetype [System.Memory]System.Span`1<int32> SpanDisallowedTests/ClassInstanceSpanTest_SpanContainer::_span
    IL_001f:  ret
  } // end of method SpanDisallowedTests::ClassInstanceSpanTestCore

  .method public hidebysig static void  GenericClassInstanceSpanTest() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  8
    IL_0000:  ldsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__6_0'
    IL_0005:  dup
    IL_0006:  brtrue.s   IL_001f

    IL_0008:  pop
    IL_0009:  ldsfld     class SpanDisallowedTests/'<>c' SpanDisallowedTests/'<>c'::'<>9'
    IL_000e:  ldftn      instance void SpanDisallowedTests/'<>c'::'<GenericClassInstanceSpanTest>b__6_0'()
    IL_0014:  newobj     instance void [System.Runtime]System.Action::.ctor(object,
                                                                            native int)
    IL_0019:  dup
    IL_001a:  stsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__6_0'
    IL_001f:  call       void Assert::Throws<class [System.Runtime]System.TypeLoadException>(class [System.Runtime]System.Action)
    IL_0024:  ret
  } // end of method SpanDisallowedTests::GenericClassInstanceSpanTest

  .method private hidebysig static void  GenericClassInstanceSpanTestCore() cil managed noinlining
  {
    // Code size       32 (0x20)
    .maxstack  5
    .locals init (class SpanDisallowedTests/GenericClassInstanceSpanTest_SpanContainer`1<valuetype [System.Memory]System.Span`1<int32>> V_0)
    IL_0000:  newobj     instance void class SpanDisallowedTests/GenericClassInstanceSpanTest_SpanContainer`1<valuetype [System.Memory]System.Span`1<int32>>::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldc.i4.2
    IL_0008:  newarr     [System.Runtime]System.Int32
    IL_000d:  dup
    IL_000e:  ldc.i4.0
    IL_000f:  ldc.i4.1
    IL_0010:  stelem.i4
    IL_0011:  dup
    IL_0012:  ldc.i4.1
    IL_0013:  ldc.i4.2
    IL_0014:  stelem.i4
    IL_0015:  newobj     instance void valuetype [System.Memory]System.Span`1<int32>::.ctor(!0[])
    IL_001a:  stfld      !0 class SpanDisallowedTests/GenericClassInstanceSpanTest_SpanContainer`1<valuetype [System.Memory]System.Span`1<int32>>::_t
    IL_001f:  ret
  } // end of method SpanDisallowedTests::GenericClassInstanceSpanTestCore

  .method public hidebysig static void  GenericInterfaceOfSpanTest() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  8
    IL_0000:  ldsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__9_0'
    IL_0005:  dup
    IL_0006:  brtrue.s   IL_001f

    IL_0008:  pop
    IL_0009:  ldsfld     class SpanDisallowedTests/'<>c' SpanDisallowedTests/'<>c'::'<>9'
    IL_000e:  ldftn      instance void SpanDisallowedTests/'<>c'::'<GenericInterfaceOfSpanTest>b__9_0'()
    IL_0014:  newobj     instance void [System.Runtime]System.Action::.ctor(object,
                                                                            native int)
    IL_0019:  dup
    IL_001a:  stsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__9_0'
    IL_001f:  call       void Assert::Throws<class [System.Runtime]System.TypeLoadException>(class [System.Runtime]System.Action)
    IL_0024:  ret
  } // end of method SpanDisallowedTests::GenericInterfaceOfSpanTest

  .method private hidebysig static void  GenericInterfaceOfSpanTestCore() cil managed noinlining
  {
    // Code size       16 (0x10)
    .maxstack  8
    IL_0000:  ldtoken    class SpanDisallowedTests/GenericInterfaceOfSpanTest_ISpanContainer`1<valuetype [System.Memory]System.Span`1<int32>>
    IL_0005:  call       class [System.Runtime]System.Type [System.Runtime]System.Type::GetTypeFromHandle(valuetype [System.Runtime]System.RuntimeTypeHandle)
    IL_000a:  call       void Assert::NotNull(object)
    IL_000f:  ret
  } // end of method SpanDisallowedTests::GenericInterfaceOfSpanTestCore

  .method public hidebysig static void  GenericStructInstanceSpanTest() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  8
    IL_0000:  ldsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__12_0'
    IL_0005:  dup
    IL_0006:  brtrue.s   IL_001f

    IL_0008:  pop
    IL_0009:  ldsfld     class SpanDisallowedTests/'<>c' SpanDisallowedTests/'<>c'::'<>9'
    IL_000e:  ldftn      instance void SpanDisallowedTests/'<>c'::'<GenericStructInstanceSpanTest>b__12_0'()
    IL_0014:  newobj     instance void [System.Runtime]System.Action::.ctor(object,
                                                                            native int)
    IL_0019:  dup
    IL_001a:  stsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__12_0'
    IL_001f:  call       void Assert::Throws<class [System.Runtime]System.TypeLoadException>(class [System.Runtime]System.Action)
    IL_0024:  ret
  } // end of method SpanDisallowedTests::GenericStructInstanceSpanTest

  .method private hidebysig static void  GenericStructInstanceSpanTestCore() cil managed noinlining
  {
    // Code size       35 (0x23)
    .maxstack  5
    .locals init (valuetype SpanDisallowedTests/GenericStructInstanceSpanTest_SpanContainer`1<valuetype [System.Memory]System.Span`1<int32>> V_0)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    valuetype SpanDisallowedTests/GenericStructInstanceSpanTest_SpanContainer`1<valuetype [System.Memory]System.Span`1<int32>>
    IL_0008:  ldloca.s   V_0
    IL_000a:  ldc.i4.2
    IL_000b:  newarr     [System.Runtime]System.Int32
    IL_0010:  dup
    IL_0011:  ldc.i4.0
    IL_0012:  ldc.i4.1
    IL_0013:  stelem.i4
    IL_0014:  dup
    IL_0015:  ldc.i4.1
    IL_0016:  ldc.i4.2
    IL_0017:  stelem.i4
    IL_0018:  newobj     instance void valuetype [System.Memory]System.Span`1<int32>::.ctor(!0[])
    IL_001d:  stfld      !0 valuetype SpanDisallowedTests/GenericStructInstanceSpanTest_SpanContainer`1<valuetype [System.Memory]System.Span`1<int32>>::_t
    IL_0022:  ret
  } // end of method SpanDisallowedTests::GenericStructInstanceSpanTestCore

  .method public hidebysig static void  GenericDelegateOfSpanTest() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  8
    IL_0000:  ldsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__15_0'
    IL_0005:  dup
    IL_0006:  brtrue.s   IL_001f

    IL_0008:  pop
    IL_0009:  ldsfld     class SpanDisallowedTests/'<>c' SpanDisallowedTests/'<>c'::'<>9'
    IL_000e:  ldftn      instance void SpanDisallowedTests/'<>c'::'<GenericDelegateOfSpanTest>b__15_0'()
    IL_0014:  newobj     instance void [System.Runtime]System.Action::.ctor(object,
                                                                            native int)
    IL_0019:  dup
    IL_001a:  stsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__15_0'
    IL_001f:  call       void Assert::Throws<class [System.Runtime]System.TypeLoadException>(class [System.Runtime]System.Action)
    IL_0024:  ret
  } // end of method SpanDisallowedTests::GenericDelegateOfSpanTest

  .method public hidebysig static void  GenericDelegateOfSpanTestCore() cil managed noinlining
  {
    // Code size       16 (0x10)
    .maxstack  8
    IL_0000:  ldtoken    class SpanDisallowedTests/GenericDelegateOfSpan_Delegate`1<valuetype [System.Memory]System.Span`1<int32>>
    IL_0005:  call       class [System.Runtime]System.Type [System.Runtime]System.Type::GetTypeFromHandle(valuetype [System.Runtime]System.RuntimeTypeHandle)
    IL_000a:  call       void Assert::NotNull(object)
    IL_000f:  ret
  } // end of method SpanDisallowedTests::GenericDelegateOfSpanTestCore

  .method public hidebysig static void  ArrayOfSpanTest() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  8
    IL_0000:  ldsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__18_0'
    IL_0005:  dup
    IL_0006:  brtrue.s   IL_001f

    IL_0008:  pop
    IL_0009:  ldsfld     class SpanDisallowedTests/'<>c' SpanDisallowedTests/'<>c'::'<>9'
    IL_000e:  ldftn      instance void SpanDisallowedTests/'<>c'::'<ArrayOfSpanTest>b__18_0'()
    IL_0014:  newobj     instance void [System.Runtime]System.Action::.ctor(object,
                                                                            native int)
    IL_0019:  dup
    IL_001a:  stsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__18_0'
    IL_001f:  call       void Assert::Throws<class [System.Runtime]System.TypeLoadException>(class [System.Runtime]System.Action)
    IL_0024:  ret
  } // end of method SpanDisallowedTests::ArrayOfSpanTest

  .method private hidebysig static void  ArrayOfSpanTestCore() cil managed noinlining
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  ldc.i4.2
    IL_0001:  newarr     valuetype [System.Memory]System.Span`1<int32>
    IL_0006:  call       void Assert::NotNull(object)
    IL_000b:  ret
  } // end of method SpanDisallowedTests::ArrayOfSpanTestCore

  .method public hidebysig static void  BoxSpanTest() cil managed
  {
    // Code size       37 (0x25)
    .maxstack  8
    IL_0000:  ldsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__20_0'
    IL_0005:  dup
    IL_0006:  brtrue.s   IL_001f

    IL_0008:  pop
    IL_0009:  ldsfld     class SpanDisallowedTests/'<>c' SpanDisallowedTests/'<>c'::'<>9'
    IL_000e:  ldftn      instance void SpanDisallowedTests/'<>c'::'<BoxSpanTest>b__20_0'()
    IL_0014:  newobj     instance void [System.Runtime]System.Action::.ctor(object,
                                                                            native int)
    IL_0019:  dup
    IL_001a:  stsfld     class [System.Runtime]System.Action SpanDisallowedTests/'<>c'::'<>9__20_0'
    IL_001f:  call       void Assert::Throws<class [System.Runtime]System.InvalidProgramException>(class [System.Runtime]System.Action)
    IL_0024:  ret
  } // end of method SpanDisallowedTests::BoxSpanTest

  .method private hidebysig static void  BoxSpanTestCore() cil managed noinlining
  {
    // Code size       35 (0x23)
    .maxstack  5
    .locals init (valuetype [System.Memory]System.Span`1<int32> V_0,
             object V_1)
    IL_0000:  ldloca.s   V_0
    IL_0002:  ldc.i4.2
    IL_0003:  newarr     [System.Runtime]System.Int32
    IL_0008:  dup
    IL_0009:  ldc.i4.0
    IL_000a:  ldc.i4.1
    IL_000b:  stelem.i4
    IL_000c:  dup
    IL_000d:  ldc.i4.1
    IL_000e:  ldc.i4.2
    IL_000f:  stelem.i4
    IL_0010:  call       instance void valuetype [System.Memory]System.Span`1<int32>::.ctor(!0[])
    IL_0015:  ldloc.0
    IL_0016:  box        valuetype [System.Memory]System.Span`1<int32>
    IL_001b:  stloc.1
    IL_001c:  ldloc.1
    IL_001d:  call       void Assert::NotNull(object)
    IL_0022:  ret
  } // end of method SpanDisallowedTests::BoxSpanTestCore

} // end of class SpanDisallowedTests

.class public abstract auto ansi sealed beforefieldinit Assert
       extends [System.Runtime]System.Object
{
  .method public hidebysig static void  NotNull(object 'value') cil managed
  {
    // Code size       35 (0x23)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  brfalse.s  IL_0004

    IL_0003:  ret

    IL_0004:  ldstr      "Expected null, got value of type '{1}'."
    IL_0009:  ldc.i4.1
    IL_000a:  newarr     [System.Runtime]System.Object
    IL_000f:  dup
    IL_0010:  ldc.i4.0
    IL_0011:  ldarg.0
    IL_0012:  callvirt   instance class [System.Runtime]System.Type [System.Runtime]System.Object::GetType()
    IL_0017:  callvirt   instance string [System.Runtime]System.Type::get_FullName()
    IL_001c:  stelem.ref
    IL_001d:  newobj     instance void AssertionFailureException::.ctor(string,
                                                                        object[])
    IL_0022:  throw
  } // end of method Assert::NotNull

  .method public hidebysig static void  Throws<([System.Runtime]System.Exception) T>(class [System.Runtime]System.Action action) cil managed
  {
    // Code size       129 (0x81)
    .maxstack  6
    .locals init (!!T V_0,
             class [System.Runtime]System.Exception V_1)
    .try
    {
      IL_0000:  ldarg.0
      IL_0001:  callvirt   instance void [System.Runtime]System.Action::Invoke()
      IL_0006:  leave.s    IL_005d

    }  // end .try
    filter
    {
      IL_0008:  isinst     !!T
      IL_000d:  dup
      IL_000e:  brtrue.s   IL_0014

      IL_0010:  pop
      IL_0011:  ldc.i4.0
      IL_0012:  br.s       IL_0037

      IL_0014:  unbox.any  !!T
      IL_0019:  stloc.0
      IL_001a:  ldloc.0
      IL_001b:  box        !!T
      IL_0020:  callvirt   instance class [System.Runtime]System.Type [System.Runtime]System.Exception::GetType()
      IL_0025:  ldtoken    !!T
      IL_002a:  call       class [System.Runtime]System.Type [System.Runtime]System.Type::GetTypeFromHandle(valuetype [System.Runtime]System.RuntimeTypeHandle)
      IL_002f:  call       bool [System.Runtime]System.Type::op_Equality(class [System.Runtime]System.Type,
                                                                         class [System.Runtime]System.Type)
      IL_0034:  ldc.i4.0
      IL_0035:  cgt.un
      IL_0037:  endfilter
    }  // end filter
    {  // handler
      IL_0039:  pop
      IL_003a:  leave.s    IL_0080

    }  // end handler
    catch [System.Runtime]System.Exception 
    {
      IL_003c:  stloc.1
      IL_003d:  ldloc.1
      IL_003e:  ldstr      "Expected exception of type '{0}', got '{1}'."
      IL_0043:  ldc.i4.1
      IL_0044:  newarr     [System.Runtime]System.Object
      IL_0049:  dup
      IL_004a:  ldc.i4.0
      IL_004b:  ldloc.1
      IL_004c:  callvirt   instance class [System.Runtime]System.Type [System.Runtime]System.Exception::GetType()
      IL_0051:  callvirt   instance string [System.Runtime]System.Type::get_FullName()
      IL_0056:  stelem.ref
      IL_0057:  newobj     instance void AssertionFailureException::.ctor(class [System.Runtime]System.Exception,
                                                                          string,
                                                                          object[])
      IL_005c:  throw

    }  // end handler
    IL_005d:  ldstr      "Exception was not thrown, expected '{0}'."
    IL_0062:  ldc.i4.1
    IL_0063:  newarr     [System.Runtime]System.Object
    IL_0068:  dup
    IL_0069:  ldc.i4.0
    IL_006a:  ldtoken    !!T
    IL_006f:  call       class [System.Runtime]System.Type [System.Runtime]System.Type::GetTypeFromHandle(valuetype [System.Runtime]System.RuntimeTypeHandle)
    IL_0074:  callvirt   instance string [System.Runtime]System.Type::get_FullName()
    IL_0079:  stelem.ref
    IL_007a:  newobj     instance void AssertionFailureException::.ctor(string,
                                                                        object[])
    IL_007f:  throw

    IL_0080:  ret
  } // end of method Assert::Throws

} // end of class Assert

.class public auto ansi beforefieldinit AssertionFailureException
       extends [System.Runtime]System.Exception
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(string format,
                               object[] args) cil managed
  {
    .param [2]
    .custom instance void [System.Runtime]System.ParamArrayAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldnull
    IL_0002:  ldarg.1
    IL_0003:  ldarg.2
    IL_0004:  call       instance void AssertionFailureException::.ctor(class [System.Runtime]System.Exception,
                                                                        string,
                                                                        object[])
    IL_0009:  ret
  } // end of method AssertionFailureException::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [System.Runtime]System.Exception innerException,
                               string format,
                               object[] args) cil managed
  {
    .param [3]
    .custom instance void [System.Runtime]System.ParamArrayAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       20 (0x14)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       class [System.Runtime]System.Globalization.CultureInfo [System.Runtime]System.Globalization.CultureInfo::get_InvariantCulture()
    IL_0006:  ldarg.2
    IL_0007:  ldarg.3
    IL_0008:  call       string [System.Runtime]System.String::Format(class [System.Runtime]System.IFormatProvider,
                                                                      string,
                                                                      object[])
    IL_000d:  ldarg.1
    IL_000e:  call       instance void [System.Runtime]System.Exception::.ctor(string,
                                                                               class [System.Runtime]System.Exception)
    IL_0013:  ret
  } // end of method AssertionFailureException::.ctor

} // end of class AssertionFailureException


// =============================================================

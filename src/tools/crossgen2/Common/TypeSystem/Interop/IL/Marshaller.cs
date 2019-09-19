// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Internal.IL.Stubs;
using Internal.IL;

using Debug = System.Diagnostics.Debug;
using ILLocalVariable = Internal.IL.Stubs.ILLocalVariable;

namespace Internal.TypeSystem.Interop
{
    enum MarshallerKind
    {
        Unknown,
        BlittableValue,
        Array,
        BlittableArray,
        Bool,   // 4 byte bool
        CBool,  // 1 byte bool
        Enum,
        AnsiChar,  // Marshal char (Unicode 16bits) for byte (Ansi 8bits)
        UnicodeChar,
        AnsiCharArray,
        ByValArray,
        ByValAnsiCharArray, // Particular case of ByValArray because the conversion between wide Char and Byte need special treatment.
        AnsiString,
        UnicodeString,
        UTF8String,
        ByValAnsiString,
        ByValUnicodeString,
        AnsiStringBuilder,
        UnicodeStringBuilder,
        FunctionPointer,
        SafeHandle,
        CriticalHandle,
        HandleRef,
        VoidReturn,
        Variant,
        Object,
        OleDateTime,
        Decimal,
        Guid,
        Struct,
        BlittableStruct,
        BlittableStructPtr,   // Additional indirection on top of blittable struct. Used by MarshalAs(LpStruct)
        LayoutClass,
        LayoutClassPtr,
        AsAnyA,
        AsAnyW,
        Invalid
    }
    public enum MarshalDirection
    {
        Forward,    // safe-to-unsafe / managed-to-native
        Reverse,    // unsafe-to-safe / native-to-managed
    }

    public enum MarshallerType
    {
        Argument,
        Element,
        Field
    }

    // Each type of marshaller knows how to generate the marshalling code for the argument it marshals.
    // Marshallers contain method related marshalling information (which is common to all the Marshallers)
    // and also argument specific marshalling informaiton
    abstract partial class Marshaller
    {
        #region Instance state information
        public TypeSystemContext Context;
#if !READYTORUN
        public InteropStateManager InteropStateManager;
#endif
        public MarshallerKind MarshallerKind;
        public MarshallerType MarshallerType;
        public MarshalAsDescriptor MarshalAsDescriptor;
        public MarshallerKind ElementMarshallerKind;
        public int Index;
        public TypeDesc ManagedType;
        public TypeDesc ManagedParameterType;
        public PInvokeFlags PInvokeFlags;
        protected Marshaller[] Marshallers;
        private TypeDesc _nativeType;
        private TypeDesc _nativeParamType;

        /// <summary>
        /// Native Type of the value being marshalled
        /// For by-ref scenarios (ref T), Native Type is T
        /// </summary>
        public TypeDesc NativeType
        {
            get
            {
                if (_nativeType == null)
                {
                    _nativeType = MarshalHelpers.GetNativeTypeFromMarshallerKind(
                        ManagedType,
                        MarshallerKind,
                        ElementMarshallerKind,
#if !READYTORUN
                        InteropStateManager,
#endif
                        MarshalAsDescriptor);
                    Debug.Assert(_nativeType != null);
                }

                return _nativeType;
            }
        }

        /// <summary>
        /// NativeType appears in function parameters
        /// For by-ref scenarios (ref T), NativeParameterType is T*
        /// </summary>
        public TypeDesc NativeParameterType
        {
            get
            {
                if (_nativeParamType == null)
                {
                    TypeDesc nativeParamType = NativeType;
                    if (IsNativeByRef)
                        nativeParamType = nativeParamType.MakePointerType();
                    _nativeParamType = nativeParamType;
                }

                return _nativeParamType;
            }
        }

        /// <summary>
        ///  Indicates whether cleanup is necessay if this marshaller is used
        ///  as an element of an array marshaller
        /// </summary>
        internal virtual bool CleanupRequired
        {
            get
            {
                return false;
            }
        }

        public bool In;
        public bool Out;
        public bool Return;
        public bool IsManagedByRef;                     // Whether managed argument is passed by ref
        public bool IsNativeByRef;                      // Whether native argument is passed by byref
                                                        // There are special cases (such as LpStruct, and class) that 
                                                        // isNativeByRef != IsManagedByRef
        public MarshalDirection MarshalDirection;
        protected PInvokeILCodeStreams _ilCodeStreams;
        protected Home _managedHome;
        protected Home _nativeHome;
#endregion

        enum HomeType
        {
            Arg,
            Local,
            ByRefArg,
            ByRefLocal
        }

        /// <summary>
        /// Abstraction for handling by-ref and non-by-ref locals/arguments
        /// </summary>
        internal class Home
        {
            public Home(ILLocalVariable var, TypeDesc type, bool isByRef)
            {
                _homeType = isByRef ? HomeType.ByRefLocal : HomeType.Local;
                _type = type;
                _var = var;
            }

            public Home(int argIndex, TypeDesc type, bool isByRef)
            {
                _homeType = isByRef ? HomeType.ByRefArg : HomeType.Arg;
                _type = type;
                _argIndex = argIndex;
            }

            public void LoadValue(ILCodeStream stream)
            {
                switch (_homeType)
                {
                    case HomeType.Arg:
                        stream.EmitLdArg(_argIndex);
                        break;
                    case HomeType.ByRefArg:
                        stream.EmitLdArg(_argIndex);
                        stream.EmitLdInd(_type);
                        break;
                    case HomeType.Local:
                        stream.EmitLdLoc(_var);
                        break;
                    case HomeType.ByRefLocal:
                        stream.EmitLdLoc(_var);
                        stream.EmitLdInd(_type);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            public void LoadAddr(ILCodeStream stream)
            {
                switch (_homeType)
                {
                    case HomeType.Arg:
                        stream.EmitLdArga(_argIndex);
                        break;
                    case HomeType.ByRefArg:
                        stream.EmitLdArg(_argIndex);
                        break;
                    case HomeType.Local:
                        stream.EmitLdLoca(_var);
                        break;
                    case HomeType.ByRefLocal:
                        stream.EmitLdLoc(_var);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            public void StoreValue(ILCodeStream stream)
            {
                switch (_homeType)
                {
                    case HomeType.Arg:
                        Debug.Fail("Unexpectting setting value on non-byref arg");
                        break;
                    case HomeType.Local:
                        stream.EmitStLoc(_var);
                        break;
                    default:
                        // Storing by-ref arg/local is not supported because StInd require
                        // address to be pushed first. Instead we need to introduce a non-byref 
                        // local and propagate value as needed for by-ref arguments
                        Debug.Assert(false);
                        break;
                }
            }

            HomeType _homeType;
            TypeDesc _type;
            ILLocalVariable _var;
            int _argIndex;
        }

#region Creation of marshallers

        /// <summary>
        /// Protected ctor
        /// Only Marshaller.CreateMarshaller can create a marshaller
        /// </summary>
        protected Marshaller()
        {
        }

        /// <summary>
        /// Create a marshaller
        /// </summary>
        /// <param name="parameterType">type of the parameter to marshal</param>
        /// <returns>The created Marshaller</returns>
        public static Marshaller CreateMarshaller(TypeDesc parameterType,
            MarshallerType marshallerType,
            MarshalAsDescriptor marshalAs,
            MarshalDirection direction,
            Marshaller[] marshallers,
#if !READYTORUN
            InteropStateManager interopStateManager,
#endif
            int index,
            PInvokeFlags flags,
            bool isIn,
            bool isOut,
            bool isReturn)
        {
            MarshallerKind elementMarshallerKind;
            MarshallerKind marshallerKind = MarshalHelpers.GetMarshallerKind(parameterType,
                                                marshalAs,
                                                isReturn,
                                                flags.CharSet == CharSet.Ansi,
                                                marshallerType,
                                                out elementMarshallerKind);

            TypeSystemContext context = parameterType.Context;
            // Create the marshaller based on MarshallerKind
            Marshaller marshaller = CreateMarshaller(marshallerKind);
            marshaller.Context = context;
#if !READYTORUN
            marshaller.InteropStateManager = interopStateManager;
#endif
            marshaller.MarshallerKind = marshallerKind;
            marshaller.MarshallerType = marshallerType;
            marshaller.ElementMarshallerKind = elementMarshallerKind;
            marshaller.ManagedParameterType = parameterType;
            marshaller.ManagedType = parameterType.IsByRef ? parameterType.GetParameterType() : parameterType;
            marshaller.Return = isReturn;
            marshaller.IsManagedByRef = parameterType.IsByRef;
            marshaller.IsNativeByRef = marshaller.IsManagedByRef /* || isRetVal || LpStruct /etc */;
            marshaller.In = isIn;
            marshaller.MarshalDirection = direction;
            marshaller.MarshalAsDescriptor = marshalAs;
            marshaller.Marshallers = marshallers;
            marshaller.Index = index;
            marshaller.PInvokeFlags = flags;

            //
            // Desktop ignores [Out] on marshaling scenarios where they don't make sense
            //
            if (isOut)
            {
                // Passing as [Out] by ref is always valid. 
                if (!marshaller.IsManagedByRef)
                {
                    // Ignore [Out] for ValueType, string and pointers
                    if (parameterType.IsValueType || parameterType.IsString || parameterType.IsPointer || parameterType.IsFunctionPointer)
                    {
                        isOut = false;
                    }
                }
            }
            marshaller.Out = isOut;

            if (!marshaller.In && !marshaller.Out)
            {
                //
                // Rules for in/out
                // 1. ByRef args: [in]/[out] implied by default
                // 2. StringBuilder: [in, out] by default
                // 3. non-ByRef args: [In] is implied if no [In]/[Out] is specified
                //
                if (marshaller.IsManagedByRef)
                {
                    marshaller.In = true;
                    marshaller.Out = true;
                }
                else if (InteropTypes.IsStringBuilder(context, parameterType))
                {
                    marshaller.In = true;
                    marshaller.Out = true;
                }
                else
                {
                    marshaller.In = true;
                }
            }

            // For unicodestring/ansistring, ignore out when it's in
            if (!marshaller.IsManagedByRef && marshaller.In)
            {
                if (marshaller.MarshallerKind == MarshallerKind.AnsiString || marshaller.MarshallerKind == MarshallerKind.UnicodeString)
                    marshaller.Out = false;
            }

            return marshaller;
        }

        public bool IsMarshallingRequired()
        {
            if (Out)
                return true;

            switch (MarshallerKind)
            {
                case MarshallerKind.Enum:
                case MarshallerKind.BlittableValue:
                case MarshallerKind.BlittableStruct:
                case MarshallerKind.UnicodeChar:
                case MarshallerKind.VoidReturn:
                    return false;
            }
            return true;
        }
#endregion

        public virtual void EmitMarshallingIL(PInvokeILCodeStreams pInvokeILCodeStreams)
        {
            _ilCodeStreams = pInvokeILCodeStreams;

            switch (MarshallerType)
            {
                case MarshallerType.Argument: EmitArgumentMarshallingIL(); return;
                case MarshallerType.Element: EmitElementMarshallingIL(); return;
                case MarshallerType.Field: EmitFieldMarshallingIL(); return;
            }
        }

        private void EmitArgumentMarshallingIL()
        {
            switch (MarshalDirection)
            {
                case MarshalDirection.Forward: EmitForwardArgumentMarshallingIL(); return;
                case MarshalDirection.Reverse: EmitReverseArgumentMarshallingIL(); return;
            }
        }

        private void EmitElementMarshallingIL()
        {
            switch (MarshalDirection)
            {
                case MarshalDirection.Forward: EmitForwardElementMarshallingIL(); return;
                case MarshalDirection.Reverse: EmitReverseElementMarshallingIL(); return;
            }
        }

        private void EmitFieldMarshallingIL()
        {
            switch (MarshalDirection)
            {
                case MarshalDirection.Forward: EmitForwardFieldMarshallingIL(); return;
                case MarshalDirection.Reverse: EmitReverseFieldMarshallingIL(); return;
            }
        }

        protected virtual void EmitForwardArgumentMarshallingIL()
        {
            if (Return)
            {
                EmitMarshalReturnValueManagedToNative();
            }
            else
            {
                EmitMarshalArgumentManagedToNative();
            }
        }

        protected virtual void EmitReverseArgumentMarshallingIL()
        {
            if (Return)
            {
                EmitMarshalReturnValueNativeToManaged();
            }
            else
            {
                EmitMarshalArgumentNativeToManaged();
            }
        }

        protected virtual void EmitForwardElementMarshallingIL()
        {
            if (In)
                EmitMarshalElementManagedToNative();
            else
                EmitMarshalElementNativeToManaged();
        }

        protected virtual void EmitReverseElementMarshallingIL()
        {
            if (In)
                EmitMarshalElementNativeToManaged();
            else
                EmitMarshalElementManagedToNative();
        }

        protected virtual void EmitForwardFieldMarshallingIL()
        {
            if (In)
                EmitMarshalFieldManagedToNative();
            else
                EmitMarshalFieldNativeToManaged();
        }

        protected virtual void EmitReverseFieldMarshallingIL()
        {
            if (In)
                EmitMarshalFieldNativeToManaged();
            else
                EmitMarshalFieldManagedToNative();
        }


        protected virtual void EmitMarshalReturnValueManagedToNative()
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            SetupArgumentsForReturnValueMarshalling();

            StoreNativeValue(_ilCodeStreams.ReturnValueMarshallingCodeStream);

            AllocAndTransformNativeToManaged(_ilCodeStreams.ReturnValueMarshallingCodeStream);
        }

        public virtual void LoadReturnValue(ILCodeStream codeStream)
        {
            Debug.Assert(Return);

            switch (MarshalDirection)
            {
                case MarshalDirection.Forward: LoadManagedValue(codeStream); return;
                case MarshalDirection.Reverse: LoadNativeValue(codeStream); return;
            }
        }

        protected virtual void SetupArguments()
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;

            if (MarshalDirection == MarshalDirection.Forward)
            {
                // Due to StInd order (address, value), we can't do the following:
                //   LoadValue
                //   StoreManagedValue (LdArg + StInd)
                // The way to work around this is to put it in a local
                if (IsManagedByRef)
                    _managedHome = new Home(emitter.NewLocal(ManagedType), ManagedType, false);
                else
                    _managedHome = new Home(Index - 1, ManagedType, false);
                _nativeHome = new Home(emitter.NewLocal(NativeType), NativeType, isByRef: false);
            }
            else
            {
                _managedHome = new Home(emitter.NewLocal(ManagedType), ManagedType, isByRef: false);
                if (IsNativeByRef)
                    _nativeHome = new Home(emitter.NewLocal(NativeType), NativeType, isByRef: false);
                else
                    _nativeHome = new Home(Index - 1, NativeType, isByRef: false);
            }
        }

        protected virtual void SetupArgumentsForElementMarshalling()
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;

            _managedHome = new Home(emitter.NewLocal(ManagedType), ManagedType, isByRef: false);
            _nativeHome = new Home(emitter.NewLocal(NativeType), NativeType, isByRef: false);
        }

        protected virtual void SetupArgumentsForFieldMarshalling()
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;

            //
            // these are temporary locals for propagating value
            //
            _managedHome = new Home(emitter.NewLocal(ManagedType), ManagedType, isByRef: false);
            _nativeHome = new Home(emitter.NewLocal(NativeType), NativeType, isByRef: false);
        }

        protected virtual void SetupArgumentsForReturnValueMarshalling()
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;

            _managedHome = new Home(emitter.NewLocal(ManagedType), ManagedType, isByRef: false);
            _nativeHome = new Home(emitter.NewLocal(NativeType), NativeType, isByRef: false);
        }

        protected void LoadManagedValue(ILCodeStream stream)
        {
            _managedHome.LoadValue(stream);
        }

        protected void LoadManagedAddr(ILCodeStream stream)
        {
            _managedHome.LoadAddr(stream);
        }

        /// <summary>
        /// Loads the argument to be passed to managed functions
        /// In by-ref scenarios (ref T), it is &T
        /// </summary>
        protected void LoadManagedArg(ILCodeStream stream)
        {
            if (IsManagedByRef)
                _managedHome.LoadAddr(stream);
            else
                _managedHome.LoadValue(stream);
        }

        protected void StoreManagedValue(ILCodeStream stream)
        {
            _managedHome.StoreValue(stream);
        }

        protected void LoadNativeValue(ILCodeStream stream)
        {
            _nativeHome.LoadValue(stream);
        }

        /// <summary>
        /// Loads the argument to be passed to native functions
        /// In by-ref scenarios (ref T), it is T*
        /// </summary>
        protected void LoadNativeArg(ILCodeStream stream)
        {
            if (IsNativeByRef)
                _nativeHome.LoadAddr(stream);
            else
                _nativeHome.LoadValue(stream);
        }

        protected void LoadNativeAddr(ILCodeStream stream)
        {
            _nativeHome.LoadAddr(stream);
        }

        protected void StoreNativeValue(ILCodeStream stream)
        {
            _nativeHome.StoreValue(stream);
        }


        /// <summary>
        /// Propagate by-ref arg to corresponding local
        /// We can't load value + ldarg + ldind in the expected order, so 
        /// we had to use a non-by-ref local and manually propagate the value
        /// </summary>
        protected void PropagateFromByRefArg(ILCodeStream stream, Home home)
        {
            stream.EmitLdArg(Index - 1);
            stream.EmitLdInd(ManagedType);
            home.StoreValue(stream);
        }

        /// <summary>
        /// Propagate local to corresponding by-ref arg
        /// We can't load value + ldarg + ldind in the expected order, so 
        /// we had to use a non-by-ref local and manually propagate the value
        /// </summary>
        protected void PropagateToByRefArg(ILCodeStream stream, Home home)
        {
            stream.EmitLdArg(Index - 1);
            home.LoadValue(stream);
            stream.EmitStInd(ManagedType);
        }

        protected virtual void EmitMarshalArgumentManagedToNative()
        {
            SetupArguments();

            if (IsManagedByRef && In)
            {
                // Propagate byref arg to local
                PropagateFromByRefArg(_ilCodeStreams.MarshallingCodeStream, _managedHome);
            }

            //
            // marshal
            //
            if (IsManagedByRef && !In)
            {
                ReInitNativeTransform(_ilCodeStreams.MarshallingCodeStream);
            }
            else
            {
                AllocAndTransformManagedToNative(_ilCodeStreams.MarshallingCodeStream);
            }

            LoadNativeArg(_ilCodeStreams.CallsiteSetupCodeStream);

            //
            // unmarshal
            //
            if (Out)
            {
                if (In)
                {
                    ClearManagedTransform(_ilCodeStreams.UnmarshallingCodestream);
                }

                if (IsManagedByRef && !In)
                {
                    AllocNativeToManaged(_ilCodeStreams.UnmarshallingCodestream);
                }

                TransformNativeToManaged(_ilCodeStreams.UnmarshallingCodestream);

                if (IsManagedByRef)
                {
                    // Propagate back to byref arguments
                    PropagateToByRefArg(_ilCodeStreams.UnmarshallingCodestream, _managedHome);
                }
            }

            // TODO This should be in finally block
            // https://github.com/dotnet/corert/issues/6075
            EmitCleanupManaged(_ilCodeStreams.UnmarshallingCodestream);
        }

        /// <summary>
        /// Reads managed parameter from _vManaged and writes the marshalled parameter in _vNative
        /// </summary>
        protected virtual void AllocAndTransformManagedToNative(ILCodeStream codeStream)
        {
            AllocManagedToNative(codeStream);
            if (In)
            {
                TransformManagedToNative(codeStream);
            }
        }

        protected virtual void AllocAndTransformNativeToManaged(ILCodeStream codeStream)
        {
            AllocNativeToManaged(codeStream);
            TransformNativeToManaged(codeStream);
        }

        protected virtual void AllocManagedToNative(ILCodeStream codeStream)
        {
        }
        protected virtual void TransformManagedToNative(ILCodeStream codeStream)
        {
            LoadManagedValue(codeStream);
            StoreNativeValue(codeStream);
        }

        protected virtual void ClearManagedTransform(ILCodeStream codeStream)
        {
        }
        protected virtual void AllocNativeToManaged(ILCodeStream codeStream)
        {
        }

        protected virtual void TransformNativeToManaged(ILCodeStream codeStream)
        {
            LoadNativeValue(codeStream);
            StoreManagedValue(codeStream);
        }

        protected virtual void EmitCleanupManaged(ILCodeStream codeStream)
        {
        }

        protected virtual void EmitMarshalReturnValueNativeToManaged()
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            SetupArgumentsForReturnValueMarshalling();

            StoreManagedValue(_ilCodeStreams.ReturnValueMarshallingCodeStream);

            AllocAndTransformManagedToNative(_ilCodeStreams.ReturnValueMarshallingCodeStream);
        }

        protected virtual void EmitMarshalArgumentNativeToManaged()
        {
            SetupArguments();

            if (IsNativeByRef && In)
            {
                // Propagate byref arg to local
                PropagateFromByRefArg(_ilCodeStreams.MarshallingCodeStream, _nativeHome);
            }

            if (IsNativeByRef && !In)
            {
                ReInitManagedTransform(_ilCodeStreams.MarshallingCodeStream);
            }
            else
            {
                AllocAndTransformNativeToManaged(_ilCodeStreams.MarshallingCodeStream);
            }

            LoadManagedArg(_ilCodeStreams.CallsiteSetupCodeStream);

            if (Out)
            {
                if (IsNativeByRef)
                {
                    AllocManagedToNative(_ilCodeStreams.UnmarshallingCodestream);
                }

                TransformManagedToNative(_ilCodeStreams.UnmarshallingCodestream);

                if (IsNativeByRef)
                {
                    // Propagate back to byref arguments
                    PropagateToByRefArg(_ilCodeStreams.UnmarshallingCodestream, _nativeHome);
                }
            }
        }

        protected virtual void EmitMarshalElementManagedToNative()
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            ILCodeStream codeStream = _ilCodeStreams.MarshallingCodeStream;
            Debug.Assert(codeStream != null);

            SetupArgumentsForElementMarshalling();

            StoreManagedValue(codeStream);

            // marshal
            AllocAndTransformManagedToNative(codeStream);

            LoadNativeValue(codeStream);
        }

        protected virtual void EmitMarshalElementNativeToManaged()
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            ILCodeStream codeStream = _ilCodeStreams.MarshallingCodeStream;
            Debug.Assert(codeStream != null);

            SetupArgumentsForElementMarshalling();

            StoreNativeValue(codeStream);

            // unmarshal
            AllocAndTransformNativeToManaged(codeStream);
            LoadManagedValue(codeStream);
        }

        protected virtual void EmitMarshalFieldManagedToNative()
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            ILCodeStream marshallingCodeStream = _ilCodeStreams.MarshallingCodeStream;

            SetupArgumentsForFieldMarshalling();
            //
            // For field marshalling we expect the value of the field is already loaded
            // in the stack. 
            //
            StoreManagedValue(marshallingCodeStream);

            // marshal
            AllocAndTransformManagedToNative(marshallingCodeStream);

            LoadNativeValue(marshallingCodeStream);
        }

        protected virtual void EmitMarshalFieldNativeToManaged()
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            ILCodeStream codeStream = _ilCodeStreams.MarshallingCodeStream;

            SetupArgumentsForFieldMarshalling();

            StoreNativeValue(codeStream);

            // unmarshal
            AllocAndTransformNativeToManaged(codeStream);
            LoadManagedValue(codeStream);
        }

        protected virtual void ReInitManagedTransform(ILCodeStream codeStream)
        {
        }

        protected virtual void ReInitNativeTransform(ILCodeStream codeStream)
        {
        }

        internal virtual void EmitElementCleanup(ILCodeStream codestream, ILEmitter emitter)
        {
        }
    }

    class NotSupportedMarshaller : Marshaller
    {
        public override void EmitMarshallingIL(PInvokeILCodeStreams pInvokeILCodeStreams)
        {
            throw new NotSupportedException();
        }
    }

    class VoidReturnMarshaller : Marshaller
    {
        protected override void EmitMarshalReturnValueManagedToNative()
        {
        }
        protected override void EmitMarshalReturnValueNativeToManaged()
        {
        }
        public override void LoadReturnValue(ILCodeStream codeStream)
        {
            Debug.Assert(Return);
        }
    }

    class BlittableValueMarshaller : Marshaller
    {
        protected override void EmitMarshalArgumentManagedToNative()
        {
            if (IsNativeByRef && MarshalDirection == MarshalDirection.Forward)
            {
                ILCodeStream marshallingCodeStream = _ilCodeStreams.MarshallingCodeStream;
                ILEmitter emitter = _ilCodeStreams.Emitter;
                ILLocalVariable native = emitter.NewLocal(Context.GetWellKnownType(WellKnownType.IntPtr));

                ILLocalVariable vPinnedByRef = emitter.NewLocal(ManagedParameterType, true);
                marshallingCodeStream.EmitLdArg(Index - 1);
                marshallingCodeStream.EmitStLoc(vPinnedByRef);
                marshallingCodeStream.EmitLdLoc(vPinnedByRef);
                marshallingCodeStream.Emit(ILOpcode.conv_i);
                marshallingCodeStream.EmitStLoc(native);
                _ilCodeStreams.CallsiteSetupCodeStream.EmitLdLoc(native);
            }
            else
            {
                _ilCodeStreams.CallsiteSetupCodeStream.EmitLdArg(Index - 1);
            }
        }

        protected override void EmitMarshalArgumentNativeToManaged()
        {
            if (Out)
            {
                base.EmitMarshalArgumentNativeToManaged();
            }
            else
            {
                _ilCodeStreams.CallsiteSetupCodeStream.EmitLdArg(Index - 1);
            }
        }
    }

    class BlittableStructPtrMarshaller : Marshaller
    {
        protected override void TransformManagedToNative(ILCodeStream codeStream)
        {
            if (Out)
            {
                // TODO: https://github.com/dotnet/corert/issues/4466
                throw new NotSupportedException("Marshalling an LPStruct argument not yet implemented");
            }
            else
            {
                LoadManagedAddr(codeStream);
                StoreNativeValue(codeStream);
            }
        }

        protected override void TransformNativeToManaged(ILCodeStream codeStream)
        {
            // TODO: https://github.com/dotnet/corert/issues/4466
            throw new NotSupportedException("Marshalling an LPStruct argument not yet implemented");
        }
    }

    class ArrayMarshaller : Marshaller
    {
        private Marshaller _elementMarshaller;

        protected TypeDesc ManagedElementType
        {
            get
            {
                Debug.Assert(ManagedType is ArrayType);
                var arrayType = (ArrayType)ManagedType;
                return arrayType.ElementType;
            }
        }

        protected TypeDesc NativeElementType
        {
            get
            {
                Debug.Assert(NativeType is PointerType);
                return ((PointerType)NativeType).ParameterType;
            }
        }

        protected Marshaller GetElementMarshaller(MarshalDirection direction)
        {
            if (_elementMarshaller == null)
            {
                _elementMarshaller = CreateMarshaller(ElementMarshallerKind);
                _elementMarshaller.MarshallerKind = ElementMarshallerKind;
                _elementMarshaller.MarshallerType = MarshallerType.Element;
#if !READYTORUN
                _elementMarshaller.InteropStateManager = InteropStateManager;
#endif
                _elementMarshaller.Return = Return;
                _elementMarshaller.Context = Context;
                _elementMarshaller.ManagedType = ManagedElementType;
                _elementMarshaller.MarshalAsDescriptor = MarshalAsDescriptor;
                _elementMarshaller.PInvokeFlags = PInvokeFlags;
            }
            _elementMarshaller.In = (direction == MarshalDirection);
            _elementMarshaller.Out = !In;
            _elementMarshaller.MarshalDirection = MarshalDirection;

            return _elementMarshaller;
        }

        protected virtual void EmitElementCount(ILCodeStream codeStream, MarshalDirection direction)
        {
            if (direction == MarshalDirection.Forward)
            {
                // In forward direction we skip whatever is passed through SizeParamIndex, because the
                // size of the managed array is already known
                LoadManagedValue(codeStream);
                codeStream.Emit(ILOpcode.ldlen);
                codeStream.Emit(ILOpcode.conv_i4);

            }
            else if (MarshalDirection == MarshalDirection.Forward
                    && MarshallerType == MarshallerType.Argument
                    && !Return
                    && !IsManagedByRef)
            {
                EmitElementCount(codeStream, MarshalDirection.Forward);
            }
            else
            {

                uint? sizeParamIndex = MarshalAsDescriptor != null ? MarshalAsDescriptor.SizeParamIndex : null;
                uint? sizeConst = MarshalAsDescriptor != null ? MarshalAsDescriptor.SizeConst : null;

                if (sizeConst.HasValue)
                {
                    codeStream.EmitLdc((int)sizeConst.Value);
                }

                if (sizeParamIndex.HasValue)
                {
                    uint index = sizeParamIndex.Value;

                    if (index < 0 || index >= Marshallers.Length - 1)
                    {
                        throw new InvalidProgramException("Invalid SizeParamIndex, must be between 0 and parameter count");
                    }

                    //zero-th index is for return type
                    index++;
                    var indexType = Marshallers[index].ManagedType;
                    switch (indexType.Category)
                    {
                        case TypeFlags.Byte:
                        case TypeFlags.SByte:
                        case TypeFlags.Int16:
                        case TypeFlags.UInt16:
                        case TypeFlags.Int32:
                        case TypeFlags.UInt32:
                        case TypeFlags.Int64:
                        case TypeFlags.UInt64:
                        case TypeFlags.IntPtr:
                        case TypeFlags.UIntPtr:
                            break;
                        default:
                            throw new InvalidProgramException("Invalid SizeParamIndex, parameter must be  of type int/uint");
                    }

                    // @TODO - We can use LoadManagedValue, but that requires byref arg propagation happen in a special setup stream
                    // otherwise there is an ordering issue
                    codeStream.EmitLdArg(Marshallers[index].Index - 1);
                    if (Marshallers[index].IsManagedByRef)
                        codeStream.EmitLdInd(indexType);

                    if (sizeConst.HasValue)
                        codeStream.Emit(ILOpcode.add);
                }

                if (!sizeConst.HasValue && !sizeParamIndex.HasValue)
                {
                    // if neither sizeConst or sizeParamIndex are specified, default to 1
                    codeStream.EmitLdc(1);
                }
            }
        }

        protected override void AllocManagedToNative(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            ILCodeLabel lNullArray = emitter.NewCodeLabel();

            LoadNativeAddr(codeStream);
            codeStream.Emit(ILOpcode.initobj, emitter.NewToken(NativeType));

            // Check for null array
            LoadManagedValue(codeStream);
            codeStream.Emit(ILOpcode.brfalse, lNullArray);

            // allocate memory
            // nativeParameter = (byte**)CoTaskMemAllocAndZeroMemory((IntPtr)(checked(managedParameter.Length * sizeof(byte*))));

            // loads the number of elements
            EmitElementCount(codeStream, MarshalDirection.Forward);

            TypeDesc nativeElementType = NativeElementType;
            codeStream.Emit(ILOpcode.sizeof_, emitter.NewToken(nativeElementType));

            codeStream.Emit(ILOpcode.mul_ovf);

            codeStream.Emit(ILOpcode.call, emitter.NewToken(
                                Context.GetHelperEntryPoint("InteropHelpers", "CoTaskMemAllocAndZeroMemory")));
            StoreNativeValue(codeStream);

            codeStream.EmitLabel(lNullArray);
        }

        protected override void TransformManagedToNative(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            var elementType = ManagedElementType;

            var lRangeCheck = emitter.NewCodeLabel();
            var lLoopHeader = emitter.NewCodeLabel();
            var  lNullArray = emitter.NewCodeLabel();

            var vNativeTemp = emitter.NewLocal(NativeType);
            var vIndex = emitter.NewLocal(Context.GetWellKnownType(WellKnownType.Int32));
            var vSizeOf = emitter.NewLocal(Context.GetWellKnownType(WellKnownType.IntPtr));
            var vLength = emitter.NewLocal(Context.GetWellKnownType(WellKnownType.Int32));
            
            // Check for null array
            LoadManagedValue(codeStream);
            codeStream.Emit(ILOpcode.brfalse, lNullArray);
            
            // loads the number of elements
            EmitElementCount(codeStream, MarshalDirection.Forward);
            codeStream.EmitStLoc(vLength);

            TypeDesc nativeElementType = NativeElementType;
            codeStream.Emit(ILOpcode.sizeof_, emitter.NewToken(nativeElementType));

            codeStream.EmitStLoc(vSizeOf);

            LoadNativeValue(codeStream);
            codeStream.EmitStLoc(vNativeTemp);

            codeStream.EmitLdc(0);
            codeStream.EmitStLoc(vIndex);
            codeStream.Emit(ILOpcode.br, lRangeCheck);

            codeStream.EmitLabel(lLoopHeader);
            codeStream.EmitLdLoc(vNativeTemp);

            LoadManagedValue(codeStream);
            codeStream.EmitLdLoc(vIndex);
            codeStream.EmitLdElem(elementType);
            // generate marshalling IL for the element
            GetElementMarshaller(MarshalDirection.Forward)
                .EmitMarshallingIL(new PInvokeILCodeStreams(_ilCodeStreams.Emitter, codeStream));

            codeStream.EmitStInd(nativeElementType);
            codeStream.EmitLdLoc(vIndex);
            codeStream.EmitLdc(1);
            codeStream.Emit(ILOpcode.add);
            codeStream.EmitStLoc(vIndex);
            codeStream.EmitLdLoc(vNativeTemp);
            codeStream.EmitLdLoc(vSizeOf);
            codeStream.Emit(ILOpcode.add);
            codeStream.EmitStLoc(vNativeTemp);

            codeStream.EmitLabel(lRangeCheck);

            codeStream.EmitLdLoc(vIndex);
            codeStream.EmitLdLoc(vLength);
            codeStream.Emit(ILOpcode.blt, lLoopHeader);
            codeStream.EmitLabel(lNullArray);
        }

        protected override void TransformNativeToManaged(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;

            var elementType = ManagedElementType;
            var nativeElementType = NativeElementType;

            ILLocalVariable vSizeOf = emitter.NewLocal(Context.GetWellKnownType(WellKnownType.Int32));
            ILLocalVariable vLength = emitter.NewLocal(Context.GetWellKnownType(WellKnownType.IntPtr));

            ILCodeLabel lRangeCheck = emitter.NewCodeLabel();
            ILCodeLabel lLoopHeader = emitter.NewCodeLabel();
            var lNullArray = emitter.NewCodeLabel();
            
            // Check for null array
            LoadManagedValue(codeStream);
            codeStream.Emit(ILOpcode.brfalse, lNullArray);


            EmitElementCount(codeStream, MarshalDirection.Reverse);

            codeStream.EmitStLoc(vLength);
            codeStream.Emit(ILOpcode.sizeof_, emitter.NewToken(nativeElementType));

            codeStream.EmitStLoc(vSizeOf);

            var vIndex = emitter.NewLocal(Context.GetWellKnownType(WellKnownType.Int32));
            ILLocalVariable vNativeTemp = emitter.NewLocal(NativeType);

            LoadNativeValue(codeStream);
            codeStream.EmitStLoc(vNativeTemp);
            codeStream.EmitLdc(0);
            codeStream.EmitStLoc(vIndex);
            codeStream.Emit(ILOpcode.br, lRangeCheck);


            codeStream.EmitLabel(lLoopHeader);

            LoadManagedValue(codeStream);

            codeStream.EmitLdLoc(vIndex);
            codeStream.EmitLdLoc(vNativeTemp);

            codeStream.EmitLdInd(nativeElementType);

            // generate marshalling IL for the element
            GetElementMarshaller(MarshalDirection.Reverse)
                .EmitMarshallingIL(new PInvokeILCodeStreams(_ilCodeStreams.Emitter, codeStream));

            codeStream.EmitStElem(elementType);

            codeStream.EmitLdLoc(vIndex);
            codeStream.EmitLdc(1);
            codeStream.Emit(ILOpcode.add);
            codeStream.EmitStLoc(vIndex);
            codeStream.EmitLdLoc(vNativeTemp);
            codeStream.EmitLdLoc(vSizeOf);
            codeStream.Emit(ILOpcode.add);
            codeStream.EmitStLoc(vNativeTemp);


            codeStream.EmitLabel(lRangeCheck);
            codeStream.EmitLdLoc(vIndex);
            codeStream.EmitLdLoc(vLength);
            codeStream.Emit(ILOpcode.blt, lLoopHeader);
            codeStream.EmitLabel(lNullArray);
        }

        protected override void AllocNativeToManaged(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;

            var elementType = ManagedElementType;
            EmitElementCount(codeStream, MarshalDirection.Reverse);
            codeStream.Emit(ILOpcode.newarr, emitter.NewToken(elementType));
            StoreManagedValue(codeStream);
        }

        protected override void EmitCleanupManaged(ILCodeStream codeStream)
        {
            Marshaller elementMarshaller = GetElementMarshaller(MarshalDirection.Forward);
            ILEmitter emitter = _ilCodeStreams.Emitter;

            var lNullArray = emitter.NewCodeLabel();

            // Check for null array
            LoadManagedValue(codeStream);
            codeStream.Emit(ILOpcode.brfalse, lNullArray);

            // generate cleanup code only if it is necessary
            if (elementMarshaller.CleanupRequired)
            {
                //
                //     for (index=0; index< array.length; index++)
                //         Cleanup(array[i]);
                //
                var vIndex = emitter.NewLocal(Context.GetWellKnownType(WellKnownType.Int32));
                ILLocalVariable vLength = emitter.NewLocal(Context.GetWellKnownType(WellKnownType.IntPtr));

                ILCodeLabel lRangeCheck = emitter.NewCodeLabel();
                ILCodeLabel lLoopHeader = emitter.NewCodeLabel();
                ILLocalVariable vSizeOf = emitter.NewLocal(Context.GetWellKnownType(WellKnownType.IntPtr));

                var nativeElementType = NativeElementType;
                // calculate sizeof(array[i])
                codeStream.Emit(ILOpcode.sizeof_, emitter.NewToken(nativeElementType));

                codeStream.EmitStLoc(vSizeOf);


                // calculate array.length
                EmitElementCount(codeStream, MarshalDirection.Forward);
                codeStream.EmitStLoc(vLength);

                // load native value
                ILLocalVariable vNativeTemp = emitter.NewLocal(NativeType);
                LoadNativeValue(codeStream);
                codeStream.EmitStLoc(vNativeTemp);

                // index = 0
                codeStream.EmitLdc(0);
                codeStream.EmitStLoc(vIndex);
                codeStream.Emit(ILOpcode.br, lRangeCheck);

                codeStream.EmitLabel(lLoopHeader);
                codeStream.EmitLdLoc(vNativeTemp);
                codeStream.EmitLdInd(nativeElementType);
                // generate cleanup code for this element
                elementMarshaller.EmitElementCleanup(codeStream, emitter);

                codeStream.EmitLdLoc(vIndex);
                codeStream.EmitLdc(1);
                codeStream.Emit(ILOpcode.add);
                codeStream.EmitStLoc(vIndex);
                codeStream.EmitLdLoc(vNativeTemp);
                codeStream.EmitLdLoc(vSizeOf);
                codeStream.Emit(ILOpcode.add);
                codeStream.EmitStLoc(vNativeTemp);

                codeStream.EmitLabel(lRangeCheck);

                codeStream.EmitLdLoc(vIndex);
                codeStream.EmitLdLoc(vLength);
                codeStream.Emit(ILOpcode.blt, lLoopHeader);
            }

            LoadNativeValue(codeStream);
            codeStream.Emit(ILOpcode.call, emitter.NewToken(
                                Context.GetHelperEntryPoint("InteropHelpers", "CoTaskMemFree")));
            codeStream.EmitLabel(lNullArray);
        }
    }

    class BlittableArrayMarshaller : ArrayMarshaller
    {
        protected override void AllocAndTransformManagedToNative(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            var arrayType = (ArrayType)ManagedType;
            Debug.Assert(arrayType.IsSzArray);

            ILLocalVariable vPinnedFirstElement = emitter.NewLocal(arrayType.ParameterType.MakeByRefType(), true);
            ILCodeLabel lNullArray = emitter.NewCodeLabel();

            // Check for null array, or 0 element array.
            LoadManagedValue(codeStream);
            codeStream.Emit(ILOpcode.brfalse, lNullArray);
            LoadManagedValue(codeStream);
            codeStream.Emit(ILOpcode.ldlen);
            codeStream.Emit(ILOpcode.conv_i4);
            codeStream.Emit(ILOpcode.brfalse, lNullArray);

            // Array has elements.
            LoadManagedValue(codeStream);
            codeStream.EmitLdc(0);
            codeStream.Emit(ILOpcode.ldelema, emitter.NewToken(arrayType.ElementType));
            codeStream.EmitStLoc(vPinnedFirstElement);

            // Fall through. If array didn't have elements, vPinnedFirstElement is zeroinit.
            codeStream.EmitLabel(lNullArray);
            codeStream.EmitLdLoc(vPinnedFirstElement);
            codeStream.Emit(ILOpcode.conv_i);
            StoreNativeValue(codeStream);
        }

        protected override void ReInitNativeTransform(ILCodeStream codeStream)
        {
            codeStream.EmitLdc(0);
            codeStream.Emit(ILOpcode.conv_u);
            StoreNativeValue(codeStream);
        }

        protected override void TransformNativeToManaged(ILCodeStream codeStream)
        {
            if ((IsManagedByRef && !In) || (MarshalDirection == MarshalDirection.Reverse && MarshallerType == MarshallerType.Argument))
                base.TransformNativeToManaged(codeStream);
        }

        protected override void EmitCleanupManaged(ILCodeStream codeStream)
        {
            if (IsManagedByRef && !In)
                base.EmitCleanupManaged(codeStream);
        }
    }

    class BooleanMarshaller : Marshaller
    {
        protected override void AllocAndTransformManagedToNative(ILCodeStream codeStream)
        {
            LoadManagedValue(codeStream);
            codeStream.EmitLdc(0);
            codeStream.Emit(ILOpcode.ceq);
            codeStream.EmitLdc(0);
            codeStream.Emit(ILOpcode.ceq);
            StoreNativeValue(codeStream);
        }

        protected override void AllocAndTransformNativeToManaged(ILCodeStream codeStream)
        {
            LoadNativeValue(codeStream);
            codeStream.EmitLdc(0);
            codeStream.Emit(ILOpcode.ceq);
            codeStream.EmitLdc(0);
            codeStream.Emit(ILOpcode.ceq);
            StoreManagedValue(codeStream);
        }
    }

    class UnicodeStringMarshaller : Marshaller
    {
        private bool ShouldBePinned
        {
            get
            {
                return MarshalDirection == MarshalDirection.Forward 
                    && MarshallerType != MarshallerType.Field
                    && !IsManagedByRef
                    && In
                    && !Out;
            }
        }

        internal override bool CleanupRequired
        {
            get
            {
                return !ShouldBePinned; //cleanup is only required when it is not pinned
            }
        }

        internal override void EmitElementCleanup(ILCodeStream codeStream, ILEmitter emitter)
        {
            codeStream.Emit(ILOpcode.call, emitter.NewToken(
                                Context.GetHelperEntryPoint("InteropHelpers", "CoTaskMemFree")));
        }

        protected override void AllocAndTransformManagedToNative(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;

            if (ShouldBePinned)
            {
                //
                // Pin the string and push a pointer to the first character on the stack.
                //
                TypeDesc stringType = Context.GetWellKnownType(WellKnownType.String);

                ILLocalVariable vPinnedString = emitter.NewLocal(stringType, true);
                ILCodeLabel lNullString = emitter.NewCodeLabel();

                LoadManagedValue(codeStream);
                codeStream.EmitStLoc(vPinnedString);
                codeStream.EmitLdLoc(vPinnedString);

                codeStream.Emit(ILOpcode.conv_i);
                codeStream.Emit(ILOpcode.dup);

                // Marshalling a null string?
                codeStream.Emit(ILOpcode.brfalse, lNullString);

                codeStream.Emit(ILOpcode.call, emitter.NewToken(
                    Context.SystemModule.
                        GetKnownType("System.Runtime.CompilerServices", "RuntimeHelpers").
                            GetKnownMethod("get_OffsetToStringData", null)));

                codeStream.Emit(ILOpcode.add);

                codeStream.EmitLabel(lNullString);
                StoreNativeValue(codeStream);
            }
            else
            {
                var helper = Context.GetHelperEntryPoint("InteropHelpers", "StringToUnicodeBuffer");
                LoadManagedValue(codeStream);

                codeStream.Emit(ILOpcode.call, emitter.NewToken(helper));

                StoreNativeValue(codeStream);
            }
        }

        protected override void TransformNativeToManaged(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            var charPtrConstructor = Context.GetWellKnownType(WellKnownType.String).GetMethod(".ctor",
                new MethodSignature(
                    MethodSignatureFlags.None, 0, Context.GetWellKnownType(WellKnownType.Void),
                        new TypeDesc[] {
                            Context.GetWellKnownType(WellKnownType.Char).MakePointerType() }
                        ));
            LoadNativeValue(codeStream);
            codeStream.Emit(ILOpcode.newobj, emitter.NewToken(charPtrConstructor));
            StoreManagedValue(codeStream);
        }

        protected override void EmitCleanupManaged(ILCodeStream codeStream)
        {
            if (CleanupRequired)
            {
                var emitter = _ilCodeStreams.Emitter;
                var lNullCheck = emitter.NewCodeLabel();

                // Check for null array
                LoadManagedValue(codeStream);
                codeStream.Emit(ILOpcode.brfalse, lNullCheck);

                LoadNativeValue(codeStream);
                codeStream.Emit(ILOpcode.call, emitter.NewToken(
                                    InteropTypes.GetMarshal(Context).GetKnownMethod("FreeCoTaskMem", null)));

                codeStream.EmitLabel(lNullCheck);
            }
        }
    }

    class AnsiStringMarshaller : Marshaller
    {
        internal override bool CleanupRequired
        {
            get
            {
                return true;
            }
        }

        internal override void EmitElementCleanup(ILCodeStream codeStream, ILEmitter emitter)
        {
            codeStream.Emit(ILOpcode.call, emitter.NewToken(
                                Context.GetHelperEntryPoint("InteropHelpers", "CoTaskMemFree")));
        }

        protected override void AllocAndTransformManagedToNative(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;

            //
            // ANSI marshalling. Allocate a byte array, copy characters
            //
            var stringToAnsi = Context.GetHelperEntryPoint("InteropHelpers", "StringToAnsiString");
            LoadManagedValue(codeStream);

            codeStream.Emit(PInvokeFlags.BestFitMapping ? ILOpcode.ldc_i4_1 : ILOpcode.ldc_i4_0);
            codeStream.Emit(PInvokeFlags.ThrowOnUnmappableChar ? ILOpcode.ldc_i4_1 : ILOpcode.ldc_i4_0);

            codeStream.Emit(ILOpcode.call, emitter.NewToken(stringToAnsi));

            StoreNativeValue(codeStream);
        }

        protected override void TransformNativeToManaged(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            var ansiToString = Context.GetHelperEntryPoint("InteropHelpers", "AnsiStringToString");
            LoadNativeValue(codeStream);
            codeStream.Emit(ILOpcode.call, emitter.NewToken(ansiToString));
            StoreManagedValue(codeStream);
        }

        protected override void EmitCleanupManaged(ILCodeStream codeStream)
        {
            var emitter = _ilCodeStreams.Emitter;
            var lNullCheck = emitter.NewCodeLabel();

            // Check for null array
            LoadManagedValue(codeStream);
            codeStream.Emit(ILOpcode.brfalse, lNullCheck);

            LoadNativeValue(codeStream);
            codeStream.Emit(ILOpcode.call, emitter.NewToken(
                                Context.GetHelperEntryPoint("InteropHelpers", "CoTaskMemFree")));

            codeStream.EmitLabel(lNullCheck);
        }
    }

    class UTF8StringMarshaller : Marshaller
    {
        internal override bool CleanupRequired
        {
            get
            {
                return true;
            }
        }

        internal override void EmitElementCleanup(ILCodeStream codeStream, ILEmitter emitter)
        {
            codeStream.Emit(ILOpcode.call, emitter.NewToken(
                                Context.GetHelperEntryPoint("InteropHelpers", "CoTaskMemFree")));
        }

        protected override void AllocAndTransformManagedToNative(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;

            //
            // UTF8 marshalling. Allocate a byte array, copy characters
            //
            var stringToAnsi = Context.GetHelperEntryPoint("InteropHelpers", "StringToUTF8String");
            LoadManagedValue(codeStream);
            codeStream.Emit(ILOpcode.call, emitter.NewToken(stringToAnsi));
            StoreNativeValue(codeStream);
        }

        protected override void TransformNativeToManaged(ILCodeStream codeStream)
        {
            ILEmitter emitter = _ilCodeStreams.Emitter;
            var ansiToString = Context.GetHelperEntryPoint("InteropHelpers", "UTF8StringToString");
            LoadNativeValue(codeStream);
            codeStream.Emit(ILOpcode.call, emitter.NewToken(ansiToString));
            StoreManagedValue(codeStream);
        }

        protected override void EmitCleanupManaged(ILCodeStream codeStream)
        {
            var emitter = _ilCodeStreams.Emitter;
            var lNullCheck = emitter.NewCodeLabel();

            // Check for null array
            LoadManagedValue(codeStream);
            codeStream.Emit(ILOpcode.brfalse, lNullCheck);

            LoadNativeValue(codeStream);
            codeStream.Emit(ILOpcode.call, emitter.NewToken(
                                Context.GetHelperEntryPoint("InteropHelpers", "CoTaskMemFree")));

            codeStream.EmitLabel(lNullCheck);
        }
    }
}

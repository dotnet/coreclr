// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using ILCompiler.DependencyAnalysis.ReadyToRun;

using Internal.JitInterface;
using Internal.TypeSystem;

namespace ILCompiler.DependencyAnalysis
{
    public enum ReadyToRunHelperId
    {
        Invalid,
        NewHelper,
        NewArr1,
        IsInstanceOf,
        CastClass,
        GetNonGCStaticBase,
        GetGCStaticBase,
        GetThreadStaticBase,
        GetThreadNonGcStaticBase,
        CctorTrigger,

        //// The following helpers are used for generic lookups only
        TypeHandle,
        DeclaringTypeHandle,
        MethodHandle,
        FieldHandle,
        MethodDictionary,
        TypeDictionary,
        MethodEntry,
        VirtualDispatchCell,
    }

    public sealed class ReadyToRunSymbolNodeFactory
    {
        private readonly ReadyToRunCodegenNodeFactory _codegenNodeFactory;

        public ReadyToRunSymbolNodeFactory(ReadyToRunCodegenNodeFactory codegenNodeFactory)
        {
            _codegenNodeFactory = codegenNodeFactory;

            // Node caches
            _importStrings = new NodeCache<ModuleToken, ISymbolNode>(key =>
            {
                return new StringImport(_codegenNodeFactory.StringImports, key, GetSignatureContext());
            });

            _r2rHelpers = new NodeCache<Tuple<ReadyToRunHelperId, object>, ISymbolNode>(CreateReadyToRunHelper);

            _fieldAddressCache = new NodeCache<FieldDesc, ISymbolNode>(fieldDesc =>
            {

                return new DelayLoadHelperImport(
                    _codegenNodeFactory,
                    _codegenNodeFactory.HelperImports,
                    ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                    new FieldFixupSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_FieldAddress, fieldDesc, GetSignatureContext())
                );
            });

            _fieldOffsetCache = new NodeCache<FieldDesc, ISymbolNode>(fieldDesc =>
            {
                return new PrecodeHelperImport(
                    _codegenNodeFactory,
                    new FieldFixupSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_FieldOffset, fieldDesc, GetSignatureContext())
                );
            });

            _fieldBaseOffsetCache = new NodeCache<TypeDesc, ISymbolNode>(typeDesc =>
            {
                return new PrecodeHelperImport(
                    _codegenNodeFactory,
                    _codegenNodeFactory.TypeSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_FieldBaseOffset, typeDesc)
                );
            });

            _interfaceDispatchCells = new NodeCache<MethodAndCallSite, ISymbolNode>(cellKey =>
            {
                SignatureContext signatureContext = GetSignatureContext();
                return new DelayLoadHelperMethodImport(
                    _codegenNodeFactory,
                    _codegenNodeFactory.DispatchImports,
                    ILCompiler.ReadyToRunHelper.DelayLoad_MethodCall,
                    cellKey.Method,
                    useVirtualCall: true,
                    useInstantiatingStub: false,
                    _codegenNodeFactory.MethodSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_VirtualEntry,
                        cellKey.Method,
                        cellKey.IsUnboxingStub, isInstantiatingStub: false, signatureContext),
                    signatureContext,
                    cellKey.CallSite);
            });

            _delegateCtors = new NodeCache<TypeAndMethod, ISymbolNode>(ctorKey =>
            {
                SignatureContext signatureContext = GetSignatureContext();
                IMethodNode targetMethodNode = _codegenNodeFactory.MethodEntrypoint(
                    ctorKey.Method,
                    isUnboxingStub: false,
                    isInstantiatingStub: false,
                    isPrecodeImportRequired: false,
                    signatureContext: signatureContext);

                return new DelayLoadHelperImport(
                    _codegenNodeFactory,
                    _codegenNodeFactory.HelperImports,
                    ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                    new DelegateCtorSignature(ctorKey.Type, targetMethodNode, ctorKey.Method.Token, signatureContext));
            });

            _genericLookupHelpers = new NodeCache<GenericLookupKey, ISymbolNode>(key =>
            {
                return new DelayLoadHelperImport(
                    _codegenNodeFactory,
                    _codegenNodeFactory.HelperImports,
                    ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                    new GenericLookupSignature(
                        key.LookupKind,
                        key.FixupKind,
                        key.TypeArgument,
                        key.MethodArgument,
                        key.FieldArgument,
                        key.MethodContext,
                        GetSignatureContext()));
            });

            _indirectPInvokeTargetNodes = new NodeCache<MethodWithToken, ISymbolNode>(key =>
            {
                return new PrecodeHelperImport(
                    _codegenNodeFactory,
                    _codegenNodeFactory.MethodSignature(
                        ReadyToRunFixupKind.READYTORUN_FIXUP_IndirectPInvokeTarget,
                        key,
                        signatureContext: GetSignatureContext(),
                        isUnboxingStub: false,
                        isInstantiatingStub: false));
            });
        }

        private SignatureContext GetSignatureContext()
        {
            return _codegenNodeFactory.InputModuleContext;
        }

        private readonly NodeCache<ModuleToken, ISymbolNode> _importStrings;

        public ISymbolNode StringLiteral(ModuleToken token, SignatureContext signatureContext)
        {
            return _importStrings.GetOrAdd(token);
        }

        private readonly NodeCache<Tuple<ReadyToRunHelperId, object>, ISymbolNode> _r2rHelpers;

        public ISymbolNode CreateReadyToRunHelper(Tuple<ReadyToRunHelperId, object> tuple)
        {
            object target = tuple.Item2;
            SignatureContext signatureContext = GetSignatureContext();
            switch (tuple.Item1)
            {
                case ReadyToRunHelperId.NewHelper:
                    return CreateNewHelper((TypeDesc)target, signatureContext);

                case ReadyToRunHelperId.NewArr1:
                    return CreateNewArrayHelper((ArrayType)target, signatureContext);

                case ReadyToRunHelperId.GetGCStaticBase:
                    return CreateGCStaticBaseHelper((TypeDesc)target, signatureContext);

                case ReadyToRunHelperId.GetNonGCStaticBase:
                    return CreateNonGCStaticBaseHelper((TypeDesc)target, signatureContext);

                case ReadyToRunHelperId.GetThreadStaticBase:
                    return CreateThreadGcStaticBaseHelper((TypeDesc)target, signatureContext);

                case ReadyToRunHelperId.GetThreadNonGcStaticBase:
                    return CreateThreadNonGcStaticBaseHelper((TypeDesc)target, signatureContext);

                case ReadyToRunHelperId.IsInstanceOf:
                    return CreateIsInstanceOfHelper((TypeDesc)target, signatureContext);

                case ReadyToRunHelperId.CastClass:
                    return CreateCastClassHelper((TypeDesc)target, signatureContext);

                case ReadyToRunHelperId.TypeHandle:
                    return CreateTypeHandleHelper((TypeDesc)target, signatureContext);

                case ReadyToRunHelperId.MethodHandle:
                    return CreateMethodHandleHelper((MethodWithToken)target, signatureContext);

                case ReadyToRunHelperId.FieldHandle:
                    return CreateFieldHandleHelper((FieldDesc)target, signatureContext);

                case ReadyToRunHelperId.CctorTrigger:
                    return CreateCctorTrigger((TypeDesc)target, signatureContext);

                case ReadyToRunHelperId.TypeDictionary:
                    return CreateTypeDictionary((TypeDesc)target, signatureContext);

                case ReadyToRunHelperId.MethodDictionary:
                    return CreateMethodDictionary((MethodWithToken)target, signatureContext);

                default:
                    throw new NotImplementedException(tuple.Item1.ToString());
            }
        }

        public ISymbolNode ReadyToRunHelper(ReadyToRunHelperId id, object target, SignatureContext signatureContext)
        {
            return _r2rHelpers.GetOrAdd(Tuple.Create(id, target));
        }

        private ISymbolNode CreateNewHelper(TypeDesc type, SignatureContext signatureContext)
        {
            return new DelayLoadHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.HelperImports,
                ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                new NewObjectFixupSignature(type, signatureContext));
        }

        private ISymbolNode CreateNewArrayHelper(ArrayType type, SignatureContext signatureContext)
        {
            return new DelayLoadHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.HelperImports,
                ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                new NewArrayFixupSignature(type, signatureContext));
        }

        private ISymbolNode CreateGCStaticBaseHelper(TypeDesc type, SignatureContext signatureContext)
        {
            return new DelayLoadHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.HelperImports,
                ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                _codegenNodeFactory.TypeSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_StaticBaseGC, type));
        }

        private ISymbolNode CreateNonGCStaticBaseHelper(TypeDesc type, SignatureContext signatureContext)
        {
            return new DelayLoadHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.HelperImports,
                ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                _codegenNodeFactory.TypeSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_StaticBaseNonGC, type));
        }

        private ISymbolNode CreateThreadGcStaticBaseHelper(TypeDesc type, SignatureContext signatureContext)
        {
            return new DelayLoadHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.HelperImports,
                ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                _codegenNodeFactory.TypeSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_ThreadStaticBaseGC, type));
        }

        private ISymbolNode CreateThreadNonGcStaticBaseHelper(TypeDesc type, SignatureContext signatureContext)
        {
            return new DelayLoadHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.HelperImports,
                ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                _codegenNodeFactory.TypeSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_ThreadStaticBaseNonGC, type));
        }

        private ISymbolNode CreateIsInstanceOfHelper(TypeDesc type, SignatureContext signatureContext)
        {
            return new DelayLoadHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.HelperImports,
                ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                _codegenNodeFactory.TypeSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_IsInstanceOf, type));
        }

        private ISymbolNode CreateCastClassHelper(TypeDesc type, SignatureContext signatureContext)
        {
            return new DelayLoadHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.HelperImports,
                ILCompiler.ReadyToRunHelper.DelayLoad_Helper_Obj,
                _codegenNodeFactory.TypeSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_ChkCast, type));
        }

        private ISymbolNode CreateTypeHandleHelper(TypeDesc type, SignatureContext signatureContext)
        {
            return new PrecodeHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.TypeSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_TypeHandle, type));
        }

        private ISymbolNode CreateMethodHandleHelper(MethodWithToken method, SignatureContext signatureContext)
        {
            bool useInstantiatingStub = method.Method.GetCanonMethodTarget(CanonicalFormKind.Specific) != method.Method;

            return new PrecodeHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.MethodSignature(
                    ReadyToRunFixupKind.READYTORUN_FIXUP_MethodHandle,
                    method,
                    isUnboxingStub: false,
                    isInstantiatingStub: useInstantiatingStub,
                    signatureContext));
        }

        private ISymbolNode CreateFieldHandleHelper(FieldDesc field, SignatureContext signatureContext)
        {
            return new PrecodeHelperImport(
                _codegenNodeFactory,
                new FieldFixupSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_FieldHandle, field, signatureContext));
        }

        private ISymbolNode CreateCctorTrigger(TypeDesc type, SignatureContext signatureContext)
        {
            return new DelayLoadHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.HelperImports,
                ILCompiler.ReadyToRunHelper.DelayLoad_Helper,
                _codegenNodeFactory.TypeSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_CctorTrigger, type));
        }

        private ISymbolNode CreateTypeDictionary(TypeDesc type, SignatureContext signatureContext)
        {
            return new PrecodeHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.TypeSignature(ReadyToRunFixupKind.READYTORUN_FIXUP_TypeDictionary, type)
            );
        }

        private ISymbolNode CreateMethodDictionary(MethodWithToken method, SignatureContext signatureContext)
        {
            return new PrecodeHelperImport(
                _codegenNodeFactory,
                _codegenNodeFactory.MethodSignature(
                    ReadyToRunFixupKind.READYTORUN_FIXUP_MethodDictionary, 
                    method, 
                    isUnboxingStub: false,
                    isInstantiatingStub: true,
                    signatureContext));
        }

        private readonly NodeCache<FieldDesc, ISymbolNode> _fieldAddressCache;

        public ISymbolNode FieldAddress(FieldDesc fieldDesc, SignatureContext signatureContext)
        {
            return _fieldAddressCache.GetOrAdd(fieldDesc);
        }

        private readonly NodeCache<FieldDesc, ISymbolNode> _fieldOffsetCache;

        public ISymbolNode FieldOffset(FieldDesc fieldDesc, SignatureContext signatureContext)
        {
            return _fieldOffsetCache.GetOrAdd(fieldDesc);
        }

        private readonly NodeCache<TypeDesc, ISymbolNode> _fieldBaseOffsetCache;

        public ISymbolNode FieldBaseOffset(TypeDesc typeDesc, SignatureContext signatureContext)
        {
            return _fieldBaseOffsetCache.GetOrAdd(typeDesc);
        }

        private readonly NodeCache<MethodAndCallSite, ISymbolNode> _interfaceDispatchCells = new NodeCache<MethodAndCallSite, ISymbolNode>();

        public ISymbolNode InterfaceDispatchCell(MethodWithToken method, SignatureContext signatureContext, bool isUnboxingStub, string callSite)
        {
            MethodAndCallSite cellKey = new MethodAndCallSite(method, isUnboxingStub, callSite);
            return _interfaceDispatchCells.GetOrAdd(cellKey);
        }

        private readonly NodeCache<TypeAndMethod, ISymbolNode> _delegateCtors = new NodeCache<TypeAndMethod, ISymbolNode>();

        public ISymbolNode DelegateCtor(TypeDesc delegateType, MethodWithToken method, SignatureContext signatureContext)
        {
            TypeAndMethod ctorKey = new TypeAndMethod(delegateType, method, isUnboxingStub: false, isInstantiatingStub: false, isPrecodeImportRequired: false);
            return _delegateCtors.GetOrAdd(ctorKey);
        }

        struct MethodAndCallSite : IEquatable<MethodAndCallSite>
        {
            public readonly MethodWithToken Method;
            public readonly bool IsUnboxingStub;
            public readonly string CallSite;

            public MethodAndCallSite(MethodWithToken method, bool isUnboxingStub, string callSite)
            {
                CallSite = callSite;
                IsUnboxingStub = isUnboxingStub;
                Method = method;
            }

            public bool Equals(MethodAndCallSite other)
            {
                return CallSite == other.CallSite && Method.Equals(other.Method) && IsUnboxingStub == other.IsUnboxingStub;
            }

            public override bool Equals(object obj)
            {
                return obj is MethodAndCallSite other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (CallSite != null ? CallSite.GetHashCode() : 0) + unchecked(31 * Method.GetHashCode())
                     ^ (IsUnboxingStub ? -0x80000000 : 0);
            }
        }

        private class GenericLookupKey : IEquatable<GenericLookupKey>
        {
            public readonly CORINFO_RUNTIME_LOOKUP_KIND LookupKind;
            public readonly ReadyToRunFixupKind FixupKind;
            public readonly TypeDesc TypeArgument;
            public readonly MethodWithToken MethodArgument;
            public readonly FieldDesc FieldArgument;
            public readonly GenericContext MethodContext;

            public GenericLookupKey(
                CORINFO_RUNTIME_LOOKUP_KIND lookupKind,
                ReadyToRunFixupKind fixupKind,
                TypeDesc typeArgument,
                MethodWithToken methodArgument,
                FieldDesc fieldArgument,
                GenericContext methodContext)
            {
                LookupKind = lookupKind;
                FixupKind = fixupKind;
                TypeArgument = typeArgument;
                MethodArgument = methodArgument;
                FieldArgument = fieldArgument;
                MethodContext = methodContext;
            }

            public bool Equals(GenericLookupKey other)
            {
                return LookupKind == other.LookupKind &&
                    FixupKind == other.FixupKind &&
                    RuntimeDeterminedTypeHelper.Equals(TypeArgument, other.TypeArgument) &&
                    RuntimeDeterminedTypeHelper.Equals(MethodArgument?.Method ?? null, other.MethodArgument?.Method ?? null) &&
                    RuntimeDeterminedTypeHelper.Equals(FieldArgument, other.FieldArgument) &&
                    MethodContext.Equals(other.MethodContext);
            }

            public override bool Equals(object obj)
            {
                return obj is GenericLookupKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return unchecked(((int)LookupKind << 24) +
                    (int)FixupKind +
                    (TypeArgument != null ? 31 * RuntimeDeterminedTypeHelper.GetHashCode(TypeArgument) : 0) +
                    (MethodArgument != null ? 31 * RuntimeDeterminedTypeHelper.GetHashCode(MethodArgument.Method) : 0) +
                    (FieldArgument != null ? 31 * RuntimeDeterminedTypeHelper.GetHashCode(FieldArgument) : 0) +
                    MethodContext.GetHashCode());
            }
        }

        private readonly NodeCache<GenericLookupKey, ISymbolNode> _genericLookupHelpers;

        public ISymbolNode GenericLookupHelper(
            CORINFO_RUNTIME_LOOKUP_KIND runtimeLookupKind,
            ReadyToRunHelperId helperId,
            object helperArgument,
            GenericContext methodContext,
            SignatureContext signatureContext)
        {
            switch (helperId)
            {
                case ReadyToRunHelperId.TypeHandle:
                    return GenericLookupTypeHelper(
                        runtimeLookupKind,
                        ReadyToRunFixupKind.READYTORUN_FIXUP_TypeHandle,
                        helperArgument,
                        methodContext,
                        signatureContext);

                case ReadyToRunHelperId.MethodHandle:
                    return GenericLookupMethodHelper(
                        runtimeLookupKind,
                        ReadyToRunFixupKind.READYTORUN_FIXUP_MethodHandle,
                        (MethodWithToken)helperArgument,
                        methodContext,
                        signatureContext);

                case ReadyToRunHelperId.MethodEntry:
                    return GenericLookupMethodHelper(
                        runtimeLookupKind,
                        ReadyToRunFixupKind.READYTORUN_FIXUP_MethodEntry,
                        (MethodWithToken)helperArgument,
                        methodContext,
                        signatureContext);

                case ReadyToRunHelperId.MethodDictionary:
                    return GenericLookupMethodHelper(
                        runtimeLookupKind,
                        ReadyToRunFixupKind.READYTORUN_FIXUP_MethodHandle,
                        (MethodWithToken)helperArgument,
                        methodContext,
                        signatureContext);

                case ReadyToRunHelperId.TypeDictionary:
                    return GenericLookupTypeHelper(
                        runtimeLookupKind,
                        ReadyToRunFixupKind.READYTORUN_FIXUP_TypeDictionary,
                        (TypeDesc)helperArgument,
                        methodContext,
                        signatureContext);

                case ReadyToRunHelperId.VirtualDispatchCell:
                    return GenericLookupMethodHelper(
                        runtimeLookupKind,
                        ReadyToRunFixupKind.READYTORUN_FIXUP_VirtualEntry,
                        (MethodWithToken)helperArgument,
                        methodContext,
                        signatureContext);

                case ReadyToRunHelperId.FieldHandle:
                    return GenericLookupFieldHelper(
                        runtimeLookupKind,
                        ReadyToRunFixupKind.READYTORUN_FIXUP_FieldHandle,
                        (FieldDesc)helperArgument,
                        methodContext,
                        signatureContext);

                default:
                    throw new NotImplementedException(helperId.ToString());
            }
        }

        private ISymbolNode GenericLookupTypeHelper(
            CORINFO_RUNTIME_LOOKUP_KIND runtimeLookupKind,
            ReadyToRunFixupKind fixupKind,
            object helperArgument,
            GenericContext methodContext,
            SignatureContext signatureContext)
        {
            TypeDesc typeArgument;
            if (helperArgument is MethodWithToken methodWithToken)
            {
                typeArgument = methodWithToken.Method.OwningType;
            }
            else if (helperArgument is FieldDesc fieldDesc)
            {
                typeArgument = fieldDesc.OwningType;
            }
            else
            {
                typeArgument = (TypeDesc)helperArgument;
            }

            GenericLookupKey key = new GenericLookupKey(runtimeLookupKind, fixupKind, typeArgument, methodArgument: null, fieldArgument: null, methodContext);
            return _genericLookupHelpers.GetOrAdd(key);
        }

        private ISymbolNode GenericLookupFieldHelper(
            CORINFO_RUNTIME_LOOKUP_KIND runtimeLookupKind,
            ReadyToRunFixupKind fixupKind,
            FieldDesc fieldArgument,
            GenericContext methodContext,
            SignatureContext signatureContext)
        {
            GenericLookupKey key = new GenericLookupKey(runtimeLookupKind, fixupKind, typeArgument: null, methodArgument: null, fieldArgument: fieldArgument, methodContext);
            return _genericLookupHelpers.GetOrAdd(key);
        }

        private ISymbolNode GenericLookupMethodHelper(
            CORINFO_RUNTIME_LOOKUP_KIND runtimeLookupKind,
            ReadyToRunFixupKind fixupKind,
            MethodWithToken methodArgument,
            GenericContext methodContext,
            SignatureContext signatureContext)
        {
            GenericLookupKey key = new GenericLookupKey(runtimeLookupKind, fixupKind, typeArgument: null, methodArgument, fieldArgument: null, methodContext);
            return _genericLookupHelpers.GetOrAdd(key);
        }

        private readonly NodeCache<MethodWithToken, ISymbolNode> _indirectPInvokeTargetNodes = new NodeCache<MethodWithToken, ISymbolNode>();

        public ISymbolNode GetIndirectPInvokeTargetNode(MethodWithToken methodWithToken, SignatureContext signatureContext)
        {
            return _indirectPInvokeTargetNodes.GetOrAdd(methodWithToken);
        }
    }
}

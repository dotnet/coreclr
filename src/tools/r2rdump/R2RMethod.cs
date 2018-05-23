﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace R2RDump
{
    struct RuntimeFunction
    {
        /// <summary>
        /// The index of the runtime function
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The relative virtual address to the start of the code block
        /// </summary>
        public int StartAddress { get; }

        /// <summary>
        /// The size of the code block in bytes
        /// </summary>
        /// /// <remarks>
        /// The EndAddress field in the runtime functions section is conditional on machine type
        /// Size is -1 for images without the EndAddress field
        /// </remarks>
        public int Size { get; }

        /// <summary>
        /// The relative virtual address to the unwind info
        /// </summary>
        public int UnwindRVA { get; }

        public RuntimeFunction(int id, int startRva, int endRva, int unwindRva)
        {
            Id = id;
            StartAddress = startRva;
            Size = endRva - startRva;
            if (endRva == -1)
                Size = -1;
            UnwindRVA = unwindRva;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($"Id: {Id}\n");
            sb.AppendFormat($"StartAddress: 0x{StartAddress:X8}\n");
            if (Size == -1)
            {
                sb.Append("Size: Unavailable\n");
            }
            else
            {
                sb.AppendFormat($"Size: {Size} bytes\n");
            }

            return sb.ToString();
        }
    }

    class R2RMethod
    {
        private const int _mdtMethodDef = 0x06000000;

        MetadataReader _mdReader;

        /// <summary>
        /// The name of the method
        /// </summary>
        public string Name { get; }

        public bool IsGeneric { get; }

        /// <summary>
        /// The return type of the method
        /// </summary>
        public SignatureType ReturnType { get; }

        /// <summary>
        /// The argument types of the method
        /// </summary>
        public SignatureType[] ArgTypes { get; }

        /// <summary>
        /// The type that the method belongs to
        /// </summary>
        public Stack<string> DeclaringType { get; }

        /// <summary>
        /// The token of the method consisting of the table code (0x06) and row id
        /// </summary>
        public uint Token { get; }

        /// <summary>
        /// All the runtime functions of this method
        /// </summary>
        public List<RuntimeFunction> NativeCode { get; }

        /// <summary>
        /// The id of the entrypoint runtime function
        /// </summary>
        public int EntryPointRuntimeFunctionId { get; }

        /// <summary>
        /// Maps all the generic parameters to the type in the instance
        /// </summary>
        Dictionary<string, GenericElementTypes> _genericParamInstance;

        [Flags]
        public enum EncodeMethodSigFlags
        {
            NONE = 0x00,
            ENCODE_METHOD_SIG_UnboxingStub = 0x01,
            ENCODE_METHOD_SIG_InstantiatingStub = 0x02,
            ENCODE_METHOD_SIG_MethodInstantiation = 0x04,
            ENCODE_METHOD_SIG_SlotInsteadOfToken = 0x08,
            ENCODE_METHOD_SIG_MemberRefToken = 0x10,
            ENCODE_METHOD_SIG_Constrained = 0x20,
            ENCODE_METHOD_SIG_OwnerType = 0x40,
        };

        public enum GenericElementTypes
        {
            __Canon = 0x3e,
            Void = 0x01,
            Boolean = 0x02,
            Char = 0x03,
            Int8 = 0x04,
            UInt8 = 0x05,
            Int16 = 0x06,
            UInt16 = 0x07,
            Int32 = 0x08,
            UInt32 = 0x09,
            Int64 = 0x0a,
            UInt64 = 0x0b,
            Float = 0x0c,
            Double = 0x0d,
            String = 0x0e,
            ValueType = 0x11,
            Object = 0x1c,
            Array = 0x1d,
        };

        /// <summary>
        /// Extracts the method signature from the metadata by rid
        /// </summary>
        public R2RMethod(byte[] image, MetadataReader mdReader, uint rid)
        {
            _mdReader = mdReader;
            NativeCode = new List<RuntimeFunction>();

            // get the method signature from the MethodDefhandle
            MethodDefinitionHandle methodDefHandle = MetadataTokens.MethodDefinitionHandle((int)rid);
            MethodDefinition methodDef = mdReader.GetMethodDefinition(methodDefHandle);
            Name = mdReader.GetString(methodDef.Name);
            BlobReader signatureReader = mdReader.GetBlobReader(methodDef.Signature);

            DeclaringType = new Stack<string>();
            TypeDefinition declaringTypeDef = mdReader.GetTypeDefinition(methodDef.GetDeclaringType());
            DeclaringType.Push(mdReader.GetString(declaringTypeDef.Name));
            NamespaceDefinitionHandle namespaceHandle = declaringTypeDef.NamespaceDefinition;
            while (!namespaceHandle.IsNil)
            {
                NamespaceDefinition namespaceDef = mdReader.GetNamespaceDefinition(namespaceHandle);
                DeclaringType.Push(mdReader.GetString(namespaceDef.Name));
                namespaceHandle = namespaceDef.Parent;
            }

            SignatureHeader signatureHeader = signatureReader.ReadSignatureHeader();
            IsGeneric = signatureHeader.IsGeneric;
            var genericParams = methodDef.GetGenericParameters();
            _genericParamInstance = new Dictionary<string, GenericElementTypes>();
            foreach (GenericParameterHandle genericParam in genericParams)
            {
                _genericParamInstance[mdReader.GetString(mdReader.GetGenericParameter(genericParam).Name)] = 0;
            }

            int argCount = signatureReader.ReadCompressedInteger();
            if (IsGeneric)
            {
                argCount = signatureReader.ReadCompressedInteger();
            }

            ReturnType = new SignatureType(ref signatureReader, mdReader, genericParams);
            ArgTypes = new SignatureType[argCount];
            for (int i = 0; i < argCount; i++)
            {
                ArgTypes[i] = new SignatureType(ref signatureReader, mdReader, genericParams);
            }

            Token = _mdtMethodDef | rid;
        }

        /// <summary>
        /// Set the entry point id for generic methods and maps the generic parameters to the type
        /// </summary>
        public R2RMethod(byte[] image, MetadataReader mdReader, uint rid, int entryPointId, GenericElementTypes[] instanceArgs, uint[] tok)
            : this(image, mdReader, rid)
        {
            EntryPointRuntimeFunctionId = entryPointId;

            Dictionary<string, string> tokens = new Dictionary<string, string>();
            for (int i = 0; i < _genericParamInstance.Count; i++)
            {
                var key = _genericParamInstance.ElementAt(i).Key;
                _genericParamInstance[key] = instanceArgs[i];

                if (instanceArgs[i] == GenericElementTypes.ValueType) {
                    tokens[key] = mdReader.GetString(mdReader.GetTypeDefinition(MetadataTokens.TypeDefinitionHandle((int)tok[i])).Name);
                }
            }

            if ((ReturnType.Flags & SignatureType.SignatureTypeFlags.GENERIC) != 0)
            {
                GenericElementTypes instance = _genericParamInstance[ReturnType.TypeName];
                if (instance == GenericElementTypes.ValueType)
                {
                    ReturnType.GenericInstance = new GenericInstance(instance, tokens[ReturnType.TypeName]);
                }
                else
                {
                    ReturnType.GenericInstance = new GenericInstance(instance, Enum.GetName(typeof(GenericElementTypes), instance));
                }
            }

            for (int i = 0; i<ArgTypes.Length; i++)
            {
                if ((ArgTypes[i].Flags & SignatureType.SignatureTypeFlags.GENERIC) != 0)
                {
                    GenericElementTypes instance = _genericParamInstance[ArgTypes[i].TypeName];
                    if (instance == GenericElementTypes.ValueType)
                    {
                        ArgTypes[i].GenericInstance = new GenericInstance(instance, tokens[ArgTypes[i].TypeName]);
                    }
                    else
                    {
                        ArgTypes[i].GenericInstance = new GenericInstance(instance, Enum.GetName(typeof(GenericElementTypes), instance));
                    }
                }
            }
        }

        /// <summary>
        /// Uses the methodEntryPoint native array to get the entry point id of non-generic methods
        /// </summary>
        public R2RMethod(byte[] image, MetadataReader mdReader, NativeArray methodEntryPoints, uint rid, int offset)
            : this(image, mdReader, rid)
        {
            // get the id of the entry point runtime function from the MethodEntryPoints NativeArray
            uint id = 0; // the RUNTIME_FUNCTIONS index
            offset = (int)NativeReader.DecodeUnsigned(image, (uint)offset, ref id);
            if ((id & 1) != 0)
            {
                if ((id & 2) != 0)
                {
                    uint val = 0;
                    NativeReader.DecodeUnsigned(image, (uint)offset, ref val);
                    offset -= (int)val;
                }
                // TODO: Dump fixups

                id >>= 2;
            }
            else
            {
                id >>= 1;
            }
            EntryPointRuntimeFunctionId = (int)id;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (Name != null)
            {
                sb.AppendFormat($"{ReturnType.ToString()} ");
                foreach (var type in DeclaringType)
                {
                    sb.AppendFormat($"{type}.");
                }
                sb.AppendFormat($"{Name}");

                if (IsGeneric)
                {
                    sb.Append("<");
                    int i = 0;
                    foreach (var value in _genericParamInstance.Values)
                    {
                        if (i > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.AppendFormat($"{Enum.GetName(typeof(GenericElementTypes), value)}");
                        i++;
                    }
                    sb.Append(">");
                }

                sb.Append("(");
                for (int i = 0; i < ArgTypes.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.AppendFormat($"{ArgTypes[i].ToString()}");
                }
                sb.Append(")\n");
            }

            sb.AppendFormat($"Token: 0x{Token:X8}\n");
            sb.AppendFormat($"EntryPointRuntimeFunctionId: {EntryPointRuntimeFunctionId}\n");
            sb.AppendFormat($"Number of RuntimeFunctions: {NativeCode.Count}\n\n");

            foreach (RuntimeFunction runtimeFunction in NativeCode)
            {
                sb.AppendFormat($"{runtimeFunction}\n");
            }

            return sb.ToString();
        }
    }
}

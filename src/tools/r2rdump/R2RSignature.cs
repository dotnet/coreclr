﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Xml.Serialization;

namespace R2RDump
{
    /// <summary>
    /// Helper class for converting metadata tokens into their textual representation.
    /// </summary>
    public class MetadataNameFormatter : DisassemblingTypeProvider
    {
        /// <summary>
        /// Metadata reader used for the purpose of metadata-based name formatting.
        /// </summary>
        private readonly MetadataReader _metadataReader;

        public MetadataNameFormatter(MetadataReader metadataReader)
        {
            _metadataReader = metadataReader;
        }

        /// <summary>
        /// Construct the textual representation of a given metadata handle.
        /// </summary>
        /// <param name="metadataReader">Metadata reader corresponding to the handle</param>
        /// <param name="handle">Metadata handle to parse</param>
        /// <param name="namespaceQualified">Include namespace in type names</param>
        public static string FormatHandle(MetadataReader metadataReader, Handle handle, bool namespaceQualified = true, string owningTypeOverride = null)
        {
            MetadataNameFormatter formatter = new MetadataNameFormatter(metadataReader);
            return formatter.EmitHandleName(handle, namespaceQualified, owningTypeOverride);
        }

        public static string FormatSignature(DumpOptions options, R2RReader r2rReader, int imageOffset)
        {
            SignatureDecoder decoder = new SignatureDecoder(options, r2rReader, imageOffset);
            string result = decoder.ReadR2RSignature();
            return result;
        }

        /// <summary>
        /// Emit a given token to a specified string builder.
        /// </summary>
        /// <param name="methodToken">ECMA token to provide string representation for</param>
        private string EmitHandleName(Handle handle, bool namespaceQualified, string owningTypeOverride)
        {
            switch (handle.Kind)
            {
                case HandleKind.MemberReference:
                    return EmitMemberReferenceName((MemberReferenceHandle)handle, owningTypeOverride);

                case HandleKind.MethodSpecification:
                    return EmitMethodSpecificationName((MethodSpecificationHandle)handle, owningTypeOverride);

                case HandleKind.MethodDefinition:
                    return EmitMethodDefinitionName((MethodDefinitionHandle)handle, owningTypeOverride);

                case HandleKind.TypeReference:
                    return EmitTypeReferenceName((TypeReferenceHandle)handle, namespaceQualified);

                case HandleKind.TypeSpecification:
                    return EmitTypeSpecificationName((TypeSpecificationHandle)handle, namespaceQualified);

                case HandleKind.TypeDefinition:
                    return EmitTypeDefinitionName((TypeDefinitionHandle)handle, namespaceQualified);

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Emit a method specification.
        /// </summary>
        /// <param name="methodSpecHandle">Method specification handle</param>
        private string EmitMethodSpecificationName(MethodSpecificationHandle methodSpecHandle, string owningTypeOverride)
        {
            MethodSpecification methodSpec = _metadataReader.GetMethodSpecification(methodSpecHandle);
            DisassemblingGenericContext genericContext = new DisassemblingGenericContext(Array.Empty<string>(), Array.Empty<string>());
            return EmitHandleName(methodSpec.Method, namespaceQualified: true, owningTypeOverride: owningTypeOverride)
                + methodSpec.DecodeSignature<string, DisassemblingGenericContext>(this, genericContext);
        }

        /// <summary>
        /// Emit a method reference.
        /// </summary>
        /// <param name="memberRefHandle">Member reference handle</param>
        private string EmitMemberReferenceName(MemberReferenceHandle memberRefHandle, string owningTypeOverride)
        {
            MemberReference memberRef = _metadataReader.GetMemberReference(memberRefHandle);
            StringBuilder builder = new StringBuilder();
            DisassemblingGenericContext genericContext = new DisassemblingGenericContext(Array.Empty<string>(), Array.Empty<string>());
            switch (memberRef.GetKind())
            {
                case MemberReferenceKind.Field:
                    {
                        string fieldSig = memberRef.DecodeFieldSignature<string, DisassemblingGenericContext>(this, genericContext);
                        builder.Append(fieldSig);
                        builder.Append(" ");
                        builder.Append(EmitContainingTypeAndMemberName(memberRef, owningTypeOverride));
                        break;
                    }

                case MemberReferenceKind.Method:
                    {
                        MethodSignature<String> methodSig = memberRef.DecodeMethodSignature<string, DisassemblingGenericContext>(this, genericContext);
                        builder.Append(methodSig.ReturnType);
                        builder.Append(" ");
                        builder.Append(EmitContainingTypeAndMemberName(memberRef, owningTypeOverride));
                        builder.Append(EmitMethodSignature(methodSig));
                        break;
                    }

                default:
                    throw new NotImplementedException(memberRef.GetKind().ToString());
            }

            return builder.ToString();
        }

        /// <summary>
        /// Emit a method definition.
        /// </summary>
        /// <param name="methodSpecHandle">Method definition handle</param>
        private string EmitMethodDefinitionName(MethodDefinitionHandle methodDefinitionHandle, string owningTypeOverride)
        {
            MethodDefinition methodDef = _metadataReader.GetMethodDefinition(methodDefinitionHandle);
            DisassemblingGenericContext genericContext = new DisassemblingGenericContext(Array.Empty<string>(), Array.Empty<string>());
            MethodSignature<string> methodSig = methodDef.DecodeSignature<string, DisassemblingGenericContext>(this, genericContext);
            StringBuilder builder = new StringBuilder();
            builder.Append(methodSig.ReturnType);
            builder.Append(" ");
            if (owningTypeOverride == null)
            {
                owningTypeOverride = EmitHandleName(methodDef.GetDeclaringType(), namespaceQualified: true, owningTypeOverride: null);
            }
            builder.Append(owningTypeOverride);
            builder.Append(".");
            builder.Append(EmitString(methodDef.Name));
            builder.Append(EmitMethodSignature(methodSig));
            return builder.ToString();
        }

        /// <summary>
        /// Emit method generic arguments and parameter list.
        /// </summary>
        /// <param name="methodSignature">Method signature to format</param>
        private string EmitMethodSignature(MethodSignature<string> methodSignature)
        {
            StringBuilder builder = new StringBuilder();
            if (methodSignature.GenericParameterCount != 0)
            {
                builder.Append("<");
                bool firstTypeArg = true;
                for (int typeArgIndex = 0; typeArgIndex < methodSignature.GenericParameterCount; typeArgIndex++)
                {
                    if (firstTypeArg)
                    {
                        firstTypeArg = false;
                    }
                    else
                    {
                        builder.Append(", ");
                    }
                    builder.Append("!!");
                    builder.Append(typeArgIndex);
                }
                builder.Append(">");
            }
            builder.Append("(");
            bool firstMethodArg = true;
            foreach (string paramType in methodSignature.ParameterTypes)
            {
                if (firstMethodArg)
                {
                    firstMethodArg = false;
                }
                else
                {
                    builder.Append(", ");
                }
                builder.Append(paramType);
            }
            builder.Append(")");
            return builder.ToString();
        }

        /// <summary>
        /// Emit containing type and member name.
        /// </summary>
        /// <param name="memberRef">Member reference to format</param>
        /// <param name="owningTypeOverride">Optional override for the owning type, null = MemberReference.Parent</param>
        private string EmitContainingTypeAndMemberName(MemberReference memberRef, string owningTypeOverride)
        {
            if (owningTypeOverride == null)
            {
                owningTypeOverride = EmitHandleName(memberRef.Parent, namespaceQualified: true, owningTypeOverride: null);
            }
            return owningTypeOverride + "." + EmitString(memberRef.Name);
        }

        /// <summary>
        /// Emit type reference.
        /// </summary>
        /// <param name="typeRefHandle">Type reference handle</param>
        /// <param name="namespaceQualified">When set to true, include namespace information</param>
        private string EmitTypeReferenceName(TypeReferenceHandle typeRefHandle, bool namespaceQualified)
        {
            TypeReference typeRef = _metadataReader.GetTypeReference(typeRefHandle);
            string typeName = EmitString(typeRef.Name);
            string output = "";
            if (typeRef.ResolutionScope.Kind != HandleKind.AssemblyReference)
            {
                // Nested type - format enclosing type followed by the nested type
                return EmitHandleName(typeRef.ResolutionScope, namespaceQualified, owningTypeOverride: null) + "+" + typeName;
            }
            if (namespaceQualified)
            {
                output = EmitString(typeRef.Namespace);
                if (!string.IsNullOrEmpty(output))
                {
                    output += ".";
                }
            }
            return output + typeName;
        }

        /// <summary>
        /// Emit a type definition.
        /// </summary>
        /// <param name="typeDefHandle">Type definition handle</param>
        /// <param name="namespaceQualified">true = prefix type name with namespace information</param>
        /// <returns></returns>
        private string EmitTypeDefinitionName(TypeDefinitionHandle typeDefHandle, bool namespaceQualified)
        {
            TypeDefinition typeDef = _metadataReader.GetTypeDefinition(typeDefHandle);
            string typeName = EmitString(typeDef.Name);
            if (typeDef.IsNested)
            {
                // Nested type
                return EmitHandleName(typeDef.GetDeclaringType(), namespaceQualified, owningTypeOverride: null) + "+" + typeName;
            }

            string output;
            if (namespaceQualified)
            {
                output = EmitString(typeDef.Namespace);
                if (!string.IsNullOrEmpty(output))
                {
                    output += ".";
                }
            }
            else
            {
                output = "";
            }
            return output + typeName;
        }

        /// <summary>
        /// Emit an arbitrary type specification.
        /// </summary>
        /// <param name="typeSpecHandle">Type specification handle</param>
        /// <param name="namespaceQualified">When set to true, include namespace information</param>
        private string EmitTypeSpecificationName(TypeSpecificationHandle typeSpecHandle, bool namespaceQualified)
        {
            TypeSpecification typeSpec = _metadataReader.GetTypeSpecification(typeSpecHandle);
            DisassemblingGenericContext genericContext = new DisassemblingGenericContext(Array.Empty<string>(), Array.Empty<string>());
            return typeSpec.DecodeSignature<string, DisassemblingGenericContext>(this, genericContext);
        }

        private string EmitString(StringHandle handle)
        {
            return _metadataReader.GetString(handle);
        }
    }

    /// <summary>
    /// Helper class used as state machine for decoding a single signature.
    /// </summary>
    public class SignatureDecoder
    {
        /// <summary>
        /// Metadata reader is used to access the embedded MSIL metadata blob in the R2R file.
        /// </summary>
        private readonly MetadataReader _metadataReader;

        /// <summary>
        /// Dump options are used to specify details of signature formatting.
        /// </summary>
        private readonly DumpOptions _options;

        /// <summary>
        /// Byte array representing the R2R PE file read from disk.
        /// </summary>
        private readonly byte[] _image;

        /// <summary>
        /// Offset within the image file.
        /// </summary>
        private int _offset;

        /// <summary>
        /// Query signature parser for the current offset.
        /// </summary>
        public int Offset => _offset;

        /// <summary>
        /// Construct the signature decoder by storing the image byte array and offset within the array. 
        /// </summary>
        /// <param name="reader">R2RReader object representing the R2R PE file</param>
        /// <param name="offset">Signature offset within the array</param>
        /// <param name="options">Formatting options</param>
        public SignatureDecoder(DumpOptions options, R2RReader reader, int offset)
        {
            _metadataReader = reader.MetadataReader;
            _options = options;
            _image = reader.Image;
            _offset = offset;
        }

        /// <summary>
        /// Construct the signature decoder by storing the image byte array and offset within the array. 
        /// </summary>
        /// <param name="metadataReader">Metadata reader for the R2R image</param>
        /// <param name="signature">Signature to parse</param>
        /// <param name="offset">Optional signature offset within the signature byte array, 0 by default</param>
        public SignatureDecoder(DumpOptions options, MetadataReader metadataReader, byte[] signature, int offset = 0)
        {
            _metadataReader = metadataReader;
            _options = options;
            _image = signature;
            _offset = offset;
        }

        /// <summary>
        /// Read a single byte from the signature stream and advances the current offset.
        /// </summary>
        public byte ReadByte()
        {
            return _image[_offset++];
        }

        /// <summary>
        /// Read a single unsigned 32-bit in from the signature stream. Adapted from CorSigUncompressData,
        /// <a href="">https://github.com/dotnet/coreclr/blob/master/src/inc/cor.h</a>.
        /// </summary>
        /// <param name="data"></param>
        public uint ReadUInt()
        {
            // Handle smallest data inline. 
            byte firstByte = ReadByte();
            if ((firstByte & 0x80) == 0x00) // 0??? ????
                return firstByte;

            uint res;
            // Medium.
            if ((firstByte & 0xC0) == 0x80)  // 10?? ????
            {
                res = ((uint)(firstByte & 0x3f) << 8);
                res |= ReadByte();
            }
            else // 110? ???? 
            {
                res = (uint)(firstByte & 0x1f) << 24;
                res |= (uint)ReadByte() << 16;
                res |= (uint)ReadByte() << 8;
                res |= (uint)ReadByte();
            }
            return res;
        }

        /// <summary>
        /// Read a signed integer from the signature stream. Signed integer is basically encoded
        /// as an unsigned integer after converting it to the unsigned number 2 * abs(x) + (x &gt;= 0 ? 0 : 1).
        /// Adapted from CorSigUncompressSignedInt, <a href="">https://github.com/dotnet/coreclr/blob/master/src/inc/cor.h</a>.
        /// </summary>
        public int ReadInt()
        {
            uint rawData = ReadUInt();
            int data = (int)(rawData >> 1);
            return ((rawData & 1) == 0 ? +data : -data);
        }

        /// <summary>
        /// Read an encoded token from the stream. This encoding left-shifts the token RID twice and
        /// fills in the two least-important bits with token type (typeDef, typeRef, typeSpec, baseType).
        /// </summary>
        public uint ReadToken()
        {
            uint encodedToken = ReadUInt();
            uint rid = encodedToken >> 2;
            CorTokenType type;
            switch (encodedToken & 3)
            {
                case 0:
                    type = CorTokenType.mdtTypeDef;
                    break;

                case 1:
                    type = CorTokenType.mdtTypeRef;
                    break;

                case 2:
                    type = CorTokenType.mdtTypeSpec;
                    break;

                case 3:
                    type = CorTokenType.mdtBaseType;
                    break;

                default:
                    // This should never happen
                    throw new NotImplementedException();
            }
            return (uint)type | rid;
        }

        /// <summary>
        /// Read a single element type from the signature stream. Adapted from CorSigUncompressElementType,
        /// <a href="">https://github.com/dotnet/coreclr/blob/master/src/inc/cor.h</a>.
        /// </summary>
        /// <returns></returns>
        public CorElementType ReadElementType()
        {
            return (CorElementType)(ReadByte() & 0x7F);
        }

        /// <summary>
        /// Decode a R2R import signature. The signature starts with the fixup type followed
        /// by custom encoding per fixup type.
        /// </summary>
        /// <returns></returns>
        public string ReadR2RSignature()
        {
            StringBuilder builder = new StringBuilder();
            ParseSignature(builder);
            return builder.ToString();
        }

        public string ReadMethodSignature()
        {
            StringBuilder builder = new StringBuilder();
            ParseMethod(builder);
            return builder.ToString();
        }

        public string ReadTypeSignature()
        {
            StringBuilder builder = new StringBuilder();
            ParseType(builder);
            return builder.ToString();
        }

        /// <summary>
        /// Parse the signature into a given output string builder.
        /// </summary>
        /// <param name="builder"></param>
        private void ParseSignature(StringBuilder builder)
        {
            uint fixupType = ReadByte();
            bool moduleOverride = (fixupType & (byte)CORCOMPILE_FIXUP_BLOB_KIND.ENCODE_MODULE_OVERRIDE) != 0;
            // Check first byte for a module override being encoded
            if (moduleOverride)
            {
                builder.Append("ENCODE_MODULE_OVERRIDE @ ");
                fixupType &= ~(uint)CORCOMPILE_FIXUP_BLOB_KIND.ENCODE_MODULE_OVERRIDE;
                uint moduleIndex = ReadUInt();
                builder.Append(string.Format(" Index:  {0:X2}", moduleIndex));
            }

            switch ((ReadyToRunFixupKind)fixupType)
            {
                case ReadyToRunFixupKind.READYTORUN_FIXUP_ThisObjDictionaryLookup:
                    builder.Append("THISOBJ_DICTIONARY_LOOKUP @ ");
                    ParseType(builder);
                    builder.Append(": ");
                    ParseSignature(builder);
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_TypeDictionaryLookup:
                    builder.Append("TYPE_DICTIONARY_LOOKUP: ");
                    ParseSignature(builder);
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_MethodDictionaryLookup:
                    builder.Append("METHOD_DICTIONARY_LOOKUP: ");
                    ParseSignature(builder);
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_TypeHandle:
                    ParseType(builder);
                    builder.Append(" (TYPE_HANDLE)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_MethodHandle:
                    ParseMethod(builder);
                    builder.Append(" (METHOD_HANDLE)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_FieldHandle:
                    builder.Append("FIELD_HANDLE");
                    // TODO
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_MethodEntry:
                    ParseMethod(builder);
                    builder.Append(" (METHOD_ENTRY)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_MethodEntry_DefToken:
                    if (!moduleOverride)
                    {
                        ParseMethodDefToken(builder, owningTypeOverride: null);
                    }
                    builder.Append(" (METHOD_ENTRY");
                    builder.Append(_options.Naked ? ")" : "_DEF_TOKEN)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_MethodEntry_RefToken:
                    ParseMethodRefToken(builder, owningTypeOverride: null);
                    builder.Append(" (METHOD_ENTRY");
                    builder.Append(_options.Naked ? ")" : "_REF_TOKEN)");
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_VirtualEntry:
                    if(!moduleOverride)
                    {
                        ParseMethod(builder);
                    }
                    builder.Append(" (VIRTUAL_ENTRY)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_VirtualEntry_DefToken:
                    ParseMethodDefToken(builder, owningTypeOverride: null);
                    builder.Append(" (VIRTUAL_ENTRY");
                    builder.Append(_options.Naked ? ")" : "_DEF_TOKEN)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_VirtualEntry_RefToken:
                    ParseMethodRefToken(builder, owningTypeOverride: null);
                    builder.Append(" (VIRTUAL_ENTRY");
                    builder.Append(_options.Naked ? ")" : "_REF_TOKEN)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_VirtualEntry_Slot:
                    {
                        uint slot = ReadUInt();
                        if (!moduleOverride)
                        {
                            ParseType(builder);
                        }

                        builder.Append($@" #{slot} (VIRTUAL_ENTRY_SLOT)");
                    }
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_Helper:
                    ParseHelper(builder);
                    builder.Append(" (HELPER)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_StringHandle:
                    ParseStringHandle(builder);
                    builder.Append(" (STRING_HANDLE)");
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_NewObject:
                    ParseType(builder);
                    builder.Append(" (NEW_OBJECT)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_NewArray:
                    ParseType(builder);
                    builder.Append(" (NEW_ARRAY)");
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_IsInstanceOf:
                    ParseType(builder);
                    builder.Append(" (IS_INSTANCE_OF)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_ChkCast:
                    ParseType(builder);
                    builder.Append(" (CHK_CAST)");
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_FieldAddress:
                    ParseField(builder);
                    builder.Append(" (FIELD_ADDRESS)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_CctorTrigger:
                    ParseType(builder);
                    builder.Append(" (CCTOR_TRIGGER)");
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_StaticBaseNonGC:
                    ParseType(builder);
                    builder.Append(" (STATIC_BASE_NON_GC)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_StaticBaseGC:
                    ParseType(builder);
                    builder.Append(" (STATIC_BASE_GC)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_ThreadStaticBaseNonGC:
                    ParseType(builder);
                    builder.Append(" (THREAD_STATIC_BASE_NON_GC)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_ThreadStaticBaseGC:
                    ParseType(builder);
                    builder.Append(" (THREAD_STATIC_BASE_GC)");
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_FieldBaseOffset:
                    ParseType(builder);
                    builder.Append(" (FIELD_BASE_OFFSET)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_FieldOffset:
                    ParseField(builder);
                    builder.Append(" (FIELD_OFFSET)");
                    // TODO
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_TypeDictionary:
                    ParseType(builder);
                    builder.Append(" (TYPE_DICTIONARY)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_MethodDictionary:
                    ParseMethod(builder);
                    builder.Append(" (METHOD_DICTIONARY)");
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_Check_TypeLayout:
                    ParseType(builder);
                    builder.Append(" (CHECK_TYPE_LAYOUT)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_Check_FieldOffset:
                    builder.Append("CHECK_FIELD_OFFSET");
                    // TODO
                    break;


                case ReadyToRunFixupKind.READYTORUN_FIXUP_DelegateCtor:
                    ParseMethod(builder);
                    builder.Append(" => ");
                    ParseType(builder);
                    builder.Append(" (DELEGATE_CTOR)");
                    break;

                case ReadyToRunFixupKind.READYTORUN_FIXUP_DeclaringTypeHandle:
                    ParseType(builder);
                    builder.Append(" (DECLARING_TYPE_HANDLE)");
                    break;


                default:
                    builder.Append(string.Format("Unknown fixup type: {0:X2}", fixupType));
                    break;
            }
        }

        /// <summary>
        /// Decode a type from the signature stream.
        /// </summary>
        /// <param name="builder"></param>
        private void ParseType(StringBuilder builder)
        {
            CorElementType corElemType = ReadElementType();
            switch (corElemType)
            {
                case CorElementType.ELEMENT_TYPE_VOID:
                    builder.Append("void");
                    break;

                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    builder.Append("bool");
                    break;

                case CorElementType.ELEMENT_TYPE_CHAR:
                    builder.Append("char");
                    break;

                case CorElementType.ELEMENT_TYPE_I1:
                    builder.Append("sbyte");
                    break;

                case CorElementType.ELEMENT_TYPE_U1:
                    builder.Append("byte");
                    break;

                case CorElementType.ELEMENT_TYPE_I2:
                    builder.Append("short");
                    break;

                case CorElementType.ELEMENT_TYPE_U2:
                    builder.Append("ushort");
                    break;

                case CorElementType.ELEMENT_TYPE_I4:
                    builder.Append("int");
                    break;

                case CorElementType.ELEMENT_TYPE_U4:
                    builder.Append("uint");
                    break;

                case CorElementType.ELEMENT_TYPE_I8:
                    builder.Append("long");
                    break;

                case CorElementType.ELEMENT_TYPE_U8:
                    builder.Append("ulong");
                    break;

                case CorElementType.ELEMENT_TYPE_R4:
                    builder.Append("float");
                    break;

                case CorElementType.ELEMENT_TYPE_R8:
                    builder.Append("double");
                    break;

                case CorElementType.ELEMENT_TYPE_STRING:
                    builder.Append("string");
                    break;

                case CorElementType.ELEMENT_TYPE_PTR:
                    ParseType(builder);
                    builder.Append('*');
                    break;

                case CorElementType.ELEMENT_TYPE_BYREF:
                    builder.Append("byref");
                    break;

                case CorElementType.ELEMENT_TYPE_VALUETYPE:
                case CorElementType.ELEMENT_TYPE_CLASS:
                    ParseTypeToken(builder);
                    break;

                case CorElementType.ELEMENT_TYPE_VAR:
                    builder.Append("var #");
                    builder.Append(ReadUInt());
                    break;

                case CorElementType.ELEMENT_TYPE_ARRAY:
                    ParseType(builder);
                    {
                        builder.Append('[');
                        uint rank = ReadUInt();
                        if (rank != 0)
                        {
                            uint sizeCount = ReadUInt(); // number of sizes
                            uint[] sizes = new uint[sizeCount];
                            for (uint sizeIndex = 0; sizeIndex < sizeCount; sizeIndex++)
                            {
                                sizes[sizeIndex] = ReadUInt();
                            }
                            uint lowerBoundCount = ReadUInt(); // number of lower bounds
                            int[] lowerBounds = new int[sizeCount];
                            for (uint lowerBoundIndex = 0; lowerBoundIndex < lowerBoundCount; lowerBoundIndex++)
                            {
                                lowerBounds[lowerBoundIndex] = ReadInt();
                            }
                            for (int index = 0; index < rank; index++)
                            {
                                if (index > 0)
                                {
                                    builder.Append(',');
                                }
                                if (lowerBoundCount > index && lowerBounds[index] != 0)
                                {
                                    builder.Append(lowerBounds[index]);
                                    builder.Append("..");
                                    if (sizeCount > index)
                                    {
                                        builder.Append(lowerBounds[index] + sizes[index] - 1);
                                    }
                                }
                                else if (sizeCount > index)
                                {
                                    builder.Append(sizes[index]);
                                }
                                else if (rank == 1)
                                {
                                    builder.Append('*');
                                }
                            }
                        }
                        builder.Append(']');
                    }
                    break;

                case CorElementType.ELEMENT_TYPE_GENERICINST:
                    ParseGenericTypeInstance(builder);
                    break;

                case CorElementType.ELEMENT_TYPE_TYPEDBYREF:
                    builder.Append("typedbyref");
                    break;

                case CorElementType.ELEMENT_TYPE_I:
                    builder.Append("IntPtr");
                    break;

                case CorElementType.ELEMENT_TYPE_U:
                    builder.Append("UIntPtr");
                    break;

                case CorElementType.ELEMENT_TYPE_FNPTR:
                    builder.Append("fnptr");
                    break;

                case CorElementType.ELEMENT_TYPE_OBJECT:
                    builder.Append("object");
                    break;

                case CorElementType.ELEMENT_TYPE_SZARRAY:
                    ParseType(builder);
                    builder.Append("[]");
                    break;

                case CorElementType.ELEMENT_TYPE_MVAR:
                    builder.Append("mvar #");
                    builder.Append(ReadUInt());
                    break;

                case CorElementType.ELEMENT_TYPE_CMOD_REQD:
                    builder.Append("cmod_reqd");
                    break;

                case CorElementType.ELEMENT_TYPE_CMOD_OPT:
                    builder.Append("cmod_opt");
                    break;

                case CorElementType.ELEMENT_TYPE_HANDLE:
                    builder.Append("handle");
                    break;

                case CorElementType.ELEMENT_TYPE_SENTINEL:
                    builder.Append("sentinel");
                    break;

                case CorElementType.ELEMENT_TYPE_PINNED:
                    builder.Append("pinned");
                    break;

                case CorElementType.ELEMENT_TYPE_VAR_ZAPSIG:
                    builder.Append("var_zapsig");
                    break;

                case CorElementType.ELEMENT_TYPE_NATIVE_ARRAY_TEMPLATE_ZAPSIG:
                    builder.Append("native_array_template_zapsig");
                    break;

                case CorElementType.ELEMENT_TYPE_NATIVE_VALUETYPE_ZAPSIG:
                    builder.Append("native_valuetype_zapsig");
                    break;

                case CorElementType.ELEMENT_TYPE_CANON_ZAPSIG:
                    builder.Append("__Canon");
                    break;

                case CorElementType.ELEMENT_TYPE_MODULE_ZAPSIG:
                    builder.Append("module_zapsig");
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        private void ParseGenericTypeInstance(StringBuilder builder)
        {
            ParseType(builder);
            uint typeArgCount = ReadUInt();
            builder.Append("<");
            for (uint paramIndex = 0; paramIndex < typeArgCount; paramIndex++)
            {
                if (paramIndex > 0)
                {
                    builder.Append(", ");
                }
                ParseType(builder);
            }
            builder.Append(">");
        }

        private void ParseTypeToken(StringBuilder builder)
        {
            uint token = ReadToken();
            builder.Append(MetadataNameFormatter.FormatHandle(_metadataReader, MetadataTokens.Handle((int)token)));
        }

        /// <summary>
        /// Parse an arbitrary method signature.
        /// </summary>
        /// <param name="builder">Output string builder to receive the textual signature representation</param>
        private void ParseMethod(StringBuilder builder)
        {
            uint methodFlags = ReadUInt();
            string owningTypeOverride = null;
            if ((methodFlags & (uint)ReadyToRunMethodSigFlags.READYTORUN_METHOD_SIG_OwnerType) != 0)
            {
                SignatureDecoder owningTypeDecoder = new SignatureDecoder(_options, _metadataReader, _image, _offset);
                owningTypeOverride = owningTypeDecoder.ReadTypeSignature();
                _offset = owningTypeDecoder._offset;
            }
            if ((methodFlags & (uint)ReadyToRunMethodSigFlags.READYTORUN_METHOD_SIG_SlotInsteadOfToken) != 0)
            {
                throw new NotImplementedException();
            }
            if ((methodFlags & (uint)ReadyToRunMethodSigFlags.READYTORUN_METHOD_SIG_MemberRefToken) != 0)
            {
                ParseMethodRefToken(builder, owningTypeOverride: owningTypeOverride);
            }
            else
            {
                ParseMethodDefToken(builder, owningTypeOverride: owningTypeOverride);
            }

            if ((methodFlags & (uint)ReadyToRunMethodSigFlags.READYTORUN_METHOD_SIG_MethodInstantiation) != 0)
            {
                uint typeArgCount = ReadUInt();
                builder.Append("<");
                for (int typeArgIndex = 0; typeArgIndex < typeArgCount; typeArgIndex++)
                {
                    if (typeArgIndex != 0)
                    {
                        builder.Append(", ");
                    }
                    ParseType(builder);
                }
                builder.Append(">");
            }

            if ((methodFlags & (uint)ReadyToRunMethodSigFlags.READYTORUN_METHOD_SIG_Constrained) != 0)
            {
                builder.Append(" @ ");
                ParseType(builder);
            }
        }

        /// <summary>
        /// Read a methodDef token from the signature and output the corresponding object to the builder.
        /// </summary>
        /// <param name="builder">Output string builder</param>
        private void ParseMethodDefToken(StringBuilder builder, string owningTypeOverride)
        {
            uint methodDefToken = ReadUInt() | (uint)CorTokenType.mdtMethodDef;
            builder.Append(MetadataNameFormatter.FormatHandle(_metadataReader, MetadataTokens.Handle((int)methodDefToken), namespaceQualified: true, owningTypeOverride: owningTypeOverride));
        }

        /// <summary>
        /// Read a memberRef token from the signature and output the corresponding object to the builder.
        /// </summary>
        /// <param name="builder">Output string builder</param>
        /// <param name="owningTypeOverride">Explicit owning type override</param>
        private void ParseMethodRefToken(StringBuilder builder, string owningTypeOverride)
        {
            uint methodRefToken = ReadUInt() | (uint)CorTokenType.mdtMemberRef;
            builder.Append(MetadataNameFormatter.FormatHandle(_metadataReader, MetadataTokens.Handle((int)methodRefToken), namespaceQualified: false, owningTypeOverride: owningTypeOverride));
        }

        /// <summary>
        /// Parse field signature and output its textual representation into the given string builder.
        /// </summary>
        /// <param name="builder">Output string builder</param>
        private void ParseField(StringBuilder builder)
        {
            uint flags = ReadUInt();
            string owningTypeOverride = null;
            if ((flags & (uint)ReadyToRunFieldSigFlags.READYTORUN_FIELD_SIG_OwnerType) != 0)
            {
                StringBuilder owningTypeBuilder = new StringBuilder();
                ParseType(owningTypeBuilder);
                owningTypeOverride = owningTypeBuilder.ToString();
            }
            uint fieldToken;
            if ((flags & (uint)ReadyToRunFieldSigFlags.READYTORUN_FIELD_SIG_MemberRefToken) != 0)
            {
                fieldToken = ReadUInt() | (uint)CorTokenType.mdtMemberRef;
            }
            else
            {
                fieldToken = ReadUInt() | (uint)CorTokenType.mdtFieldDef;
            }
            builder.Append(MetadataNameFormatter.FormatHandle(_metadataReader, MetadataTokens.Handle((int)fieldToken), namespaceQualified: false, owningTypeOverride: owningTypeOverride));
        }

        /// <summary>
        /// Read R2R helper signature.
        /// </summary>
        /// <returns></returns>
        private void ParseHelper(StringBuilder builder)
        {
            uint helperType = ReadUInt();
            if ((helperType & (uint)ReadyToRunHelper.READYTORUN_HELPER_FLAG_VSD) != 0)
            {
                builder.Append("VSD_");
            }

            switch ((ReadyToRunHelper)(helperType & ~(uint)ReadyToRunHelper.READYTORUN_HELPER_FLAG_VSD))
            {
                case ReadyToRunHelper.READYTORUN_HELPER_Invalid:
                    builder.Append("INVALID");
                    break;

                // Not a real helper - handle to current module passed to delay load helpers.
                case ReadyToRunHelper.READYTORUN_HELPER_Module:
                    builder.Append("MODULE");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_GSCookie:
                    builder.Append("GC_COOKIE");
                    break;


                //
                // Delay load helpers
                //

                // All delay load helpers use custom calling convention:
                // - scratch register - address of indirection cell. 0 = address is inferred from callsite.
                // - stack - section index, module handle
                case ReadyToRunHelper.READYTORUN_HELPER_DelayLoad_MethodCall:
                    builder.Append("DELAYLOAD_METHODCALL");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_DelayLoad_Helper:
                    builder.Append("DELAYLOAD_HELPER");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_DelayLoad_Helper_Obj:
                    builder.Append("DELAYLOAD_HELPER_OBJ");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_DelayLoad_Helper_ObjObj:
                    builder.Append("DELAYLOAD_HELPER_OBJ_OBJ");
                    break;

                // JIT helpers

                // Exception handling helpers
                case ReadyToRunHelper.READYTORUN_HELPER_Throw:
                    builder.Append("THROW");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Rethrow:
                    builder.Append("RETHROW");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Overflow:
                    builder.Append("OVERFLOW");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_RngChkFail:
                    builder.Append("RNG_CHK_FAIL");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_FailFast:
                    builder.Append("FAIL_FAST");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_ThrowNullRef:
                    builder.Append("THROW_NULL_REF");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_ThrowDivZero:
                    builder.Append("THROW_DIV_ZERO");
                    break;

                // Write barriers
                case ReadyToRunHelper.READYTORUN_HELPER_WriteBarrier:
                    builder.Append("WRITE_BARRIER");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_CheckedWriteBarrier:
                    builder.Append("CHECKED_WRITE_BARRIER");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_ByRefWriteBarrier:
                    builder.Append("BYREF_WRITE_BARRIER");
                    break;

                // Array helpers
                case ReadyToRunHelper.READYTORUN_HELPER_Stelem_Ref:
                    builder.Append("STELEM_REF");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Ldelema_Ref:
                    builder.Append("LDELEMA_REF");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_MemSet:
                    builder.Append("MEM_SET");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_MemCpy:
                    builder.Append("MEM_CPY");
                    break;

                // Get string handle lazily
                case ReadyToRunHelper.READYTORUN_HELPER_GetString:
                    builder.Append("GET_STRING");
                    break;

                // Used by /Tuning for Profile optimizations
                case ReadyToRunHelper.READYTORUN_HELPER_LogMethodEnter:
                    builder.Append("LOG_METHOD_ENTER");
                    break;

                // Reflection helpers
                case ReadyToRunHelper.READYTORUN_HELPER_GetRuntimeTypeHandle:
                    builder.Append("GET_RUNTIME_TYPE_HANDLE");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_GetRuntimeMethodHandle:
                    builder.Append("GET_RUNTIME_METHOD_HANDLE");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_GetRuntimeFieldHandle:
                    builder.Append("GET_RUNTIME_FIELD_HANDLE");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Box:
                    builder.Append("BOX");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Box_Nullable:
                    builder.Append("BOX_NULLABLE");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Unbox:
                    builder.Append("UNBOX");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Unbox_Nullable:
                    builder.Append("UNBOX_NULLABLE");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_NewMultiDimArr:
                    builder.Append("NEW_MULTI_DIM_ARR");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_NewMultiDimArr_NonVarArg:
                    builder.Append("NEW_MULTI_DIM_ARR__NON_VAR_ARG");
                    break;

                // Helpers used with generic handle lookup cases
                case ReadyToRunHelper.READYTORUN_HELPER_NewObject:
                    builder.Append("NEW_OBJECT");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_NewArray:
                    builder.Append("NEW_ARRAY");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_CheckCastAny:
                    builder.Append("CHECK_CAST_ANY");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_CheckInstanceAny:
                    builder.Append("CHECK_INSTANCE_ANY");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_GenericGcStaticBase:
                    builder.Append("GENERIC_GC_STATIC_BASE");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_GenericNonGcStaticBase:
                    builder.Append("GENERIC_NON_GC_STATIC_BASE");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_GenericGcTlsBase:
                    builder.Append("GENERIC_GC_TLS_BASE");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_GenericNonGcTlsBase:
                    builder.Append("GENERIC_NON_GC_TLS_BASE");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_VirtualFuncPtr:
                    builder.Append("VIRTUAL_FUNC_PTR");
                    break;

                // Long mul/div/shift ops
                case ReadyToRunHelper.READYTORUN_HELPER_LMul:
                    builder.Append("LMUL");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_LMulOfv:
                    builder.Append("LMUL_OFV");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_ULMulOvf:
                    builder.Append("ULMUL_OVF");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_LDiv:
                    builder.Append("LDIV");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_LMod:
                    builder.Append("LMOD");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_ULDiv:
                    builder.Append("ULDIV");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_ULMod:
                    builder.Append("ULMOD");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_LLsh:
                    builder.Append("LLSH");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_LRsh:
                    builder.Append("LRSH");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_LRsz:
                    builder.Append("LRSZ");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Lng2Dbl:
                    builder.Append("LNG2DBL");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_ULng2Dbl:
                    builder.Append("ULNG2DBL");
                    break;

                // 32-bit division helpers
                case ReadyToRunHelper.READYTORUN_HELPER_Div:
                    builder.Append("DIV");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Mod:
                    builder.Append("MOD");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_UDiv:
                    builder.Append("UDIV");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_UMod:
                    builder.Append("UMOD");
                    break;

                // Floating point conversions
                case ReadyToRunHelper.READYTORUN_HELPER_Dbl2Int:
                    builder.Append("DBL2INT");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Dbl2IntOvf:
                    builder.Append("DBL2INTOVF");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Dbl2Lng:
                    builder.Append("DBL2LNG");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Dbl2LngOvf:
                    builder.Append("DBL2LNGOVF");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Dbl2UInt:
                    builder.Append("DBL2UINT");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Dbl2UIntOvf:
                    builder.Append("DBL2UINTOVF");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Dbl2ULng:
                    builder.Append("DBL2ULNG");
                    break;

                case ReadyToRunHelper.READYTORUN_HELPER_Dbl2ULngOvf:
                    builder.Append("DBL2ULNGOVF");
                    break;

                // Floating point ops
                case ReadyToRunHelper.READYTORUN_HELPER_DblRem:
                    builder.Append("DBL_REM");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_FltRem:
                    builder.Append("FLT_REM");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_DblRound:
                    builder.Append("DBL_ROUND");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_FltRound:
                    builder.Append("FLT_ROUND");
                    break;

                // Personality rountines
                case ReadyToRunHelper.READYTORUN_HELPER_PersonalityRoutine:
                    builder.Append("PERSONALITY_ROUTINE");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_PersonalityRoutineFilterFunclet:
                    builder.Append("PERSONALITY_ROUTINE_FILTER_FUNCLET");
                    break;

                //
                // Deprecated/legacy
                //

                // JIT32 x86-specific write barriers
                case ReadyToRunHelper.READYTORUN_HELPER_WriteBarrier_EAX:
                    builder.Append("WRITE_BARRIER_EAX");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_WriteBarrier_EBX:
                    builder.Append("WRITE_BARRIER_EBX");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_WriteBarrier_ECX:
                    builder.Append("WRITE_BARRIER_ECX");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_WriteBarrier_ESI:
                    builder.Append("WRITE_BARRIER_ESI");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_WriteBarrier_EDI:
                    builder.Append("WRITE_BARRIER_EDI");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_WriteBarrier_EBP:
                    builder.Append("WRITE_BARRIER_EBP");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_CheckedWriteBarrier_EAX:
                    builder.Append("CHECKED_WRITE_BARRIER_EAX");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_CheckedWriteBarrier_EBX:
                    builder.Append("CHECKED_WRITE_BARRIER_EBX");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_CheckedWriteBarrier_ECX:
                    builder.Append("CHECKED_WRITE_BARRIER_ECX");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_CheckedWriteBarrier_ESI:
                    builder.Append("CHECKED_WRITE_BARRIER_ESI");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_CheckedWriteBarrier_EDI:
                    builder.Append("CHECKED_WRITE_BARRIER_EDI");
                    break;
                case ReadyToRunHelper.READYTORUN_HELPER_CheckedWriteBarrier_EBP:
                    builder.Append("CHECKED_WRITE_BARRIER_EBP");
                    break;

                // JIT32 x86-specific exception handling
                case ReadyToRunHelper.READYTORUN_HELPER_EndCatch:
                    builder.Append("END_CATCH");
                    break;

                default:
                    builder.Append(string.Format("Unknown helper: {0:X2}", helperType));
                    break;
            }
        }

        /// <summary>
        /// Read a string token from the signature stream and convert it to the actual string.
        /// </summary>
        /// <returns></returns>
        private void ParseStringHandle(StringBuilder builder)
        {
            uint rid = ReadUInt();
            UserStringHandle stringHandle = MetadataTokens.UserStringHandle((int)rid);
            builder.Append(_metadataReader.GetUserString(stringHandle));
        }
    }
}

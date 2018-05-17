// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace R2RDump
{
    struct SignatureType
    {
        /// <summary>
        /// The SignatureTypeCode can be a primitive type, TypeHandle for objects, ByReference for references
        /// </summary>
        public SignatureTypeCode SignatureTypeCode { get; }

        /// <summary>
        /// Indicates if the type is an array
        /// </summary>
        public bool IsArray { get; }

        /// <summary>
        /// Name of the object or primitive type
        /// </summary>
        public string ClassName { get; }

        public SignatureType(ref BlobReader signatureReader, ref MetadataReader mdReader)
        {
            SignatureTypeCode = signatureReader.ReadSignatureTypeCode();
            IsArray = (SignatureTypeCode == SignatureTypeCode.SZArray);
            if (IsArray)
            {
                SignatureTypeCode = signatureReader.ReadSignatureTypeCode();
            }
            ClassName = SignatureTypeCode.ToString();
            if (SignatureTypeCode == SignatureTypeCode.TypeHandle || SignatureTypeCode == SignatureTypeCode.ByReference)
            {
                EntityHandle handle = signatureReader.ReadTypeHandle();
                if (handle.Kind == HandleKind.TypeDefinition)
                {
                    var typeDef = mdReader.GetTypeDefinition((TypeDefinitionHandle)handle);
                    ClassName = mdReader.GetString(typeDef.Name);
                }
                else if (handle.Kind == HandleKind.TypeReference)
                {
                    var typeRef = mdReader.GetTypeReference((TypeReferenceHandle)handle);
                    ClassName = mdReader.GetString(typeRef.Name);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (SignatureTypeCode == SignatureTypeCode.ByReference)
            {
                sb.Append("ref ");
            }
            sb.AppendFormat($"{ClassName}");
            if (IsArray)
            {
                sb.Append("[]");
            }
            return sb.ToString();
        }
    }

    struct RuntimeFunction
    {
        /// <summary>
        /// The relative virtual address to the start of the code block
        /// </summary>
        public int EntryPoint { get; set; }

        /// <summary>
        /// The size of the code block in bytes
        /// </summary>
        /// /// <remarks>
        /// The EndAddress field in the runtime functions section is conditional on machine type
        /// Size is -1 for images without the EndAddress field
        /// </remarks>
        public int Size { get; set; }

        /// <summary>
        /// The relative virtual address to the unwind info
        /// </summary>
        public int UnwindRVA { get; set; }

        public RuntimeFunction(int startRva, int endRva, int unwindRva)
        {
            EntryPoint = startRva;
            Size = endRva - startRva;
            if (endRva == -1)
                Size = -1;
            UnwindRVA = unwindRva;
        }
    }

    class R2RMethod
    {
        private const int _mdtMethodDef = 0x06000000;

        public string Name { get; }
        public SignatureType ReturnType { get; }
        public SignatureType[] ArgTypes { get; }

        /// <summary>
        /// The token of the method consisting of the table code (0x06) and row id
        /// </summary>
        public int Token { get; }

        /// <summary>
        /// The entry point to the native code and the RVA of the unwind info
        /// </summary>
        public RuntimeFunction NativeCode { get; }

        public R2RMethod(byte[] image, RuntimeFunction[] runtimeFunctions, ref MetadataReader mdReader, MethodDefinitionHandle methodDefHandle, NativeArray methodEntryPoints, uint offset, int rid) 
            : this(image, runtimeFunctions, methodEntryPoints, offset, rid)
        {
            var methodDef = mdReader.GetMethodDefinition(methodDefHandle);
            BlobReader signatureReader = mdReader.GetBlobReader(methodDef.Signature);
            SignatureHeader header = signatureReader.ReadSignatureHeader();
            Name = mdReader.GetString(methodDef.Name);
            int argCount = signatureReader.ReadCompressedInteger();
            ReturnType = new SignatureType(ref signatureReader, ref mdReader);
            ArgTypes = new SignatureType[argCount];
            for (int i = 0; i < argCount; i++)
            {
                ArgTypes[i] = new SignatureType(ref signatureReader, ref mdReader);
            }
        }

        public R2RMethod(byte[] image, RuntimeFunction[] runtimeFunctions, NativeArray methodEntryPoints, uint offset, int rid)
        {
            Token = _mdtMethodDef + rid;

            uint id = 0; // the RUNTIME_FUNCTIONS index
            offset = methodEntryPoints.DecodeUnsigned(image, offset, ref id);
            if ((id & 1) != 0)
            {
                if ((id & 2) != 0)
                {
                    uint val = 0;
                    methodEntryPoints.DecodeUnsigned(image, offset, ref val);
                    offset -= val;
                }
                // TODO: Dump fixups

                id >>= 2;
            }
            else
            {
                id >>= 1;
            }
            NativeCode = runtimeFunctions[id];

            Name = "";
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (Name.Length > 0) {
                sb.AppendFormat($"{ReturnType.ToString()} {Name}(");
                for (int i = 0; i < ArgTypes.Length - 1; i++)
                {
                    sb.AppendFormat($"{ArgTypes[i].ToString()}, ");
                }
                if (ArgTypes.Length > 0) {
                    sb.AppendFormat($"{ArgTypes[ArgTypes.Length - 1].ToString()}");
                }
                sb.Append(")\n");
            }

            sb.AppendFormat($"Token: 0x{Token:X8}\n");
            sb.AppendFormat($"EntryPoint: 0x{NativeCode.EntryPoint:X8}\n");
            if (NativeCode.Size != -1) {
                sb.AppendFormat($"Size: {NativeCode.Size} bytes\n");
            }
            return sb.ToString();
        }
    }
}

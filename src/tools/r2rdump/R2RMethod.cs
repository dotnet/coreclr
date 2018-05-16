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
        public SignatureTypeCode SignatureTypeCode { get; }
        public bool IsArray { get; }
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
        private const int _blockSize = 16;

        public string Name { get; }
        public SignatureType ReturnType { get; }
        public SignatureType[] ArgTypes { get; }
        public int Token { get; }

        public RuntimeFunction NativeCode { get; }

        public R2RMethod(byte[] image, RuntimeFunction[] runtimeFunctions, ref MetadataReader mdReader, MethodDefinitionHandle methodDefHandle, int methodDefEntryPointsOffset)
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

            Token = MetadataTokens.GetToken(methodDefHandle);
            int rid = MetadataTokens.GetRowNumber(methodDefHandle);

            int val = 0;
            int baseOffset = DecodeUnsigned(image, methodDefEntryPointsOffset, ref val);
            int nElements = (int)(val >> 2);
            int entryIndexSize = (int)(val & 3);
            int offset = MethodDefEntryPointsTryGetAt(image, rid - 1, (int)baseOffset, nElements, entryIndexSize);
            int id = 0;
            offset = DecodeUnsigned(image, offset, ref id);
            if ((id & 1)!=0)
            {
                if ((id & 2)!= 0)
                {
                    val = 0;
                    DecodeUnsigned(image, offset, ref val);
                    offset -= val;
                }
                id >>= 2;
            }
            else
            {
                id >>= 1;
            }
            NativeCode = runtimeFunctions[id];
        }

        private int MethodDefEntryPointsTryGetAt(byte[] image, int index, int baseOffset, int nElements, int entryIndexSize)
        {
            if (index >= nElements)
                throw new System.BadImageFormatException("MethodDefEntryPoints");

            int offset = 0;
            if (entryIndexSize == 0)
            {
                int i = baseOffset + (index / _blockSize);
                offset = image[i];
            }
            else if (entryIndexSize == 1)
            {
                int i = baseOffset + 2 * (index / _blockSize);
                offset = R2RReader.ReadUInt16(image, ref i);
            }
            else
            {
                int i = baseOffset + 4 * (index / _blockSize);
                offset = R2RReader.ReadInt32(image, ref i);
            }
            offset += baseOffset;

            for (uint bit = _blockSize >> 1; bit > 0; bit >>= 1)
            {
                int val = 0;
                int offset2 = DecodeUnsigned(image, offset, ref val);
                if ((index & bit) != 0)
                {
                    if ((val & 2) != 0)
                    {
                        offset = offset + (val >> 2);
                        continue;
                    }
                }
                else
                {
                    if ((val & 1) != 0)
                    {
                        offset = offset2;
                        continue;
                    }
                }

                // Not found
                if ((val & 3) == 0)
                {
                    // Matching special leaf node?
                    if ((val >> 2) == (index & (_blockSize - 1)))
                    {
                        offset = offset2;
                        break;
                    }
                }
                throw new System.BadImageFormatException("MethodDefEntryPoints");
            }
            return offset;
        }

        private int DecodeUnsigned(byte[] image, int offset, ref int pValue)
        {
            int val = image[offset];

            if ((val & 1) == 0)
            {
                pValue = (val >> 1);
                offset += 1;
            }
            else if ((val & 2) == 0)
            {
                pValue = (val >> 2) |
                      (image[offset + 1] << 6);
                offset += 2;
            }
            else if ((val & 4) == 0)
            {
                pValue = (val >> 3) |
                      (image[offset + 1] << 5) |
                      (image[offset + 2] << 13);
                offset += 3;
            }
            else if ((val & 8) == 0)
            {
                pValue = (val >> 4) |
                      (image[offset + 1] << 4) |
                      (image[offset + 2] << 12) |
                      (image[offset + 3] << 20);
                offset += 4;
            }
            else if ((val & 16) == 0)
            {
                pValue = image[offset + 1];
                offset += 5;
            }
            else
            {
                throw new System.BadImageFormatException("MethodDefEntryPoints");
            }

            return offset;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"{ReturnType.ToString()} {Name}(");
            for (int i = 0; i < ArgTypes.Length - 1; i++)
            {
                sb.AppendFormat($"{ArgTypes[i].ToString()}, ");
            }
            if (ArgTypes.Length > 0) {
                sb.AppendFormat($"{ArgTypes[ArgTypes.Length - 1].ToString()}");
            }
            sb.Append(")\n");

            sb.AppendFormat($"Token: 0x{Token:X8}\n");
            sb.AppendFormat($"EntryPoint: 0x{NativeCode.EntryPoint:X8}\n");
            if (NativeCode.Size != -1) {
                sb.AppendFormat($"Size: {NativeCode.Size} bytes\n");
            }
            return sb.ToString();
        }
    }
}

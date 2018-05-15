// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection.Metadata;
using System.Text;

namespace R2RDump
{
    struct SignatureType
    {
        public SignatureTypeCode SignatureTypeCode { get; }
        public bool IsArray { get; set; }

        public SignatureType(SignatureTypeCode signatureTypeCode)
        {
            SignatureTypeCode = signatureTypeCode;
            IsArray = false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"{SignatureTypeCode.ToString().ToLower()}");
            if (IsArray)
            {
                sb.Append("[]");
            }
            return sb.ToString();
        }
    }

    class R2RMethod
    {
        public string Name { get; }
        public SignatureType ReturnType { get; }
        public SignatureType[] ArgTypes { get; }
        int ArgCount { get; }

        /// <summary>
        /// The relative virtual address to the start of the code block
        /// </summary>
        public int EntryPoint { get; set; }

        /// <summary>
        /// The size of the code block in bytes
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// The relative virtual address to the unwind info
        /// </summary>
        public int UnwindRVA { get; set; }

        public R2RMethod(byte[] image, ref MetadataReader mdReader, ref MethodDefinition methodDef)
        {
            BlobReader signatureReader = mdReader.GetBlobReader(methodDef.Signature);
            SignatureHeader header = signatureReader.ReadSignatureHeader();
            Name = mdReader.GetString(methodDef.Name);
            ArgCount = signatureReader.ReadCompressedInteger();
            ReturnType = DecodeSignatureType(ref signatureReader);
            ArgTypes = new SignatureType[ArgCount];
            for (int i = 0; i < ArgCount; i++)
            {
                ArgTypes[i] = DecodeSignatureType(ref signatureReader);
            }
        }

        internal void SetRVA(int startRva, int endRva, int unwindRva)
        {
            EntryPoint = startRva;
            Size = endRva - startRva;
            UnwindRVA = unwindRva;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"{ReturnType.ToString()} {Name}(");
            for (int i = 0; i < ArgCount - 1; i++)
            {
                sb.AppendFormat($"{ArgTypes[i].ToString()}, ");
            }
            if (ArgCount > 0) {
                sb.AppendFormat($"{ArgTypes[ArgCount - 1].ToString()}");
            }
            sb.Append(")\n");

            sb.AppendFormat($"EntryPoint: 0x{EntryPoint:X8}\n");
            sb.AppendFormat($"Size: {Size} bytes\n");
            return sb.ToString();
        }

        private SignatureType DecodeSignatureType(ref BlobReader signatureReader)
        {
            SignatureType signatureType = new SignatureType(signatureReader.ReadSignatureTypeCode());
            switch (signatureType.SignatureTypeCode)
            {
                case SignatureTypeCode.SZArray:
                    signatureType = DecodeSignatureType(ref signatureReader);
                    signatureType.IsArray = true;
                    break;
            }
            return signatureType;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection.Metadata;
using System.Text;

namespace R2RDump
{
    class SignatureType
    {
        /// <summary>
        /// Indicates if the type is an array
        /// </summary>
        public SignatureTypeFlags Flags { get; }

        /// <summary>
        /// Name of the object or primitive type
        /// </summary>
        public string TypeName { get; }

        public R2RMethod.GenericElementTypes GenericInstance { get; set; }

        public enum SignatureTypeFlags
        {
            ARRAY = 0x01,
            REFERENCE = 0x02,
            GENERIC = 0x04,
        };

        public SignatureType(ref BlobReader signatureReader, MetadataReader mdReader, GenericParameterHandleCollection genericParams)
        {
            SignatureTypeCode signatureTypeCode = signatureReader.ReadSignatureTypeCode();
            Flags = 0;
            if (signatureTypeCode == SignatureTypeCode.SZArray)
            {
                Flags |= SignatureTypeFlags.ARRAY;
                signatureTypeCode = signatureReader.ReadSignatureTypeCode();
            }

            TypeName = signatureTypeCode.ToString();
            if (signatureTypeCode == SignatureTypeCode.TypeHandle || signatureTypeCode == SignatureTypeCode.ByReference)
            {
                if (signatureTypeCode == SignatureTypeCode.ByReference)
                {
                    Flags |= SignatureTypeFlags.REFERENCE;
                }

                EntityHandle handle = signatureReader.ReadTypeHandle();
                if (handle.Kind == HandleKind.TypeDefinition)
                {
                    var typeDef = mdReader.GetTypeDefinition((TypeDefinitionHandle)handle);
                    TypeName = mdReader.GetString(typeDef.Name);
                }
                else if (handle.Kind == HandleKind.TypeReference)
                {
                    var typeRef = mdReader.GetTypeReference((TypeReferenceHandle)handle);
                    TypeName = mdReader.GetString(typeRef.Name);
                }
            }
            else if (signatureTypeCode == SignatureTypeCode.GenericMethodParameter)
            {
                int index = signatureReader.ReadCompressedInteger();
                var generic = mdReader.GetGenericParameter(genericParams[index]);
                TypeName = mdReader.GetString(generic.Name);
                Flags |= SignatureTypeFlags.GENERIC;
            }

            GenericInstance = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if ((Flags & SignatureTypeFlags.REFERENCE) != 0)
            {
                sb.Append("ref ");
            }

            if ((Flags & SignatureTypeFlags.GENERIC) != 0)
            {
                sb.AppendFormat($"{Enum.GetName(typeof(R2RMethod.GenericElementTypes), GenericInstance)}");
            }
            else
            {
                sb.AppendFormat($"{TypeName}");
            }
            if ((Flags & SignatureTypeFlags.ARRAY) != 0)
            {
                sb.Append("[]");
            }
            return sb.ToString();
        }
    }
}

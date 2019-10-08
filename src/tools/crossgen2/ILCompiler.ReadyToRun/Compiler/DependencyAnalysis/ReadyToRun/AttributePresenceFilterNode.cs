// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;

using Internal.JitInterface;
using Internal.NativeFormat;
using Internal.Text;
using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;

namespace ILCompiler.DependencyAnalysis.ReadyToRun
{
    public class AttributePresenceFilterNode : HeaderTableNode
    {
        private EcmaModule _module;

        public override int ClassCode => 56456113;

        public AttributePresenceFilterNode(EcmaModule module, TargetDetails target)
            : base(target)
        {
            this._module = module;
        }

        public override void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
            sb.Append(nameMangler.CompilationUnitPrefix);
            sb.Append("__ReadyToRunAttributePresenceFilter");
        }

        private struct CustomAttributeEntry
        {
            public string TypeNamespace;
            public string TypeName;
            public int Parent;
        }

        private List<CustomAttributeEntry> GetCustomAttributeEntries()
        {
            MetadataReader reader = _module.MetadataReader;
            List<CustomAttributeEntry> customAttributeEntries = new List<CustomAttributeEntry>();
            foreach (var handle in reader.CustomAttributes)
            {
                CustomAttribute customAttribute = reader.GetCustomAttribute(handle);
                EntityHandle customAttributeConstructorHandle = customAttribute.Constructor;
                MethodDesc customAttributeConstructorMethodDesc = this._module.GetMethod(customAttributeConstructorHandle);
                DefType customAttributeDefType = customAttributeConstructorMethodDesc.OwningType as DefType;
                Debug.Assert(customAttributeDefType != null);
                string customAttributeTypeNamespace = customAttributeDefType.Namespace;
                string customAttributeTypeName = customAttributeDefType.Name;
                // System.Runtime.CompilerServices.NullableAttribute is NEVER added to the table (There are *many* of these, and they provide no useful value to the runtime)
                if (customAttributeTypeNamespace == "System.Runtime.CompilerServices" && customAttributeTypeName == "NullableAttribute")
                {
                    continue;
                }
                bool addToTable = false;
                if (customAttributeTypeNamespace.StartsWith("System.Runtime."))
                {
                    addToTable = true;
                }
                else if (customAttributeTypeNamespace == "Windows.Foundation.Metadata")
                {
                    // Windows.Foundation.Metadata attributes are a similar construct to compilerservices attributes. Add them to the table
                    addToTable = true;
                }
                else if (customAttributeTypeNamespace == "System")
                {
                    // Some historical well known attributes were placed in the System namespace. Special case them
                    if (customAttributeTypeName == "ParamArrayAttribute")
                    {
                        addToTable = true;
                    }
                    else if (customAttributeTypeName == "ThreadStaticAttribute")
                    {
                        addToTable = true;
                    }
                }
                else if (customAttributeTypeNamespace == "System.Reflection")
                {
                    // Historical attribute in the System.Reflection namespace
                    if (customAttributeTypeName == "DefaultMemberAttribute")
                    {
                        addToTable = true;
                    }
                }

                if (!addToTable)
                    continue;

                customAttributeEntries.Add(new CustomAttributeEntry
                {
                    TypeNamespace = customAttributeTypeNamespace,
                    TypeName = customAttributeTypeName,
                    Parent = reader.GetToken(customAttribute.Parent)
                }); ;
            }
            return customAttributeEntries;
        }

        private uint xorshift128(uint[] state)
        {
            uint s, t = state[3];
            state[3] = state[2];
            state[2] = state[1];
            state[1] = s = state[0];
            t ^= t << 11;
            t ^= t >> 8;
            return state[0] = t ^ s ^ (s >> 19);
        }

        public override ObjectData GetData(NodeFactory factory, bool relocsOnly = false)
        {
            // This node does not trigger generation of other nodes.
            if (relocsOnly)
                return new ObjectData(Array.Empty<byte>(), Array.Empty<Relocation>(), 1, new ISymbolDefinitionNode[] { this });

            List<CustomAttributeEntry> customAttributeEntries = GetCustomAttributeEntries();
            int countOfEntries = customAttributeEntries.Count;
            if (countOfEntries == 0)
            {
                return new ObjectData(Array.Empty<byte>(), Array.Empty<Relocation>(), 1, new ISymbolDefinitionNode[] { this });
            }

            // Buckets have 8 entries
            uint minTableBucketCount = (uint)(countOfEntries / 8) + 1;
            uint bucketCount = 1;

            // Bucket count must be power of two
            while (bucketCount < minTableBucketCount)
            {
                bucketCount *= 2;
            }

            // Resize the array.
            bool tryAgainWithBiggerTable = false;
            int countOfRetries = 0;
            ushort[] pTable;
            do
            {
                tryAgainWithBiggerTable = false;
                uint actualSizeOfTable = bucketCount * 8; // Buckets have 8 entries in them
                pTable = new ushort[actualSizeOfTable];
                uint[] state = new uint[] {729055690, 833774698, 218408041, 493449127};
                // Attempt to fill table
                foreach (var customAttributeEntry in customAttributeEntries)
                {
                    string szNamespace = customAttributeEntry.TypeNamespace;
                    string szName = customAttributeEntry.TypeName;
                    int tkParent = customAttributeEntry.Parent;
                    string name = szNamespace + "." + szName;
                    // This hashing algorithm MUST match exactly the logic in NativeCuckooFilter
                    int hashOfAttribute = ReadyToRunHashCode.NameHashCode(name);
                    uint hash = unchecked((uint)System.HashCode.Combine(hashOfAttribute, tkParent));
                    ushort fingerprint = (ushort)(hash >> 16);
                    if (fingerprint == 0)
                    {
                        fingerprint = 1;
                    }
                    uint bucketAIndex = hash % bucketCount;
                    uint fingerprintHash = (uint)fingerprint;
                    uint bucketBIndex = (bucketAIndex ^ (fingerprintHash % bucketCount));
                    Debug.Assert(bucketAIndex == (bucketBIndex ^ (fingerprintHash % bucketCount)));
                    if ((xorshift128(state) & 1) != 0) // Randomly choose which bucket to attempt to fill first
                    {
                        uint temp = bucketAIndex;
                        bucketAIndex = bucketBIndex;
                        bucketBIndex = temp;
                    }
                    Func<uint, ushort, bool> hasEntryInBucket = (uint bucketIndex, ushort fprint) =>
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (pTable[(bucketIndex * 8) + i] == fprint)
                            {
                                return true;
                            }
                        }
                        return false;
                    };
                    Func<uint, bool> isEmptyEntryInBucket = (uint bucketIndex) =>
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (pTable[(bucketIndex * 8) + i] == 0)
                            {
                                return true;
                            }
                        }
                        return false;
                    };
                    Action<uint, ushort> fillEmptyEntryInBucket = (uint bucketIndex, ushort fprint) =>
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (pTable[(bucketIndex * 8) + i] == 0)
                            {
                                pTable[(bucketIndex * 8) + i] = fprint;
                                return;
                            }
                        }
                        Debug.Assert(false, "Not possible to reach here");
                    };
                    // Scan for pre-existing fingerprint entry in buckets
                    if (hasEntryInBucket(bucketAIndex, fingerprint) || hasEntryInBucket(bucketBIndex, fingerprint))
                        continue;

                    // Determine if there is space in a bucket to add a new entry
                    if (isEmptyEntryInBucket(bucketAIndex))
                    {
                        fillEmptyEntryInBucket(bucketAIndex, fingerprint);
                        continue;
                    }
                    if (isEmptyEntryInBucket(bucketBIndex))
                    {
                        fillEmptyEntryInBucket(bucketBIndex, fingerprint);
                        continue;
                    }
                    int MaxNumKicks = 256;
                    // Note, that bucketAIndex itself was chosen randomly above.
                    for (int n = 0; n < MaxNumKicks; n++)
                    {
                        // Randomly swap an entry in bucket bucketAIndex with fingerprint
                        uint entryIndexInBucket = xorshift128(state) & 0x7;
                        ushort temp = fingerprint;
                        fingerprint = pTable[(bucketAIndex * 8) + entryIndexInBucket];
                        pTable[(bucketAIndex * 8) + entryIndexInBucket] = temp;

                        // Find other bucket
                        fingerprintHash = (uint)fingerprint;
                        bucketAIndex = bucketAIndex ^ (fingerprintHash % bucketCount);
                        if (isEmptyEntryInBucket(bucketAIndex))
                        {
                            fillEmptyEntryInBucket(bucketAIndex, fingerprint);
                            continue;
                        }
                    }

                    tryAgainWithBiggerTable = true;
                }

                if (tryAgainWithBiggerTable)
                {
                    // bucket entry kicking path requires bucket counts to be power of two in size due to use of xor to retrieve second hash
                    bucketCount *= 2;
                }
            }
            while(tryAgainWithBiggerTable && ((countOfRetries++) < 2));
            
            if (tryAgainWithBiggerTable)
            {
                return new ObjectData(Array.Empty<byte>(), Array.Empty<Relocation>(), 1, new ISymbolDefinitionNode[] { this });
            }
            else
            {
                byte[] result = new byte[pTable.Length * 2];
                for (int i = 0; i < pTable.Length; i++)
                {
                    result[i * 2] = (byte)(pTable[i] % 256);
                    result[i * 2 + 1] = (byte)(pTable[i] >> 8);
                }
                return new ObjectData(result, Array.Empty<Relocation>(), 1, new ISymbolDefinitionNode[] { this });
            }
        }
    }
}

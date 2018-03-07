// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Reflection;
using EventMetadata = System.Diagnostics.Tracing.EventSource.EventMetadata;

namespace System.Diagnostics.Tracing
{
#if FEATURE_PERFTRACING

    internal sealed class EventPipeMetadataGenerator
    {
        public static EventPipeMetadataGenerator Instance = new EventPipeMetadataGenerator();

        private EventPipeMetadataGenerator() { }

        public unsafe byte[] GenerateEventMetadata(EventMetadata eventMetadata)
        {
            ParameterInfo[] parameters = eventMetadata.Parameters;
            EventParameterInfo[] eventParams = new EventParameterInfo[parameters.Length];
            for(int i=0; i<parameters.Length; i++)
            {
                eventParams[i].SetInfo(parameters[i].Name, parameters[i].ParameterType);
            }

            return GenerateMetadata(
                eventMetadata.Descriptor.EventId,
                eventMetadata.Name,
                eventMetadata.Descriptor.Keywords,
                eventMetadata.Descriptor.Level,
                eventMetadata.Descriptor.Version,
                eventParams);
        }

        public unsafe byte[] GenerateEventMetadata(
            int eventId,
            string eventName,
            EventKeywords keywords,
            EventLevel level,
            uint version,
            TraceLoggingTypeInfo[] parameters)
        {
            EventParameterInfo[] eventParams = new EventParameterInfo[parameters.Length];
            for(int i=0; i<parameters.Length; i++)
            {
                eventParams[i].SetInfo(parameters[i].Name, parameters[i].DataType, parameters[i]);
            }

            return GenerateMetadata(eventId, eventName, (long)keywords, (uint)level, version, eventParams);
        }

        private unsafe byte[] GenerateMetadata(
            int eventId,
            string eventName,
            long keywords,
            uint level,
            uint version,
            EventParameterInfo[] parameters)
        {
            // eventID          : 4 bytes
            // eventName        : (eventName.Length + 1) * 2 bytes
            // keywords         : 8 bytes
            // eventVersion     : 4 bytes
            // level            : 4 bytes
            // parameterCount   : 4 bytes
            uint metadataLength = 24 + ((uint)eventName.Length + 1) * 2;

            // Increase the metadataLength for parameters.
            foreach (var parameter in parameters)
            {
                metadataLength = metadataLength + parameter.GetMetadataLength();
            }

            byte[] metadata = new byte[metadataLength];

            // Write metadata: eventID, eventName, keywords, eventVersion, level, parameterCount, param1 type, param1 name...
            fixed (byte *pMetadata = metadata)
            {
                uint offset = 0;
                WriteToBuffer(pMetadata, metadataLength, ref offset, (uint)eventId);
                fixed(char *pEventName = eventName)
                {
                    WriteToBuffer(pMetadata, metadataLength, ref offset, (byte *)pEventName, ((uint)eventName.Length + 1) * 2);
                }
                WriteToBuffer(pMetadata, metadataLength, ref offset, keywords);
                WriteToBuffer(pMetadata, metadataLength, ref offset, version);
                WriteToBuffer(pMetadata, metadataLength, ref offset, level);
                WriteToBuffer(pMetadata, metadataLength, ref offset, (uint)parameters.Length);
                foreach (var parameter in parameters)
                {
                    parameter.GenerateMetadata(pMetadata, ref offset, metadataLength);
                }
                Debug.Assert(metadataLength == offset);
            }

            return metadata;
        }

        // Copy src to buffer and modify the offset.
        // Note: We know the buffer size ahead of time to make sure no buffer overflow.
        internal static unsafe void WriteToBuffer(byte *buffer, uint bufferLength, ref uint offset, byte *src, uint srcLength)
        {
            Debug.Assert(bufferLength >= (offset + srcLength));
            for (int i = 0; i < srcLength; i++)
            {
                *(byte *)(buffer + offset + i) = *(byte *)(src + i);
            }
            offset += srcLength;
        }

        // Copy uint value to buffer.
        internal static unsafe void WriteToBuffer(byte *buffer, uint bufferLength, ref uint offset, uint value)
        {
            Debug.Assert(bufferLength >= (offset + 4));
            *(uint *)(buffer + offset) = value;
            offset += 4;
        }

        // Copy long value to buffer.
        internal static unsafe void WriteToBuffer(byte *buffer, uint bufferLength, ref uint offset, long value)
        {
            Debug.Assert(bufferLength >= (offset + 8));
            *(long *)(buffer + offset) = value;
            offset += 8;
        }
    }

    internal struct EventParameterInfo
    {
        internal string ParameterName;
        internal Type ParameterType;
        internal TraceLoggingTypeInfo TypeInfo;

        internal void SetInfo(string name, Type type, TraceLoggingTypeInfo typeInfo = null)
        {
            ParameterName = name;
            ParameterType = type;
            TypeInfo = typeInfo;
        }

        internal unsafe void GenerateMetadata(byte* pMetadataBlob, ref uint offset, uint blobSize)
        {
            TypeCode typeCode = GetTypeCodeExtended(ParameterType);
            if(typeCode == TypeCode.Object)
            {
                Debug.Assert(false);
            }
            else
            {
                // Write parameter type.
                EventPipeMetadataGenerator.WriteToBuffer(pMetadataBlob, blobSize, ref offset, (uint)typeCode);

                // Write parameter name.
                fixed (char *pParameterName = ParameterName)
                {
                    EventPipeMetadataGenerator.WriteToBuffer(pMetadataBlob, blobSize, ref offset, (byte *)pParameterName, ((uint)ParameterName.Length + 1) * 2);
                }
            }
        }

        internal unsafe uint GetMetadataLength()
        {
            uint ret = 0;

            TypeCode typeCode = GetTypeCodeExtended(ParameterType);
            if(typeCode == TypeCode.Object)
            {
                Debug.Assert(false);
            }
            else
            {
                ret = (uint)(sizeof(uint) + ((ParameterName.Length + 1) * 2));
            }

            return ret;
        }

        private static TypeCode GetTypeCodeExtended(Type parameterType)
        {
            // Guid is not part of TypeCode, we decided to use 17 to represent it, as it's the "free slot"
            // see https://github.com/dotnet/coreclr/issues/16105#issuecomment-361749750 for more
            const TypeCode GuidTypeCode = (TypeCode)17;

            if (parameterType == typeof(Guid)) // Guid is not a part of TypeCode enum
                return GuidTypeCode;

            return Type.GetTypeCode(parameterType);
        }
    }

#endif // FEATURE_PERFTRACING
}

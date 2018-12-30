// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Diagnostics.Tracing
{
#if FEATURE_PERFTRACING
    internal static class EventPipePayloadDecoder
    {
        /// <summary>
        /// Given the metadata for an event and an event payload, decode and deserialize the event payload.
        /// </summary>
        internal static object[] DecodePayload(ref EventSource.EventMetadata metadata, ReadOnlySpan<byte> payload)
        {
            ParameterInfo[] parameters = metadata.Parameters;
            object[] decodedFields = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                // It is possible that an older version of the event was emitted.
                // If this happens, the payload might be missing arguments at the end.
                // We can just leave these unset.
                if (payload.Length <= 0)
                {
                    break;
                }

                Type parameterType = parameters[i].ParameterType;
                if (parameterType == typeof(IntPtr))
                {
                    decodedFields[i] = MemoryMarshal.Read<IntPtr>(payload);
                    payload = payload.Slice(IntPtr.Size);
                }
                else if (parameterType == typeof(int))
                {
                    decodedFields[i] = MemoryMarshal.Read<int>(payload);
                    payload = payload.Slice(sizeof(int));
                }
                else if (parameterType == typeof(uint))
                {
                    decodedFields[i] = MemoryMarshal.Read<uint>(payload);
                    payload = payload.Slice(sizeof(uint));
                }
                else if (parameterType == typeof(long))
                {
                    decodedFields[i] = MemoryMarshal.Read<long>(payload);
                    payload = payload.Slice(sizeof(long));
                }
                else if (parameterType == typeof(ulong))
                {
                    decodedFields[i] = MemoryMarshal.Read<ulong>(payload);
                    payload = payload.Slice(sizeof(ulong));
                }
                else if (parameterType == typeof(byte))
                {
                    decodedFields[i] = MemoryMarshal.Read<byte>(payload);
                    payload = payload.Slice(sizeof(byte));
                }
                else if (parameterType == typeof(sbyte))
                {
                    decodedFields[i] = MemoryMarshal.Read<sbyte>(payload);
                    payload = payload.Slice(sizeof(sbyte));
                }
                else if (parameterType == typeof(short))
                {
                    decodedFields[i] = MemoryMarshal.Read<short>(payload);
                    payload = payload.Slice(sizeof(short));
                }
                else if (parameterType == typeof(ushort))
                {
                    decodedFields[i] = MemoryMarshal.Read<ushort>(payload);
                    payload = payload.Slice(sizeof(ushort));
                }
                else if (parameterType == typeof(float))
                {
                    decodedFields[i] = MemoryMarshal.Read<float>(payload);
                    payload = payload.Slice(sizeof(float));
                }
                else if (parameterType == typeof(double))
                {
                    decodedFields[i] = MemoryMarshal.Read<double>(payload);
                    payload = payload.Slice(sizeof(double));
                }
                else if (parameterType == typeof(bool))
                {
                    // The manifest defines a bool as a 32bit type (WIN32 BOOL), not 1 bit as CLR Does.
                    decodedFields[i] = (MemoryMarshal.Read<int>(payload) == 1);
                    payload = payload.Slice(sizeof(int));
                }
                else if (parameterType == typeof(Guid))
                {
                    const int sizeOfGuid = 16;
                    decodedFields[i] = new Guid(payload.Slice(0, sizeOfGuid));
                    payload = payload.Slice(sizeOfGuid);
                }
                else if (parameterType == typeof(char))
                {
                    decodedFields[i] = MemoryMarshal.Read<char>(payload);
                    payload = payload.Slice(sizeof(char));
                }
                else if (parameterType == typeof(string))
                {
                    ReadOnlySpan<char> charPayload = MemoryMarshal.Cast<byte, char>(payload);
                    int charCount = charPayload.IndexOf('\0');
                    string val = new string(charCount >= 0 ? charPayload.Slice(0, charCount) : charPayload);
                    payload = payload.Slice((val.Length + 1) * sizeof(char));
                    decodedFields[i] = val;
                }
                else
                {
                    Debug.Assert(false, "Unsupported type encountered.");
                }
            }

            return decodedFields;
        }
    }
#endif // FEATURE_PERFTRACING
}

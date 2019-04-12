// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Reflection;
using Microsoft.Win32;

namespace System.Diagnostics.Tracing
{
    public partial class EventSource
    {
        private int GetParameterCount(EventMetadata eventData)
        {
            return eventData.Parameters.Length;
        }

        private Type GetDataType(EventMetadata eventData, int parameterId)
        {
            return eventData.Parameters[parameterId].ParameterType;
        }

        private static string GetResourceString(string key, params object[] args)
        {
            return SR.Format(SR.GetResourceString(key), args);
        }

        private static readonly bool m_EventSourcePreventRecursion = false;
    }

    internal static class Resources
    {
        internal static string GetResourceString(string key, params object[] args)
        {
            return SR.Format(SR.GetResourceString(key), args);
        }
    }
}

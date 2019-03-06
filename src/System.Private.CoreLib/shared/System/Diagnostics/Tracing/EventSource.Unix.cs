// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if ES_BUILD_STANDALONE
namespace Microsoft.Diagnostics.Tracing
#else
namespace System.Diagnostics.Tracing
#endif
{
    public partial class EventSource : IDisposable
    {
        internal static uint GetCurrentProcessId() => (uint)Interop.Sys.GetPid();
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.InteropServices
{
    /// <summary>
    /// Wrapper that is converted to a variant with VT_DISPATCH.
    /// </summary>
    public sealed class DispatchWrapper
    {
        public DispatchWrapper(object obj)
        {
            if (obj != null)
            {
                // Make sure this guy has an IDispatch
                IntPtr pdisp = Marshal.GetIDispatchForObject(obj);

                // If we got here without throwing an exception, the QI for IDispatch succeeded.
                Marshal.Release(pdisp);
            }
            m_WrappedObject = obj;
        }

        public object WrappedObject => m_WrappedObject;

        private object m_WrappedObject;
    }
}

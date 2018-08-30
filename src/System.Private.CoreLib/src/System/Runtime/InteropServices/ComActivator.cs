// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace System.Runtime.InteropServices
{
    [ComImport]
    [ComVisible(false)]
    [Guid("00000001-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IClassFactory
    {
        void CreateInstance(
            [MarshalAs(UnmanagedType.Interface)] object pUnkOuter,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppvObject);

        void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LICINFO
    {
        public int cbLicInfo;

        [MarshalAs(UnmanagedType.Bool)]
        public bool fRuntimeKeyAvail;

        [MarshalAs(UnmanagedType.Bool)]
        public bool fLicVerified;
    }

    [ComImport]
    [ComVisible(false)]
    [Guid("B196B28F-BAB4-101A-B69C-00AA00341D07")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IClassFactory2 : IClassFactory
    {
        new void CreateInstance(
            [MarshalAs(UnmanagedType.Interface)] object pUnkOuter,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppvObject);

        new void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);

        void GetLicInfo(ref LICINFO pLicInfo);

        void RequestLicKey(
            int dwReserved,
            [MarshalAs(UnmanagedType.BStr)] out string pBstrKey);

        void CreateInstanceLic(
            [MarshalAs(UnmanagedType.Interface)] object pUnkOuter,
            [MarshalAs(UnmanagedType.Interface)] object pUnkReserved,
            ref Guid riid,
            [MarshalAs(UnmanagedType.BStr)] string bstrKey,
            [MarshalAs(UnmanagedType.Interface)] out object ppvObject);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ComActivationContext
    {
        public Guid ClassId;
        public Guid InterfaceId;
        public string[] ActivationAssemblyList;
    }

    public static class ComActivator
    {
        /// <summary>
        /// Entry point for unmanaged COM activation API from managed code
        /// </summary>
        /// <param name="cxt">Reference to a <see cref="ComActivationContext"/> instance</param>
        public static object GetClassFactoryForType(ComActivationContext cxt)
        {
            if (cxt.InterfaceId != typeof(IClassFactory).GUID
                && cxt.InterfaceId != typeof(IClassFactory2).GUID)
            {
                throw new NotSupportedException();
            }

            string[] potentialAssemblies = cxt.ActivationAssemblyList ?? new string[0];
            (Assembly classAssembly, Type classType) = FindClassAssemblyAndType(cxt.ClassId, potentialAssemblies);
            return new BasicClassFactory(cxt.ClassId, classAssembly, classType);
        }

        private static bool IsLoggingEnabled()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        private static void Log(string fmt, params object[] args)
        {
            // [TODO] Consider using FrameworkEventSource in release builds

            Debug.WriteLine(fmt, args);
         }

        private static (Assembly assembly, Type type) FindClassAssemblyAndType(Guid clsid, string[] potentialAssembies)
        {
            // Determine what assembly the class is in
            foreach (string assemPath in potentialAssembies)
            {
                Assembly assem;
                string assemPathLocal = assemPath;

                try
                {
                    string extMaybe = Path.GetExtension(assemPath);
                    if (".manifest".Equals(extMaybe, StringComparison.OrdinalIgnoreCase))
                    {
                        assemPathLocal = Path.ChangeExtension(assemPath, ".dll");
                    }

                    assem = Assembly.LoadFrom(assemPathLocal);
                }
                catch (Exception e)
                {
                    if (IsLoggingEnabled())
                    {
                        Log($"COM Activation of {clsid} failed to load assembly {assemPathLocal}: {e}");
                    }

                    continue;
                }

                // Check the loaded assembly for a class with the desired ID
                foreach (Type t in assem.GetTypes())
                {
                    if (t.GUID == clsid)
                    {
                        return (assem, t);
                    }
                }
            }

            // [TODO] Check Registry for registration

            const int CLASS_E_CLASSNOTAVAILABLE = unchecked((int)0x80040111);
            throw new COMException(string.Empty, CLASS_E_CLASSNOTAVAILABLE);
        }

        [ComVisible(true)]
        internal class BasicClassFactory : IClassFactory2
        {
            private readonly Guid classId;
            private readonly Type classType;
            private readonly Assembly classAssembly;

            public BasicClassFactory(Guid clsid, Assembly assembly, Type classType)
            {
                this.classId = clsid;
                this.classType = classType;
                this.classAssembly = assembly;
            }

            public void CreateInstance(
                [MarshalAs(UnmanagedType.Interface)] object pUnkOuter,
                ref Guid riid,
                [MarshalAs(UnmanagedType.Interface)] out object ppvObject)
            {
                if (riid != Marshal.IID_IUnknown)
                {
                    bool found = false;

                    // Verify the class implements the desired interface
                    foreach (Type i in this.classType.GetInterfaces())
                    {
                        if (i.GUID == riid)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        // E_NOINTERFACE
                        throw new InvalidCastException();
                    }
                }

                ppvObject = Activator.CreateInstance(this.classType);
                if (pUnkOuter != null)
                {
                    try
                    {
                        IntPtr outerPtr = Marshal.GetIUnknownForObject(pUnkOuter);
                        IntPtr innerPtr = Marshal.CreateAggregatedObject(outerPtr, ppvObject);
                        ppvObject = Marshal.GetObjectForIUnknown(innerPtr);
                    }
                    finally
                    {
                        // Decrement the above 'Marshal.GetIUnknownForObject()'
                        Marshal.ReleaseComObject(pUnkOuter);
                    }
                }
            }

            public void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock)
            {
                // nop
            }

            public void GetLicInfo(ref LICINFO pLicInfo)
            {
                throw new NotImplementedException();
            }

            public void RequestLicKey(int dwReserved, [MarshalAs(UnmanagedType.BStr)] out string pBstrKey)
            {
                throw new NotImplementedException();
            }

            public void CreateInstanceLic(
                [MarshalAs(UnmanagedType.Interface)] object pUnkOuter,
                [MarshalAs(UnmanagedType.Interface)] object pUnkReserved,
                ref Guid riid,
                [MarshalAs(UnmanagedType.BStr)] string bstrKey,
                [MarshalAs(UnmanagedType.Interface)] out object ppvObject)
            {
                throw new NotImplementedException();
            }
        }
    }
}

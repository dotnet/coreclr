// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

//
// Types in this file marked as 'public' are done so only to aid in
// testing of functionality and should not be considered publicly consumable.
//
namespace Internal.Runtime.InteropServices
{
    [ComImport]
    [ComVisible(false)]
    [Guid("00000001-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IClassFactory
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
        public string AssemblyPath;
        public string AssemblyName;
        public string TypeName;
    }

    [StructLayout(LayoutKind.Sequential)]
    [CLSCompliant(false)]
    public unsafe struct ComActivationContextInternal
    {
        public Guid ClassId;
        public Guid InterfaceId;
        public char* AssemblyPathBuffer;
        public char* AssemblyNameBuffer;
        public char* TypeNameBuffer;
        public IntPtr ClassFactoryDest;
    }

    public static class ComActivator
    {
        // Collection of all ALCs used for COM activation. In the event we want to support
        // unloadable COM server ALCs, this will need to be changed.
        private static Dictionary<string, AssemblyLoadContext> s_AssemblyLoadContexts = new Dictionary<string, AssemblyLoadContext>(StringComparer.InvariantCultureIgnoreCase);

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

            if (!Path.IsPathRooted(cxt.AssemblyPath))
            {
                throw new ArgumentException();
            }

            Type classType = FindClassType(cxt.ClassId, cxt.AssemblyPath, cxt.AssemblyName, cxt.TypeName);

            if (LicenseInteropProxy.HasLicense(classType))
            {
                return new LicenseClassFactory(cxt.ClassId, classType);
            }

            return new BasicClassFactory(cxt.ClassId, classType);
        }

        /// <summary>
        /// Internal entry point for unmanaged COM activation API from native code
        /// </summary>
        /// <param name="cxtInt">Reference to a <see cref="ComActivationContextInternal"/> instance</param>
        [CLSCompliant(false)]
        public unsafe static int GetClassFactoryForTypeInternal(ref ComActivationContextInternal cxtInt)
        {
            if (IsLoggingEnabled())
            {
                Log(
$@"{nameof(GetClassFactoryForTypeInternal)} arguments:
    {cxtInt.ClassId}
    {cxtInt.InterfaceId}
    0x{(ulong)cxtInt.AssemblyPathBuffer:x}
    0x{(ulong)cxtInt.AssemblyNameBuffer:x}
    0x{(ulong)cxtInt.TypeNameBuffer:x}
    0x{cxtInt.ClassFactoryDest.ToInt64():x}");
            }

            try
            {
                var cxt = new ComActivationContext()
                {
                    ClassId = cxtInt.ClassId,
                    InterfaceId = cxtInt.InterfaceId,
                    AssemblyPath = Marshal.PtrToStringUni(new IntPtr(cxtInt.AssemblyPathBuffer)),
                    AssemblyName = Marshal.PtrToStringUni(new IntPtr(cxtInt.AssemblyNameBuffer)),
                    TypeName = Marshal.PtrToStringUni(new IntPtr(cxtInt.TypeNameBuffer))
                };

                object cf = GetClassFactoryForType(cxt);
                IntPtr nativeIUnknown = Marshal.GetIUnknownForObject(cf);
                Marshal.WriteIntPtr(cxtInt.ClassFactoryDest, nativeIUnknown);
            }
            catch (Exception e)
            {
                return e.HResult;
            }

            return 0;
        }

        private static bool IsLoggingEnabled()
        {
#if COM_ACTIVATOR_DEBUG
            return true;
#else
            return false;
#endif
        }

        private static void Log(string fmt, params object[] args)
        {
            // [TODO] Use FrameworkEventSource in release builds

            Debug.WriteLine(fmt, args);
         }

        private static Type FindClassType(Guid clsid, string assemblyPath, string assemblyName, string typeName)
        {
            try
            {
                AssemblyLoadContext alc = GetALC(assemblyPath);
                var assemblyNameLocal = new AssemblyName(assemblyName);
                Assembly assem = alc.LoadFromAssemblyName(assemblyNameLocal);
                Type t = assem.GetType(typeName);
                if (t != null)
                {
                    return t;
                }
            }
            catch (Exception e)
            {
                if (IsLoggingEnabled())
                {
                    Log($"COM Activation of {clsid} failed. {e}");
                }
            }

            const int CLASS_E_CLASSNOTAVAILABLE = unchecked((int)0x80040111);
            throw new COMException(string.Empty, CLASS_E_CLASSNOTAVAILABLE);
        }

        private static AssemblyLoadContext GetALC(string assemblyPath)
        {
            AssemblyLoadContext alc;

            lock (s_AssemblyLoadContexts)
            {
                if (!s_AssemblyLoadContexts.TryGetValue(assemblyPath, out alc))
                {
                    alc = new ComServerLoadContext(assemblyPath);
                    s_AssemblyLoadContexts.Add(assemblyPath, alc);
                }
            }

            return alc;
        }

        private class ComServerLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;

            public ComServerLoadContext(string comServerAssemblyPath)
            {
                _resolver = new AssemblyDependencyResolver(comServerAssemblyPath);
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                if (assemblyPath != null)
                {
                    return LoadFromAssemblyPath(assemblyPath);
                }

                return null;
            }

            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
                if (libraryPath != null)
                {
                    return LoadUnmanagedDllFromPath(libraryPath);
                }

                return IntPtr.Zero;
            }
        }

        [ComVisible(true)]
        private class BasicClassFactory : IClassFactory
        {
            private readonly Guid classId;
            private readonly Type classType;

            public BasicClassFactory(Guid clsid, Type classType)
            {
                this.classId = clsid;
                this.classType = classType;
            }

            public static void ValidateInterfaceRequest(Type classType, ref Guid riid, object outer)
            {
                Debug.Assert(classType != null);
                if (riid == Marshal.IID_IUnknown)
                {
                    return;
                }

                // Aggregation can only be done when requesting IUnknown.
                if (outer != null)
                {
                    const int CLASS_E_NOAGGREGATION = unchecked((int)0x80040110);
                    throw new COMException(string.Empty, CLASS_E_NOAGGREGATION);
                }

                bool found = false;

                // Verify the class implements the desired interface
                foreach (Type i in classType.GetInterfaces())
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

            public static object CreateAggregatedObject(object pUnkOuter, object comObject)
            {
                Debug.Assert(pUnkOuter != null && comObject != null);
                try
                {
                    IntPtr outerPtr = Marshal.GetIUnknownForObject(pUnkOuter);
                    IntPtr innerPtr = Marshal.CreateAggregatedObject(outerPtr, comObject);
                    return Marshal.GetObjectForIUnknown(innerPtr);
                }
                finally
                {
                    // Decrement the above 'Marshal.GetIUnknownForObject()'
                    Marshal.ReleaseComObject(pUnkOuter);
                }
            }

            public void CreateInstance(
                [MarshalAs(UnmanagedType.Interface)] object pUnkOuter,
                ref Guid riid,
                [MarshalAs(UnmanagedType.Interface)] out object ppvObject)
            {
                BasicClassFactory.ValidateInterfaceRequest(this.classType, ref riid, pUnkOuter);

                ppvObject = Activator.CreateInstance(this.classType);
                if (pUnkOuter != null)
                {
                    ppvObject = BasicClassFactory.CreateAggregatedObject(pUnkOuter, ppvObject);
                }
            }

            public void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock)
            {
                // nop
            }
        }

        [ComVisible(true)]
        private class LicenseClassFactory : IClassFactory2
        {
            private readonly LicenseInteropProxy licenseProxy = new LicenseInteropProxy();
            private readonly Guid classId;
            private readonly Type classType;

            public LicenseClassFactory(Guid clsid, Type classType)
            {
                this.classId = clsid;
                this.classType = classType;
            }

            public void CreateInstance(
                [MarshalAs(UnmanagedType.Interface)] object pUnkOuter,
                ref Guid riid,
                [MarshalAs(UnmanagedType.Interface)] out object ppvObject)
            {
                this.CreateInstanceInner(pUnkOuter, ref riid, key: null, isDesignTime: true, out ppvObject);
            }

            public void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock)
            {
                // nop
            }

            public void GetLicInfo(ref LICINFO licInfo)
            {
                bool runtimeKeyAvail;
                bool licVerified;
                this.licenseProxy.GetLicInfo(this.classType, out runtimeKeyAvail, out licVerified);

                // The LICINFO is a struct with a DWORD size field and two BOOL fields. Each BOOL
                // is typedef'd from a DWORD, therefore the size is manually computed as below.
                licInfo.cbLicInfo = sizeof(int) + sizeof(int) + sizeof(int);
                licInfo.fRuntimeKeyAvail = runtimeKeyAvail;
                licInfo.fLicVerified = licVerified;
            }

            public void RequestLicKey(int dwReserved, [MarshalAs(UnmanagedType.BStr)] out string pBstrKey)
            {
                pBstrKey = this.licenseProxy.RequestLicKey(this.classType);
            }

            public void CreateInstanceLic(
                [MarshalAs(UnmanagedType.Interface)] object pUnkOuter,
                [MarshalAs(UnmanagedType.Interface)] object pUnkReserved,
                ref Guid riid,
                [MarshalAs(UnmanagedType.BStr)] string bstrKey,
                [MarshalAs(UnmanagedType.Interface)] out object ppvObject)
            {
                Debug.Assert(pUnkReserved == null);
                this.CreateInstanceInner(pUnkOuter, ref riid, bstrKey, isDesignTime: false, out ppvObject);
            }

            private void CreateInstanceInner(
                object pUnkOuter,
                ref Guid riid,
                string key,
                bool isDesignTime,
                out object ppvObject)
            {
                BasicClassFactory.ValidateInterfaceRequest(this.classType, ref riid, pUnkOuter);

                ppvObject = this.licenseProxy.AllocateAndValidateLicense(this.classType, key, isDesignTime);
                if (pUnkOuter != null)
                {
                    ppvObject = BasicClassFactory.CreateAggregatedObject(pUnkOuter, ppvObject);
                }
            }
        }

        private class LicenseInteropProxy
        {
            private static readonly Type s_LicenseAttrType;
            private readonly Type helperType;
            private readonly MethodInfo getLicInfo;
            private readonly MethodInfo requestLicKey;
            private readonly MethodInfo allocateAndValidateLicense;
            private readonly object instance;

            static LicenseInteropProxy()
            {
                s_LicenseAttrType = Type.GetType("System.ComponentModel.LicenseProviderAttribute, System", throwOnError: true);
            }

            public LicenseInteropProxy()
            {
                Type licman = Type.GetType("System.ComponentModel.LicenseManager, System", throwOnError: true);
                this.helperType = licman.GetNestedType("LicenseInteropHelper", BindingFlags.NonPublic);
                this.getLicInfo = this.helperType.GetMethod("GetLicInfo2", BindingFlags.Instance | BindingFlags.NonPublic);
                this.requestLicKey = this.helperType.GetMethod("RequestLicKey2", BindingFlags.Static | BindingFlags.NonPublic);
                this.allocateAndValidateLicense = this.helperType.GetMethod("AllocateAndValidateLicense2", BindingFlags.Static | BindingFlags.NonPublic);

                this.instance = Activator.CreateInstance(this.helperType);
            }

            public static bool HasLicense(Type type)
            {
                return type.IsDefined(s_LicenseAttrType, inherit: true);
            }

            public void GetLicInfo(Type type, out bool runtimeKeyAvail, out bool licVerified)
            {
                var parameters = new object[] { type, null, null };
                try
                {
                    this.getLicInfo.Invoke(this.instance, parameters);
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }

                runtimeKeyAvail = (bool)parameters[1];
                licVerified = (bool)parameters[2];
            }

            public string RequestLicKey(Type type)
            {
                var parameters = new object[] { type };
                try
                {
                    return (string)this.requestLicKey.Invoke(null, parameters);
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }

            public object AllocateAndValidateLicense(Type type, string key, bool isDesignTime)
            {
                var parameters = new object[] { type, key, isDesignTime };
                try
                {
                    return this.allocateAndValidateLicense.Invoke(null, parameters);
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
        }
    }
}

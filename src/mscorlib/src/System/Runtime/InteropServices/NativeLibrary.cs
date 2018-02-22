// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
    /// <summary>
    /// A helper class which allows interacting with dynamically loaded modules.
    /// </summary>
    /// <remarks>
    /// This is similar in principle to <see cref="DllImportAttribute"/>, but it can be used where
    /// the module name or export names are not known at compile time.
    /// </remarks>
    public sealed partial class NativeLibrary
    {
        // The CLR's internal HINSTANCE, which may not match the OS-provided handle.
        private readonly IntPtr _hInstance;

        private NativeLibrary(IntPtr hModule)
        {
            if (!IsValidModuleHandle(hModule))
            {
                throw new ArgumentException(
                    paramName: nameof(hModule),
                    message: SR.Argument_InvalidHandle);
            }

            _hInstance = hModule;
        }

        /// <summary>
        /// The raw OS-provided handle for this module.
        /// </summary>
        /// <remarks>
        /// The caller can query this handle for information about the loaded module
        /// but should not attempt to free or otherwise manipulate this handle.
        /// </remarks>
        public IntPtr Handle => OperatingSystemHandle;

        // This is a managed equivalent to NDirectMethodDesc::FindEntryPoint.
        private TDelegate CreateDelegateCore<TDelegate>(string name, bool exactSpelling, RuntimeMethodInfo delegateInvokeMethod) where TDelegate : class
        {
            Debug.Assert(typeof(TDelegate) is RuntimeType);
            Debug.Assert(((RuntimeType)typeof(TDelegate)).IsDelegate());
            Debug.Assert(name != null);
            Debug.Assert(delegateInvokeMethod != null);

            // If we allow lookup by ordinal value and an ordinal is specified ("#1234", where
            // the value immediately following the '#' can fit into a WORD), then process the
            // ordinal and skip all other logic in this routine.

            if (AllowLocatingFunctionsByOrdinal && name.Length >= 1 && name[0] == '#')
            {
                if (UInt16.TryParse(name.AsReadOnlySpan(1), style: NumberStyles.None, provider: CultureInfo.InvariantCulture, out ushort ordinal))
                {
                    return CreateDelegateForSymbolByOrdinal<TDelegate>(ordinal);
                }
                else
                {
                    // TODO: Cleaner exception message
                    throw new ArgumentException(
                        paramName: nameof(name),
                        message: "Couldn't parse ordinal value from input string.");
                }
            }

            GetCharsetAndCallingConvention(delegateInvokeMethod, out bool isAnsi, out bool isStdcall);

            // Always look for the unmangled name first.
            // If there's a match and we're in ANSI mode, return it without looking for mangled names.
            // If mangling is disabled, don't proceed, even if we couldn't find the matching symbol.

            TDelegate retVal = CreateDelegateForSymbolByName<TDelegate>(name);
            if (exactSpelling || (retVal != null && isAnsi))
            {
                return retVal;
            }

            // Try appending an 'A' or 'W' suffix to get the ANSI / Unicode version of the API.

            if (isAnsi)
            {
                Debug.Assert(retVal == null);
                retVal = CreateDelegateForSymbolByName<TDelegate>(name + "A");
            }
            else
            {
                // On Unicode only (not ANSI), a method with a 'W' suffix takes precedence
                // over a method with an exact name match when name mangling is enabled.
                retVal = CreateDelegateForSymbolByName<TDelegate>(name + "W") ?? retVal;
            }

#if X86
            // On x86 only, look for __stdcall mangled names as a last resort.
            // They'll be of the form "_Name@x", where x is the stack size in bytes of the method arguments.

            if (retVal == null && isStdcall)
            {
                Debug.Assert(!exactSpelling);
                string stdcallMangledName = FormattableString.Invariant($"_{name}@{(uint)Marshal.NumParamBytes(delegateInvokeMethod, isForStdCallDelegate: true):D}");
                retVal = CreateDelegateForSymbolByName<TDelegate>(stdcallMangledName);
            }
#endif

            // This could still be null if we had no match, but there's nothing left for us to try.
            return retVal;
        }

        // Looks up a symbol by name (caller is responsible for mangling), and if it's found returns a delegate for it.
        // Returns null if the symbol is not found.
        private TDelegate CreateDelegateForSymbolByName<TDelegate>(string symbolName) where TDelegate : class
        {
            Debug.Assert(((RuntimeType)typeof(TDelegate)).IsDelegate());

            IntPtr symbolAddress = GetProcAddress(_hInstance, symbolName);
            return (symbolAddress != IntPtr.Zero)
                ? Marshal.GetDelegateForFunctionPointer<TDelegate>(symbolAddress)
                : null;
        }

        // Looks up a symbol by ordinal and if it's found returns a delegate for it.
        // Returns null if the symbol is not found.
        private TDelegate CreateDelegateForSymbolByOrdinal<TDelegate>(ushort ordinal) where TDelegate : class
        {
            Debug.Assert(((RuntimeType)typeof(TDelegate)).IsDelegate());

            IntPtr symbolAddress = GetProcAddress(_hInstance, (IntPtr)ordinal);
            return (symbolAddress != IntPtr.Zero)
                ? Marshal.GetDelegateForFunctionPointer<TDelegate>(symbolAddress)
                : null;
        }

        /// <summary>
        /// Equivalent to <see cref="TryGetDelegate{TDelegate}(string, bool, out TDelegate)"/> where <em>exactSpelling</em> is <see langword="false"/>.
        /// </summary>
        public bool TryGetDelegate<TDelegate>(string name, out TDelegate result) where TDelegate : class
            => TryGetDelegate<TDelegate>(name, exactSpelling: false, out result);

        /// <summary>
        /// Attempts to locate a named function in the module's export table and return a managed delegate to that function.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type that will be created. This type must subclass <see cref="MulticastDelegate"/>.
        /// This type <em>should</em> be (but is not required to be) annotated with <see cref="UnmanagedFunctionPointerAttribute"/>.
        /// Generic delegate types (e.g., <see cref="Func{T, TResult}"/>) are not supported.</typeparam>
        /// <param name="name">The name of the function to look up.</param>
        /// <param name="exactSpelling"><see langword="false"/> (default) if the runtime should automatically perform name decoration when
        /// locating the exported function; <see langword="true"/> if the runtime should use the provided name as-is with no additional
        /// decoration. See also <see cref="DllImportAttribute.ExactSpelling"/>.</param>
        /// <param name="result">A <typeparamref name="TDelegate"/> that represents the exported function,
        /// or <see langword="null"/> if the function cannot be located in the export table.</param>
        /// <returns><see langword="true"/> if the function was found, otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// Functions can also be queried by ordinal export by using a "#" prefix (e.g., "#1") for <paramref name="name"/>.
        /// </remarks>
        public bool TryGetDelegate<TDelegate>(string name, bool exactSpelling, out TDelegate result) where TDelegate : class
        {
            // Parameter validation
            // Some checks overlap with Marshal.GetDelegateForFunctionPointer since we need to
            // call into the CLR separately so that we can support mangling and other features.

            var delegateType = typeof(TDelegate) as RuntimeType;

            // Must be a normal RuntimeType and the immediate parent class must be MulticastDelegate
            if (delegateType == null || !delegateType.IsDelegate())
            {
                throw new ArgumentException(
                    paramName: nameof(TDelegate),
                    message: SR.Arg_MustBeDelegate);
            }

            RuntimeMethodHandleInternal invokeMethodHandle = new RuntimeMethodHandleInternal(System.StubHelpers.StubHelpers.GetDelegateInvokeMethodFromDelegateType(delegateType));
            RuntimeMethodInfo invokeMethodInfo = (RuntimeMethodInfo)RuntimeType.GetMethodBase(delegateType, invokeMethodHandle);

            // Generic types are disallowed
            if (delegateType.IsGenericType || invokeMethodInfo.IsGenericMethod)
            {
                throw new ArgumentException(
                    paramName: nameof(TDelegate),
                    message: SR.Argument_NeedNonGenericType);
            }

            if (invokeMethodInfo.CallingConvention == CallingConventions.VarArgs)
            {
                throw new ArgumentException(
                    paramName: nameof(TDelegate),
                    message: SR.NotSupported_CallToVarArg);
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            // End parameter validation

            result = CreateDelegateCore<TDelegate>(name, exactSpelling, invokeMethodInfo);
            return (result != null);
        }

        /// <summary>
        /// Attempts to locate the address of a symbol in the module's export table.
        /// </summary>
        /// <param name="symbolName">The name of the symbol to locate.</param>
        /// <param name="result">The address of the symbol, or <see cref="IntPtr.Zero"/> if the symbol cannot be found.</param>
        /// <returns><see langword="true"/> if the symbol was found, otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// Unlike <see cref="TryGetDelegate{TDelegate}(string, bool, out TDelegate)"/>, this method does not perform name decoration.
        /// The name provided in <paramref name="symbolName"/> must exactly match the name present in the export table.
        /// </remarks>
        public bool TryGetSymbolAddress(string symbolName, out IntPtr result)
        {
            // Parameter validation

            if (symbolName == null)
            {
                throw new ArgumentNullException(nameof(symbolName));
            }

            // End parameter validation

            // We expect that the caller has provided the exact symbol name; we don't perform extra mangling.

            result = GetProcAddress(_hInstance, symbolName);
            return (result != IntPtr.Zero);
        }

        /// <summary>
        /// Attempts to dynamically load the named module.
        /// </summary>
        /// <param name="name">The name of the module to load, similar to <see cref="DllImportAttribute.Value"/>.</param>
        /// <param name="caller">The assembly which is requesting the module load. Generally this should be the managed assembly
        /// which has a direct dependency on the native library.</param>
        /// <param name="paths">A flags value which contains the locations that should be searched during module load.</param>
        /// <param name="result">A <see cref="NativeLibrary"/> instance that represents the loaded module,
        /// or <see langword="null"/> if the module could not be found. Once loaded, a module cannot be unloaded.</param>
        /// <returns><see langword="true"/> if the module was found and loaded, otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// There are some behavioral differences between this method and <see cref="DllImportAttribute"/>. For example, the
        /// value provided in <paramref name="name"/> cannot be the display name of a managed assembly. Additionally, if
        /// <paramref name="caller"/> is not specified, then <paramref name="paths"/> cannot have the flags values
        /// <see cref="DllImportSearchPath.LegacyBehavior"/> or <see cref="DllImportSearchPath.AssemblyDirectory"/>.
        /// If <paramref name="caller"/> is specified and <paramref name="paths"/> is <see cref="DllImportSearchPath.LegacyBehavior"/>
        /// and an assembly-level <see cref="DefaultDllImportSearchPathsAttribute"/> is present, the assembly-level default
        /// <see cref="DllImportSearchPath"/> value will be used instead.
        /// This method may throw an exception in certain cases, such as if the module was located but is invalid
        /// for the current architecture.
        /// </remarks>
        public static bool TryLoad(string name, Assembly caller, DllImportSearchPath paths, out NativeLibrary result)
        {
            // Parameter validation

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            RuntimeAssembly callerAsRuntimeAssembly = caller as RuntimeAssembly;
            if (caller != null && callerAsRuntimeAssembly == null)
            {
                throw new ArgumentException(
                    paramName: nameof(caller),
                    message: SR.Argument_MustBeRuntimeAssembly);
            }
            
            // Assembly-level default search paths win out over legacy behavior.
            // This is a slight change in behavior from [DllImport], where search paths specified at
            // the method level override search paths specified at the assembly level. But since our
            // paths parameter isn't nullable, we need to resolve the ambiguity *somehow*, and this
            // seems like the way that'll be least surprising to consumers.

            if (paths == DllImportSearchPath.LegacyBehavior)
            {
                Debug.Assert(caller != null); // should've already been checked
                var attr = caller.GetCustomAttribute<DefaultDllImportSearchPathsAttribute>();
                if (attr != null)
                {
                    paths = attr.Paths;
                }
            }

            // The set of allowed flags is going to be different per OS. The caller is responsible
            // for querying RuntimeInformation.IsOSPlatform(...) so that they don't inadvertently
            // provide flags that are meaningless for the current OS.

            if (((uint)paths & ~AllowedDllImportSearchPathsMask) != 0)
            {
                // TODO: Turn error message into a resource string.
                throw new ArgumentException(
                    paramName: nameof(paths),
                    message: "Invalid flags were provided.");
            }

            // "Should search assembly directory?" gets special treatment by the CLR,
            // so we strip it off into its own parameter.

            bool searchAssemblyDirectory = paths.HasFlag(DllImportSearchPath.AssemblyDirectory);
            paths &= ~DllImportSearchPath.AssemblyDirectory;

            if (caller == null && searchAssemblyDirectory)
            {
                // TODO: Turn error message into a resource string.
                throw new ArgumentException(
                    paramName: nameof(paths),
                    message: "Cannot specify AssemblyDirectory if no calling assembly given.");
            }

            // End parameter validation

            IntPtr hModule = LoadLibrary(name, callerAsRuntimeAssembly?.GetNativeHandle(), searchAssemblyDirectory, paths);
            result = (hModule != IntPtr.Zero) ? new NativeLibrary(hModule) : null;
            return (result != null);
        }

        // For symbol lookups by name
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        private static extern void GetCharsetAndCallingConvention(
            [In] IRuntimeMethodInfo pMdDelegate,
            [Out] out bool pfIsAnsi,
            [Out] out bool pfIsStdcall);

        // For symbol lookups by name
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true)]
        private static extern IntPtr GetProcAddress(
            [In] IntPtr hModule,
            [In, MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        // For symbol lookups by ordinal value
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        private static extern IntPtr GetProcAddress(
            [In] IntPtr hModule,
            [In] IntPtr lpProcName);

        // Loads a library by name
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true)]
        private static extern IntPtr LoadLibrary(
                [In, MarshalAs(UnmanagedType.LPUTF8Str)] string moduleName,
                [In] RuntimeAssembly callingAssembly,
                [In] bool searchAssemblyDirectory,
                [In] DllImportSearchPath searchPaths);
    }
}

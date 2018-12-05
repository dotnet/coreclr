﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Internal.IO;

namespace System.Runtime.Loader
{
    public sealed class ComponentDependencyResolver
    {
        /// <summary>
        /// The name of the neutral culture (same value as in Variables::Init in CoreCLR)
        /// </summary>
        private const string NeutralCultureName = "neutral";

        /// <summary>
        /// The extension of resource assembly (same as in BindSatelliteResourceByResourceRoots in CoreCLR)
        /// </summary>
        private const string ResourceAssemblyExtension = ".dll";

        private readonly string _componentAssemblyPath;
        private readonly Dictionary<string, string> _assemblyPaths;
        private readonly string[] _nativeSearchPaths;
        private readonly string[] _resourceSearchPaths;
        private readonly string[] _componentDirectorySearchPaths;

        public ComponentDependencyResolver(string componentAssemblyPath)
        {
            _componentAssemblyPath = componentAssemblyPath;

            string assemblyPathsList = null;
            string nativeSearchPathsList = null;
            string resourceSearchPathsList = null;
            int returnCode = 0;

            try
            {
                returnCode = corehost_resolve_component_dependencies(
                    componentAssemblyPath,
                    (assembly_paths, native_search_paths, resource_search_paths) =>
                    {
                        assemblyPathsList = assembly_paths;
                        nativeSearchPathsList = native_search_paths;
                        resourceSearchPathsList = resource_search_paths;
                    });
            }
            catch (EntryPointNotFoundException entryPointNotFoundException)
            {
                throw new InvalidOperationException(SR.ComponentDependencyResolver_FailedToLoadHostpolicy, entryPointNotFoundException);
            }
            catch (DllNotFoundException dllNotFoundException)
            {
                throw new InvalidOperationException(SR.ComponentDependencyResolver_FailedToLoadHostpolicy, dllNotFoundException);
            }

            if (returnCode != 0)
            {
                // Something went wrong - report a failure
                // TODO - once we have the ability to capture detailed error messages from the hostpolicy
                // use it here to make the exception more actionable.
                throw new InvalidOperationException(SR.Format(SR.ComponentDependencyResolver_FailedToResolveDependencies, componentAssemblyPath, returnCode));
            }

            string[] assemblyPaths = SplitPathsList(assemblyPathsList);

            // Assembly simple names are case insensitive per the runtime behavior
            // (see SimpleNameToFileNameMapTraits for the TPA lookup hash).
            _assemblyPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string assemblyPath in assemblyPaths)
            {
                _assemblyPaths.Add(Path.GetFileNameWithoutExtension(assemblyPath), assemblyPath);
            }

            _nativeSearchPaths = SplitPathsList(nativeSearchPathsList);
            _resourceSearchPaths = SplitPathsList(resourceSearchPathsList);

            _componentDirectorySearchPaths = new string[1] { Path.GetDirectoryName(componentAssemblyPath) };
        }

        public string ResolveAssemblyPath(AssemblyName assemblyName)
        {
            // Determine if the assembly name is for a satellite assembly or not
            // This is the same logic as in AssemblyBinder::BindByTpaList in CoreCLR
            // - If the culture name is non-empty and it's not 'neutral' 
            // - The culture name is the value of the AssemblyName.Culture.Name 
            //     (CoreCLR gets this and stores it as the culture name in the internal assembly name)
            //     AssemblyName.CultureName is just a shortcut to AssemblyName.Culture.Name.
            if (!string.IsNullOrEmpty(assemblyName.CultureName) && 
                !string.Equals(assemblyName.CultureName, "neutral", StringComparison.OrdinalIgnoreCase))
            {
                // Load satellite assembly
                // Search resource search paths by appending the culture name and the expected assembly file name.
                // Copies the logic in BindSatelliteResourceByResourceRoots in CoreCLR.
                // Note that the runtime will also probe APP_PATHS the same way, but that feature is effectively 
                // being deprecated, so we chose to not support the same behavior for components.
                foreach (string searchPath in _resourceSearchPaths)
                {
                    string assemblyPath = Path.Combine(
                        searchPath,
                        assemblyName.CultureName,
                        assemblyName.Name + ResourceAssemblyExtension);
                    if (File.Exists(assemblyPath))
                    {
                        return assemblyPath;
                    }
                }
            }
            else
            {
                // Load code assembly - simply look it up in the dictionary by its simple name.
                if (_assemblyPaths.TryGetValue(assemblyName.Name, out string assemblyPath))
                {
                    // Only returnd the assembly if it exists on disk - this is to make the behavior of the API
                    // consistent. Resource and native resolutions will only return existing files
                    // so assembly resolution should do the same.
                    if (File.Exists(assemblyPath))
                    {
                        return assemblyPath;
                    }
                }
            }

            return null;
        }

        public string ResolveUnmanagedDllPath(string unmanagedDllName)
        {
            string[] searchPaths;
            if (unmanagedDllName.Contains(Path.DirectorySeparatorChar))
            {
                // Library names with absolute or relative path can't be resolved
                // using the component .deps.json as that defines simple names.
                // So instead use the component directory as the lookup path.
                searchPaths = _componentDirectorySearchPaths;
            }
            else
            {
                searchPaths = _nativeSearchPaths;
            }

            bool isRelativePath = !Path.IsPathFullyQualified(unmanagedDllName);
            foreach (LibraryNameVariation libraryNameVariation in DetermineLibraryNameVariations(unmanagedDllName, isRelativePath))
            {
                string libraryName = libraryNameVariation.Prefix + unmanagedDllName + libraryNameVariation.Suffix;
                foreach (string searchPath in searchPaths)
                {
                    string libraryPath = Path.Combine(searchPath, libraryName);
                    if (File.Exists(libraryPath))
                    {
                        return libraryPath;
                    }
                }
            }

            return null;
        }

        private static string[] SplitPathsList(string pathsList)
        {
            if (pathsList == null)
            {
                return Array.Empty<string>();
            }
            else
            {
                return pathsList.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        private struct LibraryNameVariation
        {
            public string Prefix;
            public string Suffix;

            public LibraryNameVariation(string prefix, string suffix)
            {
                Prefix = prefix;
                Suffix = suffix;
            }
        }

#if PLATFORM_WINDOWS
        private const CharSet HostpolicyCharSet = CharSet.Unicode;
        private const string LibraryNamePrefix = "";
        private const string LibraryNameSuffix = ".dll";

        private IEnumerable<LibraryNameVariation> DetermineLibraryNameVariations(string libName, bool isRelativePath)
        {
            // This is a copy of the logic in DetermineLibNameVariations in dllimport.cpp in CoreCLR

            yield return new LibraryNameVariation(string.Empty, string.Empty);

            if (isRelativePath &&
                !libName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
                !libName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                yield return new LibraryNameVariation(string.Empty, LibraryNameSuffix);
            }
        }
#else
        private const CharSet HostpolicyCharSet = CharSet.Ansi;

        private const string LibraryNamePrefix = "lib";
#if PLATFORM_OSX
        private const string LibraryNameSuffix = ".dylib";
#else
        private const string LibraryNameSuffix = ".so";
#endif

        private IEnumerable<LibraryNameVariation> DetermineLibraryNameVariations(string libName, bool isRelativePath)
        {
            // This is a copy of the logic in DetermineLibNameVariations in dllimport.cpp in CoreCLR

        if (!isRelativePath)
            {
                yield return new LibraryNameVariation(string.Empty, string.Empty);
            }
            else
            {
                bool containsSuffix = false;
                int indexOfSuffix = libName.IndexOf(LibraryNameSuffix);
                if (indexOfSuffix >= 0)
                {
                    indexOfSuffix += LibraryNameSuffix.Length;
                    containsSuffix = indexOfSuffix == libName.Length || libName[indexOfSuffix] == '.';
                }

                bool containsDelim = libName.Contains(Path.DirectorySeparatorChar);

                if (containsSuffix)
                {
                    yield return new LibraryNameVariation(string.Empty, string.Empty);
                    if (!containsDelim)
                    {
                        yield return new LibraryNameVariation(LibraryNamePrefix, string.Empty);
                    }
                    yield return new LibraryNameVariation(string.Empty, LibraryNameSuffix);
                    if (!containsDelim)
                    {
                        yield return new LibraryNameVariation(LibraryNamePrefix, LibraryNameSuffix);
                    }
                }
                else
                {
                    yield return new LibraryNameVariation(string.Empty, LibraryNameSuffix);
                    if (!containsDelim)
                    {
                        yield return new LibraryNameVariation(LibraryNamePrefix, LibraryNameSuffix);
                    }
                    yield return new LibraryNameVariation(string.Empty, string.Empty);
                    if (!containsDelim)
                    {
                        yield return new LibraryNameVariation(LibraryNamePrefix, string.Empty);
                    }
                }
            }


            yield return new LibraryNameVariation(string.Empty, string.Empty);

            if (isRelativePath &&
                !libName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
                !libName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                yield return new LibraryNameVariation(string.Empty, LibraryNameSuffix);
            }
        }
#endif

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = HostpolicyCharSet)]
        internal delegate void corehost_resolve_component_dependencies_result_fn(
            string assembly_paths,
            string native_search_paths,
            string resource_search_paths);

#pragma warning disable BCL0015 // Disable Pinvoke analyzer errors.
        [DllImport("hostpolicy", CharSet = HostpolicyCharSet)]
        private static extern int corehost_resolve_component_dependencies(
            string component_main_assembly_path,
            corehost_resolve_component_dependencies_result_fn result);
#pragma warning restore
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

using Internal.TypeSystem.Ecma;
using Internal.TypeSystem;
using ILCompiler.DependencyAnalysis.ReadyToRun;

namespace ILCompiler
{
    /// <summary>
    /// Provides compilation group for a library that compiles everything in the input IL module.
    /// </summary>
    public class ReadyToRunRootProvider : ICompilationRootProvider
    {
        private EcmaModule _module;
        private ProfileData _profileData;

        public ReadyToRunRootProvider(EcmaModule module, ProfileDataManager profileDataManager)
        {
            _module = module;
            _profileData = profileDataManager.GetDataForModuleDesc(module);
        }

        public void AddCompilationRoots(IRootingServiceProvider rootProvider)
        {
            foreach (var methodProfileInfo in _profileData.GetAllMethodProfileData())
            {
                if (!methodProfileInfo.Flags.HasFlag(MethodProfilingDataFlags.ExcludeHotMethodCode) &&
                    !methodProfileInfo.Flags.HasFlag(MethodProfilingDataFlags.ExcludeColdMethodCode))
                {
                    try
                    {
                        MethodDesc method = methodProfileInfo.Method;

                        // Validate that this method is fully instantiated
                        if (method.OwningType.IsGenericDefinition || method.OwningType.ContainsSignatureVariables())
                        {
                            continue;
                        }

                        if (method.IsGenericMethodDefinition)
                        {
                            continue;
                        }

                        bool containsSignatureVariables = false;
                        foreach (TypeDesc t in method.Instantiation)
                        {
                            if (t.IsGenericDefinition)
                            {
                                containsSignatureVariables = true;
                                break;
                            }

                            if (t.ContainsSignatureVariables())
                            {
                                containsSignatureVariables = true;
                                break;
                            }
                        }
                        if (containsSignatureVariables)
                            continue;

                        CheckCanGenerateMethod(method);
                        rootProvider.AddCompilationRoot(method, "Profile triggered method");
                    }
                    catch (TypeSystemException)
                    {
                        // Individual methods can fail to load types referenced in their signatures.
                        // Skip them in library mode since they're not going to be callable.
                        continue;
                    }
                }
            }

            if (!_profileData.PartialNGen)
            {
                foreach (MetadataType type in _module.GetAllTypes())
                {
                    try
                    {
                        rootProvider.AddCompilationRoot(type, "Library module type");
                    }
                    catch (TypeSystemException)
                    {
                        // Swallow type load exceptions while rooting
                        continue;
                    }

                    MetadataType typeWithMethods = type;
                    if (type.HasInstantiation)
                    {
                        typeWithMethods = InstantiateIfPossible(type);
                        if (typeWithMethods == null)
                            continue;
                    }

                    RootMethods(typeWithMethods, "Library module method", rootProvider);
                }
            }
        }

        private void RootMethods(TypeDesc type, string reason, IRootingServiceProvider rootProvider)
        {
            foreach (MethodDesc method in type.GetAllMethods())
            {
                // Skip methods with no IL
                if (method.IsAbstract)
                    continue;

                if (method.IsInternalCall)
                    continue;

                MethodDesc methodToRoot = method;
                if (method.HasInstantiation)
                {
                    methodToRoot = InstantiateIfPossible(method);

                    if (methodToRoot == null)
                        continue;
                }

                try
                {
                    CheckCanGenerateMethod(methodToRoot);
                    rootProvider.AddCompilationRoot(methodToRoot, reason);
                }
                catch (TypeSystemException)
                {
                    // Individual methods can fail to load types referenced in their signatures.
                    // Skip them in library mode since they're not going to be callable.
                    continue;
                }
                catch (InvalidOperationException)
                {
                    //
                    // This catch block is meant to catch the exception thrown by ArgIterator
                    // used in GCRefMapBuilder.GetCallRefMap, an example when that
                    // happen is when an indeterminate size type is used in the signature.
                    //
                    // TODO: InvalidOperationException sounds fairly general - do we risk
                    // catching cases where we should have crashed the compiler instead?
                    // (e.g. Some of us wrote buggy code but got misled by test result)
                    //
                    continue;
                }
            }
        }

        /// <summary>
        /// Validates that it will be possible to generate '<paramref name="method"/>' based on the types
        /// in its signature. Unresolvable types in a method's signature prevent RyuJIT from generating
        /// even a stubbed out throwing implementation.
        /// </summary>
        public void CheckCanGenerateMethod(MethodDesc method)
        {
            MethodSignature signature = method.Signature;

            // Vararg methods are not supported in .NET Core
            if ((signature.Flags & MethodSignatureFlags.UnmanagedCallingConventionMask) == MethodSignatureFlags.CallingConventionVarargs)
                ThrowHelper.ThrowBadImageFormatException();

            CheckTypeCanBeUsedInSignature(signature.ReturnType);

            for (int i = 0; i < signature.Length; i++)
            {
                CheckTypeCanBeUsedInSignature(signature[i]);
            }

            //
            // CheckTypeCanBeUsedInSignature is insufficient - the ArgIterator used in GetCallRefMap() can be used to detect
            // some cases (e.g. usage of types with indeterminate size) where compilation will eventually fail downstream.
            //
            // TODO: Is it possible to augment CheckTypeCanBeUsedInSignature() to accomplish the same? It is relative straightforward
            // to check if the type has indeterminate size, but is that sufficient?
            //
            new GCRefMapBuilder(_module.Context.Target, false).GetCallRefMap(method);
        }

        private static void CheckTypeCanBeUsedInSignature(TypeDesc type)
        {
            MetadataType defType = type as MetadataType;

            if (defType != null)
            {
                defType.ComputeTypeContainsGCPointers();
            }
        }

        private static Instantiation GetInstantiationThatMeetsConstraints(Instantiation definition)
        {
            TypeDesc[] args = new TypeDesc[definition.Length];

            for (int i = 0; i < definition.Length; i++)
            {
                GenericParameterDesc genericParameter = (GenericParameterDesc)definition[i];

                // If the parameter is not constrained to be a valuetype, we can instantiate over __Canon
                if (genericParameter.HasNotNullableValueTypeConstraint)
                {
                    return default;
                }

                args[i] = genericParameter.Context.CanonType;
            }

            return new Instantiation(args);
        }

        private static InstantiatedType InstantiateIfPossible(MetadataType type)
        {
            Instantiation inst = GetInstantiationThatMeetsConstraints(type.Instantiation);
            if (inst.IsNull)
            {
                return null;
            }

            return type.MakeInstantiatedType(inst);
        }

        private static MethodDesc InstantiateIfPossible(MethodDesc method)
        {
            Instantiation inst = GetInstantiationThatMeetsConstraints(method.Instantiation);
            if (inst.IsNull)
            {
                return null;
            }

            return method.MakeInstantiatedMethod(inst);
        }
    }
}

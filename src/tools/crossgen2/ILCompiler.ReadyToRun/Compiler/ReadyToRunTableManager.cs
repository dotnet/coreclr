// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

using ILCompiler.DependencyAnalysis;
using ILCompiler.DependencyAnalysisFramework;

using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;

using Debug = System.Diagnostics.Debug;

namespace ILCompiler
{
    public struct TypeInfo<THandle>
    {
        public readonly MetadataReader MetadataReader;
        public readonly THandle Handle;

        public TypeInfo(MetadataReader metadataReader, THandle handle)
        {
            MetadataReader = metadataReader;
            Handle = handle;
        }
    }

    /// <summary>
    /// Hooks into the de
    /// </summary>
    public class ReadyToRunTableManager
    {
        CompilationModuleGroup _compilationModuleGroup;
        private HashSet<MethodDesc> _methodsGenerated = new HashSet<MethodDesc>();

        public ReadyToRunTableManager(CompilationModuleGroup compilationModuleGroup)
        {
            _compilationModuleGroup = compilationModuleGroup;
        }

        public void AttachToDependencyGraph(DependencyAnalyzerBase<NodeFactory> graph)
        {
            graph.NewMarkedNode += GraphNewMarkedNode;
        }

        protected virtual void GraphNewMarkedNode(DependencyNodeCore<NodeFactory> obj)
        {
            IMethodBodyNode methodBodyNode = obj as IMethodBodyNode;
            var methodNode = methodBodyNode as IMethodNode;

            if (methodNode != null)
            {
                _methodsGenerated.Add(methodNode.Method);
            }
        }

        public IEnumerable<TypeInfo<TypeDefinitionHandle>> GetDefinedTypes()
        {
            foreach (EcmaModule module in _compilationModuleGroup.CompilationModules)
            {
                foreach (TypeDefinitionHandle typeDefHandle in module.MetadataReader.TypeDefinitions)
                {
                    yield return new TypeInfo<TypeDefinitionHandle>(module.MetadataReader, typeDefHandle);
                }
            }
        }

        public IEnumerable<TypeInfo<ExportedTypeHandle>> GetExportedTypes()
        {
            foreach (EcmaModule module in _compilationModuleGroup.CompilationModules)
            {
                foreach (ExportedTypeHandle exportedTypeHandle in module.MetadataReader.ExportedTypes)
                {
                    yield return new TypeInfo<ExportedTypeHandle>(module.MetadataReader, exportedTypeHandle);
                }
            }
        }

        public IEnumerable<MethodDesc> GetCompiledMethods()
        {
            return _methodsGenerated;
        }
    }
}

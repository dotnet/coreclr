// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Internal.Text;

namespace ILCompiler.DependencyAnalysis
{
    interface IRvaEmbeddedPointerIndirectionNode<out TTarget>
        where TTarget : ISortableSymbolNode
    {
        TTarget Target { get; }
        string CallSite { get;  }
    }

    class RvaEmbeddedPointerIndirectionNode<TTarget> : EmbeddedPointerIndirectionNode<TTarget>, IRvaEmbeddedPointerIndirectionNode<TTarget>
        where TTarget : ISortableSymbolNode
    {
        private readonly string _callSite;
        
        public RvaEmbeddedPointerIndirectionNode(TTarget target, string callSite = null)
            : base(target)
        {
            _callSite = callSite;
        }

        public string CallSite => _callSite;

        protected override string GetName(NodeFactory factory) => $"Embedded pointer to {Target.GetMangledName(factory.NameMangler)}";

        public override IEnumerable<DependencyListEntry> GetStaticDependencies(NodeFactory factory)
        {
            return new[]
            {
                new DependencyListEntry(Target, "reloc"),
            };
        }

        public override void EncodeData(ref ObjectDataBuilder dataBuilder, NodeFactory factory, bool relocsOnly)
        {
            dataBuilder.RequireInitialPointerAlignment();
            dataBuilder.EmitReloc(Target, RelocType.IMAGE_REL_BASED_ADDR32NB);
        }
    
        public override void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
            sb.Append("RVA_");
            Target.AppendMangledName(nameMangler, sb);
            if (_callSite != null)
            {
                sb.Append(" @ ");
                sb.Append(_callSite);
            }
        }

        public override int ClassCode => -66002498;

        public override int CompareToImpl(ISortableNode other, CompilerComparer comparer)
        {
            var otherNode = (IRvaEmbeddedPointerIndirectionNode<ISortableSymbolNode>) other;
            int result;

            if (CallSite != null || otherNode.CallSite != null)
            {
                if (CallSite == null) return -1;
                result = CallSite.CompareTo(otherNode.CallSite);
                if (result != 0) return result;
            }

            // We don't know whether other's generic type is the same as TTarget
            return comparer.Compare(Target, otherNode.Target);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.IO;

namespace R2RDump
{
    public abstract class TransitionBlock
    {
        public R2RReader _reader;

        public static TransitionBlock FromReader(R2RReader reader)
        {
            switch (reader.Architecture)
            {
                case Architecture.X86:
                    return X86TransitionBlock.Instance;

                case Architecture.X64:
                    return reader.OS == OperatingSystem.Windows ? X64WindowsTransitionBlock.Instance : X64UnixTransitionBlock.Instance;

                case Architecture.Arm:
                    return ArmTransitionBlock.Instance;

                case Architecture.Arm64:
                    return Arm64TransitionBlock.Instance;

                default:
                    throw new NotImplementedException();
            }
        }

        public abstract int PointerSize { get; }

        public abstract int NumArgumentRegisters { get;  }

        public abstract int NumPointersInReturnBlock { get; }

        public int SizeOfReturnBlock => NumPointersInReturnBlock * PointerSize;

        public int SizeOfArgumentRegisters => NumArgumentRegisters * PointerSize;

        public abstract int SizeOfTransitionBlock { get;  }

        public abstract int OffsetOfArgumentRegisters { get; }

        /// <summary>
        /// Recalculate pos in GC ref map to actual offset. This is the default implementation for all architectures
        /// except for X86 where it's overridden to supply a more complex algorithm.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public virtual int OffsetFromGCRefMapPos(int pos)
        {
            return OffsetOfArgumentRegisters + pos * PointerSize;
        }

        /// <summary>
        /// The transition block should define everything pushed by callee. The code assumes in number of places that
        /// end of the transition block is caller's stack pointer.
        /// </summary>
        public int OffsetOfArgs => SizeOfTransitionBlock;

        private sealed class X86TransitionBlock : TransitionBlock
        {
            public static readonly TransitionBlock Instance = new X86TransitionBlock();

            public override int PointerSize => 4;
            public override int NumArgumentRegisters => 2;
            public override int NumPointersInReturnBlock => 2;
            // Two extra pointers for EBP and return address
            public override int SizeOfTransitionBlock => SizeOfArgumentRegisters + SizeOfReturnBlock + 2 * PointerSize;
            public override int OffsetOfArgumentRegisters => 0;

            public override int OffsetFromGCRefMapPos(int pos)
            {
                if (pos < NumArgumentRegisters)
                {
                    return OffsetOfArgumentRegisters + SizeOfArgumentRegisters - (pos + 1) * PointerSize;
                }
                else
                {
                    return OffsetOfArgs + (pos - NumArgumentRegisters) * PointerSize;
                }
            }
        }

        private sealed class X64WindowsTransitionBlock : TransitionBlock
        {
            public static readonly TransitionBlock Instance = new X64WindowsTransitionBlock();

            public override int PointerSize => 8;
            public override int NumArgumentRegisters => 4;
            public override int NumPointersInReturnBlock => 1;
            // return block padding, return block, alignment padding, return address
            public override int SizeOfTransitionBlock => SizeOfReturnBlock + 3 * PointerSize;
            public override int OffsetOfArgumentRegisters => SizeOfTransitionBlock;
        }

        private sealed class X64UnixTransitionBlock : TransitionBlock
        {
            public static readonly TransitionBlock Instance = new X64UnixTransitionBlock();

            public override int PointerSize => 8;
            public override int NumArgumentRegisters => 6;
            public override int NumPointersInReturnBlock => 2;
            // Two extra pointers for alignment padding and return address
            public override int SizeOfTransitionBlock => SizeOfReturnBlock + SizeOfArgumentRegisters + 2 * PointerSize;
            public override int OffsetOfArgumentRegisters => SizeOfReturnBlock;
        }

        private sealed class ArmTransitionBlock : TransitionBlock
        {
            public static readonly TransitionBlock Instance = new ArmTransitionBlock();

            public override int PointerSize => 4;
            public override int NumArgumentRegisters => 4;
            public override int NumPointersInReturnBlock => 8;
            public override int SizeOfTransitionBlock => SizeOfReturnBlock + SizeOfArgumentRegisters;
            public override int OffsetOfArgumentRegisters => SizeOfReturnBlock;
        }

        private sealed class Arm64TransitionBlock : TransitionBlock
        {
            public static readonly TransitionBlock Instance = new Arm64TransitionBlock();

            public override int PointerSize => 8;
            public override int NumArgumentRegisters => 8;
            public override int NumPointersInReturnBlock => 4;
            // Extra pointer for alignment padding
            public override int SizeOfTransitionBlock => SizeOfReturnBlock + SizeOfArgumentRegisters + PointerSize;
            public override int OffsetOfArgumentRegisters => SizeOfReturnBlock;
        }
    }

}


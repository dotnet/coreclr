using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xunit.Performance;
using System.Runtime.CompilerServices;
using Xunit;
using System.Collections;
using System.Diagnostics;

[assembly: OptimizeForBenchmarks]
[assembly: MeasureInstructionsRetired]

internal struct Struct {
    public void Nop() { }
}

internal struct Unexpected {

    private int m_zeroOffset;
    private Struct m_nonZeroOffset;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Call() {
        m_nonZeroOffset.Nop();
        //cmp         dword ptr [rcx],ecx   <- unexpected
        //ret  
    }

}

internal struct Expected {

    private Struct m_zeroOffset;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Call() {
        m_zeroOffset.Nop();
        //ret      
    }
}

public static class Template {

#if DEBUG
    private const int Iterations = 1;
#else
    private const int Iterations = 10000;
#endif
    private const int CollectionCount = 10000;

    private interface IGenericEnumerator<TCollection, TPosition, T> {
        int GetVersion(TCollection collection);
        TPosition StepForward(TPosition position);
        T GetValue(TCollection collection, TPosition position);
        bool Equals(TPosition lhs, TPosition rhs);
    }

    private struct GenericEnumerator<TOperations, TCollection, TPosition, T> : IEnumerator<T>
        where TOperations : struct, IGenericEnumerator<TCollection, TPosition, T> {

        private TOperations m_operations;
        private TCollection m_collection;
        private int m_version;
        private T m_current;
        private TPosition m_position;
        private TPosition m_begin;
        private TPosition m_end;

        internal GenericEnumerator(TCollection collection, TPosition begin, TPosition end) : this() {
            m_operations = default(TOperations);
            m_collection = collection;
            m_begin = begin;
            m_end = end;
            Reset();
        }

        private bool MoveLast() {
            if (m_operations.GetVersion(m_collection) != m_version)
                throw new InvalidOperationException();

            m_current = default(T);
            return false;
        }

        public bool MoveNext() {
            var position = m_position;

            if (m_operations.GetVersion(m_collection) == m_version && !m_operations.Equals(position, m_end)) {
                //cmp         dword ptr [rcx],ecx           <-- remove
                //mov         r8,qword ptr [rcx]  
                //cmp         dword ptr [rcx+8],0  
                //jne         00007FFBE0F14F2C  
                //cmp         dword ptr [rcx],ecx           <-- remove
                //mov         r8d,dword ptr [rcx+18h]  
                //cmp         eax,r8d  
                //je          00007FFBE0F14F2C  

                m_current = m_operations.GetValue(m_collection, position);
                //cmp         dword ptr [rcx],ecx           <-- remove
                //mov         rdx,qword ptr [rcx]  
                //mov         r8d,dword ptr [rdx+8]  
                //cmp         eax,r8d  
                //jae         00007FFBE0F14F3D  
                //movsxd      r8,eax  
                //mov         edx,dword ptr [rdx+r8*4+10h]  
                //mov         dword ptr [rcx+0Ch],edx  

                m_position = m_operations.StepForward(position);
                //cmp         byte ptr [ecx],al             <-- remove
                //inc         esi  
                //mov         dword ptr [ecx+0Ch],esi

                return true;
            }

            return MoveLast();
        }

        public T Current => m_current;
        object IEnumerator.Current => m_current;

        public void Reset() {
            m_current = default(T);
            m_version = m_operations.GetVersion(m_collection);
            m_position = m_begin;
        }
        public void Dispose() {
            Reset();
            m_version = -1;
        }
    }

    private struct ArrayEnumerator<T> : IGenericEnumerator<T[], int, T> {
        public bool Equals(int lhs, int rhs) => lhs == rhs;
        public T GetValue(T[] collection, int position) => collection[position];
        public int GetVersion(T[] collection) => 0;
        public int StepForward(int iterator) => iterator + 1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Bench<TOperations, TCollection, TPosition, T>(
        GenericEnumerator<TOperations, TCollection, TPosition, T> enumerator)
        where TOperations : struct, IGenericEnumerator<TCollection, TPosition, T> {

        while (enumerator.MoveNext())
            continue;
    }

    private static void Bench() {
        var collection = new int[CollectionCount];
        var enumerator = new GenericEnumerator<ArrayEnumerator<int>, int[], int, int>(
            collection: collection,
            begin: 0,
            end: CollectionCount
        );

        Bench(enumerator);
    }

    [Benchmark]
    public static void Test() {
        foreach (var iteration in Benchmark.Iterations) {
            using (iteration.StartMeasurement()) {
                for (int i = 0; i < Iterations; i++)
                    Bench();
            }
        }
    }

    public static int Main() {

        //Debugger.Break();
        Console.WriteLine("Hello World!");
        //new Unexpected().Call();

        //new Expected().Call();

        //for (int i = 0; i < Iterations; i++)
        //    Bench();

        return 0;
    }
}
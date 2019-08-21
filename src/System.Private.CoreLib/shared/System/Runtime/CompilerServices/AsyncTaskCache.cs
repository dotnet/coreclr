// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
    /// <summary>Provides a cache of closed generic tasks for async methods.</summary>
    internal static class AsyncTaskCache
    {
        // All static members are initialized inline to ensure type is beforefieldinit

        /// <summary>A cached Task{Boolean}.Result == true.</summary>
        internal static readonly Task<bool> TrueTask = CreateCacheableTask(true);
        /// <summary>A cached Task{Boolean}.Result == false.</summary>
        internal static readonly Task<bool> FalseTask = CreateCacheableTask(false);

        /// <summary>The cache of Task{Int32}.</summary>
        internal static readonly Task<int>[] Int32Tasks = CreateInt32Tasks();
        /// <summary>The minimum value, inclusive, for which we want a cached task.</summary>
        internal const int INCLUSIVE_INT32_MIN = -1;
        /// <summary>The maximum value, exclusive, for which we want a cached task.</summary>
        internal const int EXCLUSIVE_INT32_MAX = 9;
        /// <summary>Creates an array of cached tasks for the values in the range [INCLUSIVE_MIN,EXCLUSIVE_MAX).</summary>
        private static Task<int>[] CreateInt32Tasks()
        {
            Debug.Assert(EXCLUSIVE_INT32_MAX >= INCLUSIVE_INT32_MIN, "Expected max to be at least min");
            var tasks = new Task<int>[EXCLUSIVE_INT32_MAX - INCLUSIVE_INT32_MIN];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = CreateCacheableTask(i + INCLUSIVE_INT32_MIN);
            }
            return tasks;
        }

        /// <summary>Creates a non-disposable task.</summary>
        /// <typeparam name="TResult">Specifies the result type.</typeparam>
        /// <param name="result">The result for the task.</param>
        /// <returns>The cacheable task.</returns>
        internal static Task<TResult> CreateCacheableTask<TResult>([AllowNull] TResult result) =>
            new Task<TResult>(false, result, (TaskCreationOptions)InternalTaskOptions.DoNotDispose, default);
    }
}

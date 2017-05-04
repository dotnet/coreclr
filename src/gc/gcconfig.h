// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This class is responsible for retreiving configuration information
// for how the GC should operate.
class GCConfig
{
public:
    // Whether or not we have been requested to use Server GC.
    static bool UseServerGC();

    // Whether or not we have been requested to use Concurrent GC.
    static bool UseConcurrentGC();
};

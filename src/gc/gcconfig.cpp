// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "gc.h"

bool GCConfig::UseServerGC()
{
    bool result = false;
    GCToEEInterface::GetBooleanConfigValue(BoolConfigKey::ServerGC, &result);
    return result;
}

bool GCConfig::UseConcurrentGC()
{
    bool result = true;
    GCToEEInterface::GetBooleanConfigValue(BoolConfigKey::ConcurrentGC, &result);
    return result;
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "gcenv.h"
#include "gc.h"

#define BOOL_CONFIG(name, key, default, unused_doc)            \
  bool GCConfig::Get##name()                                   \
  {                                                            \
      bool result = default;                                   \
      GCToEEInterface::GetBooleanConfigValue(key, &result);    \
      return result;                                           \
  }

#define INT_CONFIG(name, key, default, unused_doc)             \
  int64_t GCConfig::Get##name()                                \
  {                                                            \
      int64_t result = default;                                \
      GCToEEInterface::GetIntConfigValue(key, &result);        \
      return result;                                           \
  }

#define STRING_CONFIG(name, key, unused_doc)   \
  GCConfigStringHolder GCConfig::Get##name()                   \
  {                                                            \
      const char* resultStr = nullptr;                         \
      GCToEEInterface::GetStringConfigValue(key, &resultStr);  \
      return GCConfigStringHolder(resultStr);                  \
  }

GC_CONFIGURATION_KEYS

#undef BOOL_CONFIG
#undef INT_CONFIG
#undef STRING_CONFIG

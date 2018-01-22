// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "gcevents.h"

#define KNOWN_EVENT(name, provider, level, keyword) \
  gc_events::GCKnownEvent name##EventDescriptor(#name, provider, level, keyword);
#include "gcevents.def"

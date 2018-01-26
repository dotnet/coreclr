// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#ifndef KNOWN_EVENT
 #define KNOWN_EVENT(name, provider, level, keyword)
#endif // KNOWN_EVENT

#ifndef DYNAMIC_EVENT
 #define DYNAMIC_EVENT(name, level, keyword, ...)
#endif // DYNAMIC_EVENT

KNOWN_EVENT(GCStart_V2, GCEventProvider_Default, GCEventLevel_Information, GCEventKeyword_GC)
KNOWN_EVENT(GCEnd_V1, GCEventProvider_Default, GCEventLevel_Information, GCEventKeyword_GC)
KNOWN_EVENT(GCGenerationRange, GCEventProvider_Default, GCEventLevel_Information, GCEventKeyword_GCHeapSurvivalAndMovement)
KNOWN_EVENT(GCHeapStats_V1, GCEventProvider_Default, GCEventLevel_Information, GCEventKeyword_GC)
KNOWN_EVENT(GCCreateSegment_V1, GCEventProvider_Default, GCEventLevel_Information, GCEventKeyword_GC)
KNOWN_EVENT(GCFreeSegment_V1, GCEventProvider_Default, GCEventLevel_Information, GCEventKeyword_GC)
KNOWN_EVENT(GCCreateConcurrentThread_V1, GCEventProvider_Default, GCEventLevel_Information, GCEventKeyword_GC)
KNOWN_EVENT(GCTerminateConcurrentThread_V1, GCEventProvider_Default, GCEventLevel_Information, GCEventKeyword_GC)
KNOWN_EVENT(GCTriggered, GCEventProvider_Default, GCEventLevel_Information, GCEventKeyword_GC)

#undef KNOWN_EVENT
#undef DYNAMIC_EVENT

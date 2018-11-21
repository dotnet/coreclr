// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

[assembly:ImportedFromTypeLib("TypeEquivalenceTest")] // Required to support embeddable types
[assembly:Guid("3B491C47-B176-4CF3-8748-F19E303F1714")]

// 4D441C6F-90BB-4003-802E-2D2126E37380
// 42877991-3822-4B21-AC74-F441362D9FD1
// F0F1636E-EC86-46EF-BB8F-10B4BAB26AD4
// 1216AD93-30D5-477E-AC84-667A3675885B
// CCD43893-58AF-4B68-B587-CE81ECA00E78
// 47E1386F-22D2-4877-932D-3B1248F18D6F
// 52DFEAA8-515C-404C-A9D4-18821680D96E
// D7233C55-201F-4250-B8DA-972BE4E5C050

namespace TypeEquivalenceTypes
{
    [ComImport]
    [Guid("F34D4DE8-B891-4D73-B177-C8F1139A9A67")]
    public interface IEmptyType
    {
    }
}

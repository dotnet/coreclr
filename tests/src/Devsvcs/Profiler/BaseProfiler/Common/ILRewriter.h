// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma once

HRESULT RewriteIL(
    ICorProfilerInfo * pICorProfilerInfo,
    ICorProfilerFunctionControl * pICorProfilerFunctionControl,
    ModuleID moduleID,
    mdMethodDef methodDef,
    FunctionID functionId,
    UINT_PTR enterMethodAddress,
    UINT_PTR exitMethodAddress,
    ULONG32 methodSignature);
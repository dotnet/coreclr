// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "compiler.h"
#include "phase.h"

class CopyPropogation : public Phase
{
public:
    CopyPropogation(Compiler* compiler);

    virtual void DoPhase() override;

private:
};

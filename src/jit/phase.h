// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*****************************************************************************/
#ifndef _PHASE_H_
#define _PHASE_H_

// Specify which checks a phase should run
//
enum class PhaseChecks
{
    NONE,
    ALL
};

// A phase encapsulates a part of the compilation pipeline for a method.
//
class Phase
{
public:
    virtual void Run();

protected:
    Phase(Compiler* _comp, Phases _phase, PhaseChecks _checks = PhaseChecks::ALL)
        : comp(_comp), name(nullptr), phase(_phase), checks(_checks)
    {
        name = PhaseNames[_phase];
    }

    virtual void PrePhase();
    virtual void DoPhase() = 0;
    virtual void PostPhase();

    Compiler*   comp;
    const char* name;
    Phases      phase;
    PhaseChecks checks;
};

inline void Phase::Run()
{
    PrePhase();
    DoPhase();
    PostPhase();
}

inline void Phase::PrePhase()
{
    comp->BeginPhase(phase);

#ifdef DEBUG
    if (VERBOSE)
    {
        if (comp->compIsForInlining())
        {
            printf("\n*************** Inline @[%06u] Starting PHASE %s\n",
                   Compiler::dspTreeID(comp->impInlineInfo->iciCall), name);
        }
        else
        {
            printf("\n*************** Starting PHASE %s\n", name);
        }
    }

    if ((checks == PhaseChecks::ALL) && (comp->expensiveDebugCheckLevel >= 2))
    {
        // If everyone used the Phase class, this would duplicate the PostPhase() from the previous phase.
        // But, not everyone does, so go ahead and do the check here, too.
        comp->fgDebugCheckBBlist();
        comp->fgDebugCheckLinks();
    }
#endif // DEBUG
}

inline void Phase::PostPhase()
{
#ifdef DEBUG
    if (VERBOSE)
    {
        if (comp->compIsForInlining())
        {
            printf("\n*************** Inline @[%06u] Finishing PHASE %s\n",
                   Compiler::dspTreeID(comp->impInlineInfo->iciCall), name);
        }
        else
        {
            printf("\n*************** Finishing PHASE %s\n", name);
        }

        printf("Trees after %s\n", name);
        comp->fgDispBasicBlocks(true);
    }

    if (checks == PhaseChecks::ALL)
    {
        comp->fgDebugCheckBBlist();
        comp->fgDebugCheckLinks();
        comp->fgDebugCheckNodesUniqueness();
    }
#endif // DEBUG

    comp->EndPhase(phase);
}

// A phase that accepts a lambda for the actions done by the phase.
//
// Would prefer to use std::function via <functional> here, but seemingly can't.
//
template <typename A>
class ActionPhase final : public Phase
{
public:
    ActionPhase(Compiler* _comp, Phases _phase, A _action, PhaseChecks _checks)
        : Phase(_comp, _phase, _checks), action(_action)
    {
    }

protected:
    virtual void DoPhase() override
    {
        action();
    }

private:
    A action;
};

// Wrapper for using ActionPhase
//
template <typename A>
void DoPhase(Compiler* _comp, Phases _phase, A _action, PhaseChecks _checks = PhaseChecks::ALL)
{
    ActionPhase<A> phase(_comp, _phase, _action, _checks);
    phase.Run();
}

// A simple phase that just invokes a method on the compiler instance
//
class CompilerPhase final : public Phase
{
public:
    CompilerPhase(Compiler* _comp, Phases _phase, void (Compiler::*_action)(), PhaseChecks _checks)
        : Phase(_comp, _phase, _checks), action(_action)
    {
    }

protected:
    virtual void DoPhase() override
    {
        (comp->*action)();
    }

private:
    void (Compiler::*action)();
};

// Wrapper for using CompilePhase
//
inline void DoPhase(Compiler* _comp, Phases _phase, void (Compiler::*_action)(), PhaseChecks _checks = PhaseChecks::ALL)
{
    CompilerPhase phase(_comp, _phase, _action, _checks);
    phase.Run();
}

#endif /* End of _PHASE_H_ */

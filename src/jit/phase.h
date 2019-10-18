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
    Phase(Compiler* _compiler, Phases _phase, PhaseChecks _checks = PhaseChecks::ALL)
        : comp(_compiler), m_name(nullptr), m_phase(_phase), m_checks(_checks)
    {
        m_name = PhaseNames[_phase];
    }

    virtual void PrePhase();
    virtual void DoPhase() = 0;
    virtual void PostPhase();

    Compiler*   comp;
    const char* m_name;
    Phases      m_phase;
    PhaseChecks m_checks;
};

// A phase that accepts a lambda for the actions done by the phase.
//
// Would prefer to use std::function via <functional> here, but seemingly can't.
//
template <typename A>
class ActionPhase final : public Phase
{
public:
    ActionPhase(Compiler* _compiler, Phases _phase, A _action, PhaseChecks _checks)
        : Phase(_compiler, _phase, _checks), action(_action)
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
void DoPhase(Compiler* _compiler, Phases _phase, A _action, PhaseChecks _checks = PhaseChecks::ALL)
{
    ActionPhase<A> phase(_compiler, _phase, _action, _checks);
    phase.Run();
}

// A simple phase that just invokes a method on the compiler instance
//
class CompilerPhase final : public Phase
{
public:
    CompilerPhase(Compiler* _compiler, Phases _phase, void (Compiler::*_action)(), PhaseChecks _checks)
        : Phase(_compiler, _phase, _checks), action(_action)
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
inline void DoPhase(Compiler* _compiler,
                    Phases    _phase,
                    void (Compiler::*_action)(),
                    PhaseChecks      _checks = PhaseChecks::ALL)
{
    CompilerPhase phase(_compiler, _phase, _action, _checks);
    phase.Run();
}

#endif /* End of _PHASE_H_ */

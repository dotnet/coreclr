#include <windows.h>
#include <winnt.h>
#include <process.h>
#include "ProfilerCommon.h"

////////////////////////////////////////////////////////////////////////////////
//
//  Utility (convenience) functions
//
////////////////////////////////////////////////////////////////////////////////

IPrfCom *pPrfCom = NULL;

//
// MUST_PASS will assert if the call returns failing HR.
//
#undef  MUST_PASS
#define MUST_PASS(call)                         \
    do {                                          \
        HRESULT __hr = (call);                                          \
        if( FAILED(__hr) ) {                                            \
            FAILURE(L"call '" << L#call << L"' failed with HR=" << __hr); \
            throw std::logic_error("unexpected profiler call failed");  \
        }                                                               \
    } while(0);

//
// Helper class for simple allocation of buffer. Used mainly for functions
// that are expecting arrays.
//
template<class char_type , int bufferLength=255, class SizeType=ULONG32>
class BufferHolder
{
public:
    BufferHolder()
        : returned_(0)
    {
    }

    int size()  // returns size of buffer in items
    {
        return bufferLength;
    }

    char_type* buffer() // returns pointer to the buffer.
    {
        return static_buffer_;
    }

    SizeType* returned() // returns a reference to the returned size.
    {
        return &returned_;
    }

private:
    char_type static_buffer_[bufferLength];
    SizeType returned_;
};

class CriticalSection
{
public:
    CriticalSection()
    {
        ::InitializeCriticalSection(&cs_);
    }

    friend class Lock;
private:
    CRITICAL_SECTION cs_;
};

class Lock
{
public:
    Lock(CriticalSection& cs)
        : cs_(cs)
    {
        ::EnterCriticalSection(&cs_.cs_);
    }

    ~Lock()
    {
        ::LeaveCriticalSection(&cs_.cs_);
    }
private:
    CriticalSection& cs_;
};



////////////////////////////////////////////////////////////////////////////////
//
//  Structures holding info about profilers
//
////////////////////////////////////////////////////////////////////////////////

// info about a frame in the stack
struct StackFrameInfo
{
    StackFrameInfo(FunctionID funcId, UINT_PTR ip);

    std::wstring functionName() const;

    // always set
    FunctionID funcId;
    UINT_PTR ip;

    // optionally set
    ClassID classId;
    ModuleID moduleId;
    mdToken token;
    std::wstring funcName;
};

typedef std::vector<StackFrameInfo> StackFrames_t;

class ExceptionInFlight {
public:
    enum ExceptionState {
        FirstPass                   = 0x001,
        FirstPassSearchOne          = 0x002,
        FirstPassCatchHandlerFound  = 0x004,
        FirstPassExecuteFilter      = 0x008,
        SecondPass                  = 0x010,
        SecondPassUnwindOne         = 0x020,
        SecondPassExecuteFinally    = 0x040,
        SecondPassExecuteCatch      = 0x100
    };

    ExceptionInFlight(ObjectID objectID);

    void assertState(int validStates) const;
    void changeState(ExceptionState newState);

    ExceptionState state() const
    {
        return state_;
    }

    StackFrames_t& stack()
    {
        return exceptionLocation_;
    }

    const StackFrames_t& stack() const
    {
        return exceptionLocation_;
    }

    StackFrames_t::const_iterator& catchingFrame()
    {
        return catchingFrame_;
    }

    const StackFrames_t::const_iterator& catchingFrame() const
    {
        return catchingFrame_;
    }

    bool hasNativeHandler() const
    {
        return hasNativeHandler_;
    }

    void hasNativeHandler(bool value)
    {
        hasNativeHandler_ = value;
    }

    // returns current frame if we are stackwalking
    // or null if we haven't started yet.
    const StackFrames_t::const_iterator* currentFrame() const;
    void switchToUnwindPhase(StackFrames_t::const_iterator
                             catchingStack);

    void enterNewManagedFrame(FunctionID funcID);

    void checkCallStack() const;

    std::wstring toString() const;

    bool hasNativeHandler_;
private:
    std::wstring stateToString(int state) const;
    void takeCallstack(StackFrames_t& callback) const;
    void resetStackWalk();

    ObjectID objectID_;
    ExceptionState state_;
    StackFrames_t exceptionLocation_;
    StackFrames_t::const_iterator currentFrame_;
    StackFrames_t::const_iterator catchingFrame_;
    bool frameWalkStarted_;
    bool unwindPhase_;
};

class ExceptionsOnThread {
public:
    static bool profileASP;
    static ExceptionsOnThread& thread_instance();
    static void register_thread(ThreadID threadiD);
    static void unregister_thread(ThreadID threadID);
    static void verifyNoActiveExceptionOnAnyThread();

    void exceptionThrown(ObjectID exception);
    void exceptionSearchFunctionEnter(FunctionID functionId);
    void exceptionSearchFunctionLeave();

    void exceptionSearchFilterEnter(FunctionID functionId);
    void exceptionSearchFilterLeave();

    void exceptionSearchCatcherFound(FunctionID functionId);


    void exceptionUnwindFunctionEnter(FunctionID functionId);
    void exceptionUnwindFunctionLeave();

    void exceptionUnwindFinallyEnter(FunctionID functionId);
    void exceptionUnwindFinallyLeave();

    void exceptionCatcherEnter(FunctionID functionId, ObjectID objectId);
    void exceptionCatcherLeave();
private:
    ExceptionsOnThread(ThreadID threadId);

    ExceptionInFlight& topException();
    void verifyNoActiveExceptions();

    typedef std::list<ExceptionInFlight> ExceptionStack_t;
    ExceptionStack_t exceptionStack_;
    const ThreadID threadId_;
};

////////////////////////////////////////////////////////////////////////////////
//
// Implementation
//
////////////////////////////////////////////////////////////////////////////////
namespace /*private*/ { // private implementation part

typedef std::map<ThreadID, ExceptionsOnThread> thread_map_t;

thread_map_t thread_map;

}; // namespace /*private*/

struct FrameComparer
{
    bool operator () (const StackFrameInfo& a,const StackFrameInfo& b)
    {
        bool match = a.funcId==b.funcId && a.classId==b.classId && a.moduleId==b.moduleId &&
            a.token == b.token;
#if 0
        if( !match )
            FAILURE(( "Frame comparision failed" ));
#endif
        return match;
    }
};

StackFrameInfo generateCurrentStackFrameInfo(/*OUT*/ COR_PRF_EX_CLAUSE_INFO* pEci = NULL )
{
    COR_PRF_EX_CLAUSE_INFO eci;
    HRESULT hr;
    hr = pPrfCom->m_pInfo->GetNotifiedExceptionClauseInfo(&eci);
    if( hr==S_FALSE )
        FAILURE(L"GetNotifiedExceptionClauseInfo returned S_FALSE");

    FunctionID functionID;
    MUST_PASS( pPrfCom->m_pInfo->GetFunctionFromIP( reinterpret_cast<LPCBYTE>(eci.programCounter), // casting BUGBUG #502513.
                                                    &functionID) );

    StackFrameInfo sfi(functionID, eci.programCounter);

    if( functionID!=0 )
    {
        // this means a managed frame
        MUST_PASS( pPrfCom->m_pInfo->GetFunctionInfo(functionID, &sfi.classId,
                                                     &sfi.moduleId, &sfi.token) );
    }
    if( pEci )
        *pEci = eci;                                        // copy out the result if requested.

    return sfi;
}

ExceptionsOnThread& ExceptionsOnThread::thread_instance()
{
    ThreadID tid;
    MUST_PASS( pPrfCom->m_pInfo->GetCurrentThreadID(&tid) );
    thread_map_t::iterator i = thread_map.find(tid);
    if( i == thread_map.end() )
        // not found
        FAILURE(L"Exception on undefind thread");
    return i->second;
}

namespace /*private*/
{
CriticalSection g_threadLock;
}

void ExceptionsOnThread::verifyNoActiveExceptions()
{
    // check that we don't have any active exception there.
    std::wstring exs;
    bool failed=false;
    for(ExceptionStack_t::iterator j= exceptionStack_.begin(); j!=exceptionStack_.end(); ++j)
    {
        exs+= j->toString();
        if( !( j->state() & (ExceptionInFlight::SecondPassExecuteCatch | ExceptionInFlight::SecondPassExecuteFinally) ) )
            failed = true;
    }
    if( failed )
        FAILURE(L"Active exception on the thread " << HEX(threadId_) << L":" << exs.c_str());
}


#define REPORT(method) DISPLAY(L"(TID: " << HEX(threadId_) << L") CALLBACK " << L#method);

#define REPORT1(method,num) {                                   \
        WCHAR_STR( bh );                                        \
        MUST_PASS( pPrfCom->GetFunctionIDName(num, bh));        \
        DISPLAY(L"(TID: " << HEX(threadId_) << L") CALLBACK " << L#method << L" (" << HEX(num) << L" - " << bh << L")"); \
    }    

void ExceptionsOnThread::register_thread(ThreadID threadId_)
{
    REPORT(register_thread);
    Lock lock(g_threadLock);
    thread_map_t::iterator i = thread_map.find(threadId_);
    if( i!=thread_map.end() )
        FAILURE(L"Re-registering existing thread - tid:" << threadId_);
    thread_map.insert(thread_map_t::value_type(threadId_, ExceptionsOnThread(threadId_)));
}

void ExceptionsOnThread::unregister_thread(ThreadID threadId_)
{
    REPORT(unregister_thread);
    Lock lock(g_threadLock);

    CriticalSection g_threadLock;
    thread_map_t::iterator i = thread_map.find(threadId_);
    if( i==thread_map.end() )
    {
        FAILURE(L"Degistering nonexisting thread - tid:" << threadId_);
    }
    else
    {
        if (profileASP == false)
            i->second.verifyNoActiveExceptions();
        thread_map.erase(i);
    }
}
void ExceptionsOnThread::verifyNoActiveExceptionOnAnyThread()
{
    if( !thread_map.empty() )
    {
        if (profileASP == false)
        {
            for(thread_map_t::iterator i=thread_map.begin();
                i != thread_map.end(); ++i)
            {
                i->second.verifyNoActiveExceptions();
            }
        }
    }
}

void ExceptionsOnThread::exceptionThrown(ObjectID exception)
{
    REPORT(exceptionThrown);
    if( !exceptionStack_.empty() )
        topException().assertState(ExceptionInFlight::FirstPassExecuteFilter
                                   | ExceptionInFlight::SecondPassExecuteFinally
                                   | ExceptionInFlight::SecondPassExecuteCatch);

    exceptionStack_.push_front(ExceptionInFlight(exception));
}

void ExceptionsOnThread::exceptionSearchFunctionEnter(FunctionID functionId)
{
    REPORT1(exceptionSearchFunctionEnter, functionId);
    topException().assertState(ExceptionInFlight::FirstPass);
    topException().changeState(ExceptionInFlight::FirstPassSearchOne);
    topException().enterNewManagedFrame(functionId);
    topException().checkCallStack();
}

void ExceptionsOnThread::exceptionSearchFunctionLeave()
{
    REPORT(exceptionSearchFunctionLeave);
    topException().assertState(ExceptionInFlight::FirstPassCatchHandlerFound
                               | ExceptionInFlight::FirstPassSearchOne);

    topException().checkCallStack();
    if( topException().state() == ExceptionInFlight::FirstPassSearchOne )
    {
        topException().changeState(ExceptionInFlight::FirstPass);
    }
    else
    {
        // must be in CatchHandlerFound
        topException().changeState(ExceptionInFlight::SecondPass);
        topException().switchToUnwindPhase(*topException().currentFrame());
    }
}

void ExceptionsOnThread::exceptionSearchFilterEnter(FunctionID functionId)
{
    REPORT1(exceptionSearchFilterEnter, functionId);
    topException().assertState(ExceptionInFlight::FirstPassSearchOne);
    topException().changeState(ExceptionInFlight::FirstPassExecuteFilter);
    topException().checkCallStack();
}

void ExceptionsOnThread::exceptionSearchFilterLeave()
{
    REPORT(exceptionSearchFilterLeave);
    topException().assertState(ExceptionInFlight::FirstPassExecuteFilter);
    topException().changeState(ExceptionInFlight::FirstPassSearchOne);
    topException().checkCallStack();
}

void ExceptionsOnThread::exceptionSearchCatcherFound(FunctionID functionId)
{
    REPORT1(exceptionSearchCatcherFound, functionId);
    topException().assertState(ExceptionInFlight::FirstPassSearchOne);
    topException().changeState(ExceptionInFlight::FirstPassCatchHandlerFound);
    topException().checkCallStack();
}

void ExceptionsOnThread::exceptionUnwindFunctionEnter(FunctionID functionId)
{
    REPORT1(exceptionUnwindFunctionEnter,functionId);
    if( topException().state() == ExceptionInFlight::FirstPass )
    {
        // this would mean that exception has been handled in native code
        // let's check that next frame is native code
        if( topException().currentFrame()==NULL )
            FAILURE(L"Unwinding without Search phase");
        StackFrames_t::const_iterator i = *topException().currentFrame();

        ++i;
        if( i==topException().stack().end() || i->funcId!=0 )
            FAILURE(L"Transition to unwind phase without native frames");
        topException().assertState(ExceptionInFlight::FirstPass);

        topException().switchToUnwindPhase(*topException().currentFrame());
        topException().hasNativeHandler(true);
    }
    else
    {
        topException().assertState(ExceptionInFlight::SecondPass);
    }

    topException().changeState(ExceptionInFlight::SecondPassUnwindOne);
    topException().enterNewManagedFrame(functionId);
    if( (*topException().currentFrame()) > topException().catchingFrame() )
        FAILURE(L"Unwinding more frames than were searched");

    //we cannot topException().checkCallStack() because during unwinding
    //there is no API that allows us to do so.
}

void ExceptionsOnThread::exceptionUnwindFunctionLeave()
{
    REPORT(exceptionUnwindFunctionLeave);
    topException().assertState(ExceptionInFlight::SecondPassUnwindOne);
    topException().changeState(ExceptionInFlight::SecondPass);

    //we cannot topException().checkCallStack() because during unwinding
    //there is no API that allows us to do so.

    if( *topException().currentFrame() == topException().catchingFrame()
        && topException().hasNativeHandler() )
    {
        // exception handling is done for this exception
        exceptionStack_.pop_front();
    }
}

void ExceptionsOnThread::exceptionUnwindFinallyEnter(FunctionID functionId)
{
    REPORT1(exceptionUnwindFinallyEnter,functionId);
    topException().assertState(/*ExceptionInFlight::SecondPass/ | */ExceptionInFlight::SecondPassUnwindOne);
    topException().changeState(ExceptionInFlight::SecondPassExecuteFinally);

    topException().checkCallStack();
}

void ExceptionsOnThread::exceptionUnwindFinallyLeave()
{
    REPORT(exceptionUnwindFinallyLeave);
    topException().checkCallStack();

    topException().assertState(ExceptionInFlight::SecondPassExecuteFinally);
    topException().changeState(ExceptionInFlight::SecondPassUnwindOne);
}

void ExceptionsOnThread::exceptionCatcherEnter(FunctionID functionId, ObjectID objectId)
{
    REPORT1(exceptionCatcherEnter, functionId);
    topException().assertState(ExceptionInFlight::SecondPassUnwindOne);
    topException().changeState(ExceptionInFlight::SecondPassExecuteCatch);
    if( (*topException().currentFrame()) != topException().catchingFrame() )
        FAILURE(L"Exception caught in different frame than searched!");

    topException().checkCallStack();
}

void ExceptionsOnThread::exceptionCatcherLeave()
{
    REPORT(exceptionCatcherLeave);

    // find-out what stack we are at ...
    StackFrameInfo& sfi = generateCurrentStackFrameInfo();
    bool displayedGetNotifiedExceptionClauseInfoResult = false;

    // ... and elminiate exceptions that has been "killed" in flight.
    // exception can be killed in flight by another exception as in
    // e.g. following statement:
    // try {
    //    throw new MyException();
    // } finally {
    //    throw new MyNewException();
    // }
    ExceptionStack_t::iterator i = exceptionStack_.begin();
    while( i != exceptionStack_.end() )
    {
        ExceptionStack_t::iterator c = i++;
        bool needToSkip;
        if( c->state() != ExceptionInFlight::SecondPassExecuteCatch )
        {
            c->assertState( ExceptionInFlight::FirstPassExecuteFilter | ExceptionInFlight::SecondPassExecuteFinally );
            needToSkip = true;
        }
        else
        {
        // check if this exception ends-up in correct place
            FrameComparer fc;
            if( fc( **c->currentFrame(), sfi) )
                break;
            else
                needToSkip = true;
        }
        if( needToSkip )
        {
            if( !displayedGetNotifiedExceptionClauseInfoResult )
            {
                DISPLAY(L"GetNotifiedExceptionClauseInfo reported that we are at: " << HEX(sfi.funcId) << " " <<  sfi.functionName().c_str());
                displayedGetNotifiedExceptionClauseInfoResult = true;
            }
            DISPLAY(L"Skipping exception -  " <<c->toString().c_str());
            exceptionStack_.erase( c );
        }
    }
    topException().assertState(ExceptionInFlight::SecondPassExecuteCatch);

    topException().checkCallStack();

    // this is end of exception
    exceptionStack_.pop_front();
}

ExceptionsOnThread::ExceptionsOnThread(ThreadID threadId)
    : threadId_(threadId)
{
}


ExceptionInFlight& ExceptionsOnThread::topException()
{
    if( exceptionStack_.empty() )
        FAILURE(L"No exception on thread --- expected at least one!");
    return exceptionStack_.front();
}

StackFrameInfo::StackFrameInfo(FunctionID funcId, UINT_PTR ip)
    : ip(ip), funcId(funcId),
      classId(0), moduleId(0), token(0)
{
     if( funcId==0 )
    {
        // this means native code
        funcName = std::wstring(L"<native frames>");
    }
    else
    {
        WCHAR_STR( name );
        MUST_PASS( pPrfCom->GetFunctionIDName(this->funcId, name) );
        funcName = name;
    }
}

std::wstring StackFrameInfo::functionName() const
{
    return funcName;
}

ExceptionInFlight::ExceptionInFlight(ObjectID objectID)
    : objectID_(objectID), state_(FirstPass),
      unwindPhase_(false), hasNativeHandler_(false)
{
    takeCallstack(exceptionLocation_);
    resetStackWalk();
}


void ExceptionInFlight::assertState(int validStates) const
{
    if( !(state_ & validStates) )
    {
        std::wstring txt = toString();
        FAILURE(L"Wrong exception state:\n current: " << stateToString(state_).c_str() << L"\n expected: " << stateToString( validStates).c_str() << L"\n log: " << txt.c_str());
    }
}

void ExceptionInFlight::changeState(ExceptionState newState)
{
    state_ = newState;
}

const StackFrames_t::const_iterator* ExceptionInFlight::currentFrame() const
{
    return (frameWalkStarted_?&currentFrame_:NULL);
}

void ExceptionInFlight::switchToUnwindPhase(StackFrames_t::const_iterator
                                            catchingFrame)
{
    catchingFrame_ = catchingFrame;
    unwindPhase_ = true;
    resetStackWalk();
}

void ExceptionInFlight::enterNewManagedFrame(FunctionID funcID)
{
    if( !frameWalkStarted_ )
    {
        frameWalkStarted_ = true;
        currentFrame_ = exceptionLocation_.begin();
        if( currentFrame_==exceptionLocation_.end() )
            FAILURE(L"Started stackwalking without any managed frames");
    }
    else
    {
        ++currentFrame_;
        if( currentFrame_ == exceptionLocation_.end() )
            FAILURE(L"Extra new frame entered (funcID: " << HEX(funcID) << L")");
    }
    if( funcID!= currentFrame_->funcId )
        FAILURE(L"Expected to enter frame " << currentFrame_-exceptionLocation_.begin() << L" entered funcid " << HEX(funcID));
}

void printStack(std::wostream& s,const StackFrames_t& stack)
{
    int idx  = 0;
    for(StackFrames_t::const_iterator i = stack.begin();
        i!=stack.end(); ++i)
        s << "\t" << idx++ << ". " << i->functionName().c_str() << "\n";
}


void ExceptionInFlight::checkCallStack() const
{
    if( state() & (FirstPassExecuteFilter | SecondPassExecuteCatch | SecondPassExecuteFinally) )
    {
        COR_PRF_EX_CLAUSE_INFO eci;
        StackFrameInfo& sfi = generateCurrentStackFrameInfo( &eci );

        bool inconsistent = false;
        switch( eci.clauseType ) {
        case COR_PRF_CLAUSE_FILTER:
            if( state() != FirstPassExecuteFilter )
                inconsistent = true;
            break;
        case COR_PRF_CLAUSE_CATCH:
            if( state() != SecondPassExecuteCatch )
                inconsistent = true;
            break;
        case COR_PRF_CLAUSE_FINALLY:
            if( state() != SecondPassExecuteFinally )
                inconsistent = true;
            break;
        case COR_PRF_CLAUSE_NONE:
        default:
            inconsistent = true;
            break;
        }
        if( inconsistent )
            FAILURE(L"GetNotifiedExceptionClauseInfo - inconsistent state (clauseType==" << eci.clauseType << L"; Exception state: " << stateToString( state()));

        FrameComparer fc;
        if( currentFrame()==NULL ||
            ! fc( sfi, **currentFrame() ) )
        {
            std::wostringstream os;
            os << L"GetNotifiedExceptionClauseInfo returned inconsistent information to shadow stack\n"
               << L"we're located at: \n" << sfi.functionName() << L"(0x" << hex << sfi.funcId << L") \n\n"
               << L"current exception state: \n" << toString();
            FAILURE(( os.str().c_str() ));
        }
    }
    else
    {
#if 0
        // @TODO Due to a lack of documentation I don't know how this API should really work.
        // The .idl comments say that the GetNotifiedExceptionClauseInfo should only work on
        // {catch,finalle,filter}{Enter,Leave} callbacks and return S_FALSE otherwise.
        // This suggest that it shoudl return S_FALSE in all other callbakcs. This code bellow
        // is trying to verify that but it is failing. Apparently in cases of nested exception
        // the rules are more complicated. Need to clarify.
        COR_PRF_EX_CLAUSE_INFO eci;
        HRESULT hr;
        MUST_PASS( hr = pPrfCom->m_pInfo->GetNotifiedExceptionClauseInfo(&eci) );
        if( hr!=S_FALSE )
            FAILURE(( "Expected GetNotifiedExceptionClauseInfo to return S_FALSE" ));
#endif
    }

    if( !unwindPhase_ )
    {
        // we're in the 1st phase and therefore the DoStackSnapshot should work.
        StackFrames_t currentCallstack;
        takeCallstack(currentCallstack);

        if( currentCallstack.size()==0 )
        {
            DISPLAY(L"TakeSnapshotCallstack function returned 0 frames");
            return;
        }

        bool callstackMismatch = false;
        if( exceptionLocation_.size() != currentCallstack.size() )
            callstackMismatch = true;
        else
        {
            callstackMismatch = !equal(exceptionLocation_.begin(), exceptionLocation_.end(),
                                       currentCallstack.begin(), FrameComparer());
        }
        if( callstackMismatch )
        {
            std::wostringstream os;
            os << L"CheckCallStack failed --- Mismatched callstacks:\n"
               << L"freshly taken callstack: \n";
            printStack(os, currentCallstack);
            os << L"Current exception state: \n" << toString().c_str();
            FAILURE(( os.str().c_str() ));
        }
    }
}


std::wstring ExceptionInFlight::toString() const
{
    std::wostringstream txt;

    txt << L"Exception (0x" << hex << objectID_ << L"): \n";
    printStack(txt, exceptionLocation_);
    txt << L"Current frame: ";
    if( currentFrame()==NULL )
        txt << L"<NONE>";
    else
        txt << (*currentFrame() - exceptionLocation_.begin());
    txt << L"\nCurrent state: " << stateToString(state()).c_str();
    return txt.str();
}

void ExceptionInFlight::resetStackWalk()
{
    frameWalkStarted_ = false;
}

std::wstring ExceptionInFlight::stateToString(int exceptionState) const
{
    struct StateNames {
        int value;
        const wchar_t* name;
    };

#define VALUE(value) {ExceptionInFlight:: value, L#value}
    static const StateNames names[] = {
        VALUE(FirstPass),
        VALUE(FirstPassSearchOne),
        VALUE(FirstPassCatchHandlerFound),
        VALUE(FirstPassExecuteFilter),
        VALUE(SecondPass),
        VALUE(SecondPassUnwindOne),
        VALUE(SecondPassExecuteFinally),
        VALUE(SecondPassExecuteCatch)
    };
#undef VALUE

    std::wstring txt;
    for(int i=0; i< sizeof(names)/sizeof names[0]; ++i )
    {
        if( names[i].value & exceptionState ) {
            if( !txt.empty() )
                txt+=L", ";
            txt+= names[i].name;
        }
    }
    if( txt.empty() )
    {
        txt+=L"<invalid value> ";
        BufferHolder<WCHAR> bh;
        if( _itow_s(exceptionState, bh.buffer(), bh.size(), 16) == 0 )
            txt+=bh.buffer();
    }
    return txt;
}

namespace /*private*/
{
    struct TakeSnapshotCallback
    {
		BOOL bWalkFrameDone;

        TakeSnapshotCallback(IPrfCom* prfCom, StackFrames_t& stackFames, ThreadID threadID)
            : prfCom(prfCom), stackFames(stackFames), threadID(threadID)
        {
        }

        static HRESULT __stdcall MySnapshotCallback(FunctionID funcId,
                                                    UINT_PTR ip,
                                                    COR_PRF_FRAME_INFO frameInfo,
                                                    ULONG32 contextSize,
                                                    BYTE context[],
                                                    void *clientData)
        {
            TakeSnapshotCallback& callbackData = *static_cast<TakeSnapshotCallback*>(clientData);

            StackFrameInfo sfi(funcId, ip);
            if( funcId!=0 )
            {
                // this means a managed frame
                MUST_PASS( callbackData.prfCom->m_pInfo->GetFunctionInfo2(funcId, frameInfo,
                                                                          &sfi.classId, &sfi.moduleId, &sfi.token, 0, NULL, NULL) );
            }
            callbackData.stackFames.push_back(sfi);
            return S_OK;
        }
#ifdef _ARM_
		BOOL WalkFrame(CONTEXT* pCtx)
		{
			if (pCtx->Pc == 0)
			{
				// All done
				return FALSE;
			}		
			

			DWORD dwImageBase;
			PRUNTIME_FUNCTION pRuntimeFunction =  RtlLookupFunctionEntry(
				pCtx->Pc,
				&dwImageBase,
				NULL);  // HistoryTable

			if (pRuntimeFunction == NULL)
			{
				// Nested functions that do not use any stack space or nonvolatile
				// registers are not required to have unwind info (ex.
				// USER32!ZwUserCreateWindowEx).				
				pCtx->Pc = (ULONG32)(*(PULONG32)pCtx->Sp);
				pCtx->Sp += sizeof(ULONG32);
				return TRUE;
			}

			// For debugging purposes, reserve a clean copy of what we're passing in to
			// RtlVirtualUnwind that we know has not been modified by RtlVirtualUnwind
			CONTEXT ctxOld;
			memcpy(&ctxOld, pCtx, sizeof(ctxOld));

			LPVOID pHandlerData = NULL;
			DWORD dwEstablisherFrame;

			PEXCEPTION_ROUTINE pExceptionRoutine = NULL;	

			/*FunctionID funcID = NULL;
			HRESULT hr = pPrfCom->m_pInfo->GetFunctionFromIP((LPCBYTE)pCtx->Pc, &funcID);
			if( funcID!=0 )
			{
				// this means a managed frame
				DISPLAY("MANAGED frame found");
			}*/
			
			__try
			{
				pExceptionRoutine = RtlVirtualUnwind(
					NULL,               // HandlerType,
					dwImageBase,
					pCtx->Pc,
					pRuntimeFunction,
					pCtx,
					&pHandlerData,
					&dwEstablisherFrame,
					NULL);              // ContextPointers
			}
			__except(EXCEPTION_EXECUTE_HANDLER)
			{
				// RtlVirtualUnwind is known to AV sometimes when the target thread is
				// unwalkable. The example we regularly hit is when the target is suspended
				// toward the end of RtlRestoreContext, after rbp has been modified.			
				return FALSE;
			}
			
			return TRUE;
		}



		void WalkStack(CONTEXT * pCtx)
		{
			BOOL fContinue;
			do
			{			
				FunctionID funcID = NULL;		
				HRESULT hr = pPrfCom->m_pInfo->GetFunctionFromIP((LPCBYTE)pCtx->Pc, &funcID);		
					
				StackFrameInfo sfi(funcID, pCtx->Pc);
				if( funcID!=0 )
				{
					// this means a managed frame					
					MUST_PASS( prfCom->m_pInfo->GetFunctionInfo2(funcID, 0, &sfi.classId, &sfi.moduleId, &sfi.token, 0, NULL, NULL) );
					stackFames.push_back(sfi);
				}
				
				
				fContinue = WalkFrame(pCtx);			
			}
			while (fContinue);
		}

		static void __stdcall CollectStackFrames(void* arglist)
		{
			TakeSnapshotCallback* This = static_cast<TakeSnapshotCallback*>(arglist);
			DWORD osThreadId;	
			pPrfCom->m_pInfo->GetThreadInfo(This->threadID, &osThreadId);
			HANDLE hThreadTarget = OpenThread(THREAD_ALL_ACCESS, FALSE, (DWORD)osThreadId);
			DWORD dwRet = SuspendThread(hThreadTarget);
			_ASSERTE(dwRet != (DWORD) -1);

			CONTEXT ctx;
			ZeroMemory(&ctx, sizeof(ctx));
			ctx.ContextFlags = CONTEXT_FULL | CONTEXT_EXCEPTION_REQUEST;

			if (!GetThreadContext(hThreadTarget, &ctx))
				_ASSERTE(!"GetThreadContext failed");

			_ASSERTE(ctx.ContextFlags & CONTEXT_EXCEPTION_REPORTING);        
			
			This->WalkStack(&ctx);

			dwRet = ResumeThread(hThreadTarget);			
			_ASSERTE(dwRet != (DWORD) -1);	
			This->bWalkFrameDone = TRUE;
		}		
#endif		
    private:

        IPrfCom* prfCom;
        StackFrames_t& stackFames;
		ThreadID threadID;		
    };
}

void ExceptionInFlight::takeCallstack(StackFrames_t& collection) const
{
    ThreadID tid;
    MUST_PASS( pPrfCom->m_pInfo->GetCurrentThreadID(&tid) );

    TakeSnapshotCallback tsc(pPrfCom, collection, tid);	
#ifndef _ARM_
    MUST_PASS( pPrfCom->m_pInfo->DoStackSnapshot(tid,
                                                 TakeSnapshotCallback::MySnapshotCallback,
                                                 COR_PRF_SNAPSHOT_DEFAULT,
                                                 &tsc,
                                                 NULL,
                                                 NULL) );
#else
	HANDLE thread_ = reinterpret_cast<HANDLE>( _beginthread( tsc.CollectStackFrames, 0, &tsc) );
	
	//This is not the right way to do it. Ideally ::WaitForSingleObject must be used but WalkFrame is not able to handle anonymous functions on the callstack in the target thread.
	//::WaitForSingleObject( thread_, INFINITE);	
	tsc.bWalkFrameDone = FALSE;
	while(!tsc.bWalkFrameDone)
	{}
#endif
}



////////////////////////////////////////////////////////////////////////////////
//
// Real callbacks
//
////////////////////////////////////////////////////////////////////////////////

HRESULT ex_ThreadCreated(IPrfCom *pPrfCom, ThreadID managedThreadId)
{
    wcout << "thread created: " << managedThreadId << endl;
    HRESULT hr = S_OK;
    ExceptionsOnThread::register_thread(managedThreadId);
    return hr;
}

HRESULT ex_ThreadDestroyed(IPrfCom *pPrfCom, ThreadID managedThreadId)
{
    wcout << "thread destroyed: " << managedThreadId << endl;
    HRESULT hr = S_OK;
    ExceptionsOnThread::unregister_thread(managedThreadId);
    return hr;
}

HRESULT ex_ExceptionThrown(IPrfCom *pPrfCom, ObjectID thrownObjectId)
{
    ThreadID tid;
    MUST_PASS( pPrfCom->m_pInfo->GetCurrentThreadID(&tid) );

    StackFrames_t sf;
#ifndef _ARM_
    MUST_PASS( pPrfCom->m_pInfo->DoStackSnapshot(tid,
                                                 TakeSnapshotCallback::MySnapshotCallback,
                                                 COR_PRF_SNAPSHOT_DEFAULT,
                                                 &TakeSnapshotCallback(pPrfCom,sf,tid),
                                                 NULL,
                                                 NULL) );
#else
	TakeSnapshotCallback tsc(pPrfCom, sf, tid);
	HANDLE thread_ = reinterpret_cast<HANDLE>( _beginthread( tsc.CollectStackFrames, 0, &tsc) );
	
	//This is not the right way to do it. Ideally ::WaitForSingleObject must be used but WalkFrame is not able to handle anonymous functions on the callstack in the target thread.
	//::WaitForSingleObject( thread_, INFINITE);	
	tsc.bWalkFrameDone = FALSE;
	while(!tsc.bWalkFrameDone)
	{}
#endif
    ClassID classId;
    MUST_PASS( pPrfCom->m_pInfo->GetClassFromObject(thrownObjectId, &classId) );
    WCHAR_STR( bh );
    pPrfCom->GetClassIDName(classId, bh);

    DISPLAY(L"EXCEPTION OCCURED: " << thrownObjectId<< L" " << bh);
    for( StackFrames_t::const_iterator i = sf.begin(); i!=sf.end(); ++i )
    {
        DISPLAY(L" at " << i->functionName().c_str() << L" (" << i->funcId << L") <nativeIPoffset:" << i->ip <<L")");
    }

    ExceptionsOnThread::thread_instance().exceptionThrown(thrownObjectId);
    return S_OK;
}

HRESULT ex_ExceptionSearchFunctionEnter(IPrfCom *pPrfCom, FunctionID functionId)
{
    ExceptionsOnThread::thread_instance().exceptionSearchFunctionEnter(functionId);
    return S_OK;
}

HRESULT ex_ExceptionSearchFunctionLeave(IPrfCom *pPrfCom)
{
    ExceptionsOnThread::thread_instance().exceptionSearchFunctionLeave();
    return S_OK;
}

HRESULT ex_ExceptionSearchFilterEnter(IPrfCom *pPrfCom, FunctionID functionId)
{
    ExceptionsOnThread::thread_instance().exceptionSearchFilterEnter(functionId);
    return S_OK;
}

HRESULT ex_ExceptionSearchFilterLeave(IPrfCom *pPrfCom)
{
    ExceptionsOnThread::thread_instance().exceptionSearchFilterLeave();
    return S_OK;
}

HRESULT ex_ExceptionSearchCatcherFound(IPrfCom *pPrfCom, FunctionID functionId)
{
    ExceptionsOnThread::thread_instance().exceptionSearchCatcherFound(functionId);
    return S_OK;
}

HRESULT ex_ExceptionOSHandlerEnter(IPrfCom *pPrfCom, UINT_PTR __unused)
{
    FAILURE(L"Invocation of deprecated function ExceptionOSHandlerEnter");
    return S_OK;
}

HRESULT ex_ExceptionOSHandlerLeave(IPrfCom *pPrfCom, UINT_PTR __unused)
{
    FAILURE(L"Invocation of deprecated function ExceptionOSHandlerLeave");
    return S_OK;
}

HRESULT ex_ExceptionUnwindFunctionEnter(IPrfCom *pPrfCom, FunctionID functionId)
{
    ExceptionsOnThread::thread_instance().exceptionUnwindFunctionEnter(functionId);
    return S_OK;
}

HRESULT ex_ExceptionUnwindFUnctionLeave(IPrfCom *pPrfCom)
{
    ExceptionsOnThread::thread_instance().exceptionUnwindFunctionLeave();
    return S_OK;
}

HRESULT ex_ExceptionUnwindFinallyEnter(IPrfCom *pPrfCom, FunctionID functionId)
{
    ExceptionsOnThread::thread_instance().exceptionUnwindFinallyEnter(functionId);
    return S_OK;
}

HRESULT ex_ExceptionUnwindFinallyLeave(IPrfCom *pPrfCom)
{
    ExceptionsOnThread::thread_instance().exceptionUnwindFinallyLeave();
    return S_OK;
}

HRESULT ex_ExceptionCatcherEnter(IPrfCom *pPrfCom, FunctionID functionId, ObjectID objectId)
{
    ExceptionsOnThread::thread_instance().exceptionCatcherEnter(functionId,objectId);
    return S_OK;
}

HRESULT ex_ExceptionCatcherLeave(IPrfCom *pPrfCom)
{
    ExceptionsOnThread::thread_instance().exceptionCatcherLeave();
    return S_OK;
}

HRESULT ex_ExceptionCLRCatcherFound(IPrfCom *pPrfCom)
{
    FAILURE(L"Invocation of deprecated function ExceptionCLRCatcherFound");
    return S_OK;
}

HRESULT ex_ExceptionCLRCatcherExecute(IPrfCom *pPrfCom)
{
    FAILURE(L"Invocation of deprecated function ExceptionCLRCatcherExecuted");
    return S_OK;
}


HRESULT ex_Verify(IPrfCom *pPrfCom)
{
    ExceptionsOnThread::verifyNoActiveExceptionOnAnyThread();
    return S_OK;
}

bool ExceptionsOnThread::profileASP;

void ex_Initialize (IPrfCom *pPrfCom, PMODULEMETHODTABLE pModuleMethodTable, BOOL testASP)
{
    // save globally so that I can use FAILURE macros outside of callback functions
    ::pPrfCom = pPrfCom;

    DISPLAY(L"Initialize Exception extension");

    // Keep track if we are profiling ASP
    if (testASP ==TRUE)
            ExceptionsOnThread::profileASP = true;

    pModuleMethodTable->FLAGS = (COR_PRF_MONITOR_ALL | COR_PRF_DISABLE_INLINING | COR_PRF_ENABLE_STACK_SNAPSHOT |
                                 COR_PRF_ENABLE_FRAME_INFO | COR_PRF_MONITOR_THREADS)
                            & ~(COR_PRF_MONITOR_REMOTING_COOKIE | COR_PRF_MONITOR_REMOTING_ASYNC | COR_PRF_MONITOR_GC | COR_PRF_ENABLE_REJIT)
                                       |     COR_PRF_MONITOR_REMOTING ;

    REGISTER_CALLBACK(VERIFY,                       ex_Verify);
    REGISTER_CALLBACK(THREADCREATED,                ex_ThreadCreated);
    REGISTER_CALLBACK(THREADDESTROYED,              ex_ThreadDestroyed);
    REGISTER_CALLBACK(EXCEPTIONTHROWN,              ex_ExceptionThrown);
    REGISTER_CALLBACK(EXCEPTIONSEARCHFUNCTIONENTER, ex_ExceptionSearchFunctionEnter);
    REGISTER_CALLBACK(EXCEPTIONSEARCHFUNCTIONLEAVE, ex_ExceptionSearchFunctionLeave);
    REGISTER_CALLBACK(EXCEPTIONSEARCHFILTERENTER,   ex_ExceptionSearchFilterEnter);
    REGISTER_CALLBACK(EXCEPTIONSEARCHFILTERLEAVE,   ex_ExceptionSearchFilterLeave);
    REGISTER_CALLBACK(EXCEPTIONSEARCHCATCHERFOUND,  ex_ExceptionSearchCatcherFound);
    REGISTER_CALLBACK(EXCEPTIONOSHANDLERENTER,      ex_ExceptionOSHandlerEnter);
    REGISTER_CALLBACK(EXCEPTIONOSHANDLERLEAVE,      ex_ExceptionOSHandlerLeave);
    REGISTER_CALLBACK(EXCEPTIONUNWINDFUNCTIONENTER, ex_ExceptionUnwindFunctionEnter);
    REGISTER_CALLBACK(EXCEPTIONUNWINDFUNCTIONLEAVE, ex_ExceptionUnwindFUnctionLeave);
    REGISTER_CALLBACK(EXCEPTIONUNWINDFINALLYENTER,  ex_ExceptionUnwindFinallyEnter);
    REGISTER_CALLBACK(EXCEPTIONUNWINDFINALLYLEAVE,  ex_ExceptionUnwindFinallyLeave);
    REGISTER_CALLBACK(EXCEPTIONCATCHERENTER,        ex_ExceptionCatcherEnter);
    REGISTER_CALLBACK(EXCEPTIONCATCHERLEAVE,        ex_ExceptionCatcherLeave);
    REGISTER_CALLBACK(EXCEPTIONCLRCATCHERFOUND,     ex_ExceptionCLRCatcherFound);
    REGISTER_CALLBACK(EXCEPTIONCLRCATCHEREXECUTE,   ex_ExceptionCLRCatcherExecute);
    return;
}

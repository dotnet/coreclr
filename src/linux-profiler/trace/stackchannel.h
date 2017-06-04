#ifndef _STACK_CHANNEL_H_
#define _STACK_CHANNEL_H_

#include <mutex>
#include <vector>
#include <utility>
#include <atomic>

#include "functioninfo.h"
#include "ringbuffer.h"

struct SampleInfo
{
    unsigned long  ticks;
    size_t         count;
};

struct Frame
{
    InternalID functionIid;
    UINT_PTR ip;
    // TODO: other useful stuff.
};

class StackTraceDiff
{
    friend class StackChannel;

private:
    typedef std::vector<Frame> Stack;

public:
    StackTraceDiff() = default;

    ~StackTraceDiff() = default;

    Stack::const_iterator begin() const noexcept
    {
        return m_begin;
    }

    Stack::const_iterator end() const noexcept
    {
        return m_end;
    }

    Stack::difference_type MatchPrefixSize() const noexcept
    {
        return m_matchPrefixSize;
    }

    Stack::size_type StackSize() const noexcept
    {
        return m_stackSize;
    }

    UINT_PTR IP() const noexcept
    {
        return m_ip;
    }

private:
    Stack::difference_type m_matchPrefixSize;
    Stack::size_type       m_stackSize;
    UINT_PTR               m_ip;
    Stack::iterator        m_begin;
    Stack::iterator        m_end;
};

enum class ChanCanRealloc
{
    NO,
    YES
};

class StackChannel
{
private:
    enum class StackAction
    {
        PUSH,
        POP,
        CHIP,
        SAMPLE,
    };

    struct StackEvent
    {
        StackAction action;
        union
        {
            struct {
                UINT_PTR ip;
                InternalID functionIid;
            } ofPushAction;
            struct {
                size_t count;
            } ofPopAction;
            struct {
                UINT_PTR ip;
            } ofChipAction;
            struct {
                SampleInfo info;
            } ofSampleAction;
        } data;

        static StackEvent CreateNopAction() noexcept;

        static StackEvent CreatePushAction(InternalID functionIid) noexcept;

        static StackEvent CreatePopAction(size_t count) noexcept;

        static StackEvent CreateChIPAction(UINT_PTR ip) noexcept;

        static StackEvent CreateSampleAction(SampleInfo info) noexcept;

        bool IsNopAction() noexcept;

        bool IsPushAction() noexcept;

        bool IsPopAction() noexcept;

        bool IsChIPAction() noexcept;

        bool IsSampleAction() noexcept;
    };

    typedef StackTraceDiff::Stack Stack;

public:
    StackChannel();

    ~StackChannel();

private:
    void IncreaseBufferCapacity();

    bool EnsureBufferCapacity(
        ChanCanRealloc canRealloc = ChanCanRealloc::YES,
        size_t count = 1);

public:
    //
    // Writer methods.
    //

    void Push(const FunctionInfo &funcInfo) noexcept;

    void Pop() noexcept;

    bool ChIP(
        UINT_PTR ip,
        ChanCanRealloc canRealloc = ChanCanRealloc::YES) noexcept;

    bool Sample(
        unsigned long ticks, size_t count = 1,
        ChanCanRealloc canRealloc = ChanCanRealloc::YES) noexcept;

    void PlanToIncreaseBufferCapacity() noexcept;

    size_t GetStackSize() const noexcept;

    const FunctionInfo &Top() const noexcept;

    //
    // Reader methods.
    //

    size_t GetSampleInfoCount() const noexcept;

    // StackTraceDiff only valid until next call to GetNextSampleInfo().
    std::pair<SampleInfo, StackTraceDiff> GetNextSampleInfo() noexcept;

private:
    std::vector<std::reference_wrapper<const FunctionInfo>> m_writerStack;
    ring_buffer<StackEvent> m_buffer;
    std::mutex m_mutex;
    bool m_bufferCapacityIncreaseIsPlanned;

    StackAction m_firstActionInSample;
    size_t      m_pushActionCountInCurrentSample;

    Stack m_readerStack;
    std::atomic_size_t m_sampleInfoCount;
};

#endif // _STACK_CHANNEL_H_

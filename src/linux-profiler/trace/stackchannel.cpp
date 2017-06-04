#include <assert.h>

#include "stackchannel.h"

#define STACK_CHANNEL_START_CAP 128 // Should be power of 2.

// static
StackChannel::StackEvent
StackChannel::StackEvent::CreateNopAction() noexcept
{
    StackEvent event = {StackAction::POP};
    event.data.ofPopAction = {0};
    return event;
}

// static
StackChannel::StackEvent
StackChannel::StackEvent::CreatePushAction(InternalID functionIid) noexcept
{
    StackEvent event = {StackAction::PUSH};
    event.data.ofPushAction = {0, functionIid};
    return event;
}

// static
StackChannel::StackEvent
StackChannel::StackEvent::CreatePopAction(size_t count) noexcept
{
    StackEvent event = {StackAction::POP};
    event.data.ofPopAction = {count};
    return event;
}

// static
StackChannel::StackEvent
StackChannel::StackEvent::CreateChIPAction(UINT_PTR ip) noexcept
{
    StackEvent event = {StackAction::CHIP};
    event.data.ofChipAction = {ip};
    return event;
}

// static
StackChannel::StackEvent
StackChannel::StackEvent::CreateSampleAction(SampleInfo info) noexcept
{
    StackEvent event = {StackAction::SAMPLE};
    event.data.ofSampleAction = {info};
    return event;
}

bool StackChannel::StackEvent::IsNopAction() noexcept
{
    return action == StackAction::POP && data.ofPopAction.count == 0;
}

bool StackChannel::StackEvent::IsPushAction() noexcept
{
    return action == StackAction::PUSH;
}

bool StackChannel::StackEvent::IsPopAction() noexcept
{
    return action == StackAction::POP;
}

bool StackChannel::StackEvent::IsChIPAction() noexcept
{
    return action == StackAction::CHIP;
}

bool StackChannel::StackEvent::IsSampleAction() noexcept
{
    return action == StackAction::SAMPLE;
}

StackChannel::StackChannel()
    : m_buffer(STACK_CHANNEL_START_CAP)
    , m_mutex()
    , m_bufferCapacityIncreaseIsPlanned(false)
    , m_firstActionInSample(StackAction::POP)
    , m_pushActionCountInCurrentSample(0)
    , m_readerStack()
    , m_sampleInfoCount(0)
{
    // NOTE: m_buffer never should be empty.
    m_buffer.push_back(StackEvent::CreateNopAction());
}

StackChannel::~StackChannel()
{
}

void StackChannel::IncreaseBufferCapacity()
{
    m_bufferCapacityIncreaseIsPlanned = false;
    m_buffer.reserve(m_buffer.capacity() * 2); // Always power of two.
}

bool StackChannel::EnsureBufferCapacity(ChanCanRealloc canRealloc, size_t count)
{
    bool isNoSpace = (m_buffer.capacity() - m_buffer.size()) < count;
    switch (canRealloc)
    {
    case ChanCanRealloc::NO:
        return !isNoSpace;

    case ChanCanRealloc::YES:
        if (isNoSpace || m_bufferCapacityIncreaseIsPlanned)
        {
            std::lock_guard<decltype(m_mutex)> lock(m_mutex);
            this->IncreaseBufferCapacity();
        }
        assert(m_buffer.capacity() - m_buffer.size() >= count);
        return true;
    }
}

void StackChannel::Push(const FunctionInfo &funcInfo) noexcept
{
    // XXX: exception in this call will terminate process!

    m_writerStack.push_back(std::cref(funcInfo));

    // Try to optimize PUSH action.
    // NOTE: race condition with GetNextSampleInfo() is possible, but this
    // operations are valid:
    //   1) m_buffer.back() always exists. It is POP if all other output
    //      is handled by GetNextSampleInfo();
    //   2) GetNextSampleInfo() should not be called when there is no SAMPLE
    //      elements in buffer.
    StackEvent &lastEvent = m_buffer.back();
    StackEvent event = StackEvent::CreatePushAction(funcInfo.internalId);
    if (lastEvent.IsNopAction())
    {
        // Replace POP(0) with PUSH.
        lastEvent = event;
        m_firstActionInSample = StackAction::PUSH;
    }
    else if (lastEvent.IsSampleAction())
    {
        assert(
            !"lastEvent should never be SAMPLE because "
            "POP(0) should be used after SAMPLE"
        );
    }
    else
    {
        // Unoptimazed action performing.
        this->EnsureBufferCapacity();
        m_buffer.push_back(event); // We can do it concurently with pop_front().
    }
    ++m_pushActionCountInCurrentSample;
}

void StackChannel::Pop() noexcept
{
    // XXX: exception in this call will terminate process!

    assert(!m_writerStack.empty());
    m_writerStack.pop_back();

    // Optimize POP action.
    // NOTE: race condition with GetNextSampleInfo() is possible, but this
    // operations are valid:
    //   1) m_buffer.back() always exists. It is POP if all other output
    //      is handled by GetNextSampleInfo();
    //   2) GetNextSampleInfo() should not be called when there is no SAMPLE
    //      elements in buffer.
    StackEvent &lastEvent = m_buffer.back();
    if (lastEvent.IsPushAction())
    {
        // If PUSH is first action in sample it can only be replaced with
        // POP(0) to save optimization invariant.
        if (m_pushActionCountInCurrentSample == 1 &&
            m_firstActionInSample == StackAction::PUSH)
        {
            lastEvent = StackEvent::CreateNopAction();
            m_firstActionInSample = StackAction::POP;
        }
        else
        {
            m_buffer.pop_back();
        }
        --m_pushActionCountInCurrentSample;
    }
    else if (lastEvent.IsPopAction())
    {
        ++lastEvent.data.ofPopAction.count;
    }
    else if (lastEvent.IsChIPAction())
    {
        // CHIP can be first action or second after POP.
        if (m_firstActionInSample == StackAction::CHIP)
        {
            // At first case we replace CHIP with POP.
            lastEvent = StackEvent::CreatePopAction(1);
            m_firstActionInSample = StackAction::POP;
        }
        else
        {
            // At second case we remove CHIP and increment POP count.
            assert(m_firstActionInSample == StackAction::POP);
            m_buffer.pop_back();
            assert(m_buffer.back().IsPopAction());
            ++m_buffer.back().data.ofPopAction.count;
        }
    }
    else if (lastEvent.IsSampleAction())
    {
        assert(
            !"lastEvent should never be SAMPLE because "
            "POP(0) should be used after SAMPLE"
        );
    }
}

bool StackChannel::ChIP(UINT_PTR ip, ChanCanRealloc canRealloc) noexcept
{
    // XXX: exception in this call will terminate process!

    assert(!m_writerStack.empty());

    // Try to optimize CHIP action.
    // NOTE: race condition with GetNextSampleInfo() is possible, but this
    // operations are valid:
    //   1) m_buffer.back() always exists. It is POP if all other output
    //      is handled by GetNextSampleInfo();
    //   2) GetNextSampleInfo() should not be called when there is no SAMPLE
    //      elements in buffer.
    StackEvent &lastEvent = m_buffer.back();

    if (lastEvent.IsNopAction())
    {
        // Replace POP(0) with CHIP.
        lastEvent = StackEvent::CreateChIPAction(ip);
        m_firstActionInSample = StackAction::CHIP;
    }
    else if (lastEvent.IsPushAction())
    {
        lastEvent.data.ofPushAction.ip = ip;
    }
    else if (lastEvent.IsChIPAction())
    {
        lastEvent.data.ofChipAction.ip = ip;
    }
    else if (lastEvent.IsSampleAction())
    {
        assert(
            !"lastEvent should never be SAMPLE because "
            "POP(0) should be used after SAMPLE"
        );
    }
    else
    {
        // Unoptimazed action performing.
        if (!this->EnsureBufferCapacity(canRealloc))
        {
            // No space for new CHIP.
            return false;
        }
        // We can do it concurently with pop_front().
        m_buffer.push_back(StackEvent::CreateChIPAction(ip));
    }
    return true;
}

bool StackChannel::Sample(
    unsigned long ticks, size_t count, ChanCanRealloc canRealloc) noexcept
{
    // XXX: exception in this call will terminate process!

    // Try to optimize SAMPLE action.
    // NOTE: race condition with GetNextSampleInfo() is possible, but this
    // operations are valid:
    //   1) m_buffer.back() always exists. It is POP if all other output
    //      is handled by GetNextSampleInfo();
    //   2) GetNextSampleInfo() should not be called when there is no SAMPLE
    //      elements in buffer.
    StackEvent &lastEvent = m_buffer.back();
    StackEvent event = StackEvent::CreateSampleAction({ticks, count});
    if (lastEvent.IsNopAction())
    {
        // Replace old POP(0) with SAMPLE.
        if (!this->EnsureBufferCapacity(canRealloc, 1))
        {
            // No space for new POP(0).
            return false;
        }
        lastEvent = event;
    }
    else if (lastEvent.IsSampleAction())
    {
        assert(
            !"lastEvent should never be SAMPLE because "
            "POP(0) should be used after SAMPLE"
        );
    }
    else
    {
        // Unoptimazed action performing.
        if (!this->EnsureBufferCapacity(canRealloc, 2))
        {
            // No space for SAMPLE and new POP(0).
            return false;
        }
        m_buffer.push_back(event); // We can do it concurently with pop_front().
    }

    // NOTE: SAMPLE is always succeeded by POP(0).
    // We can do it concurently with pop_front().
    m_buffer.push_back(StackEvent::CreateNopAction());

    // Reset optimization invariant.
    m_firstActionInSample = StackAction::POP;
    m_pushActionCountInCurrentSample = 0;

    m_sampleInfoCount++;
    return true;
}

void StackChannel::PlanToIncreaseBufferCapacity() noexcept
{
    m_bufferCapacityIncreaseIsPlanned = true;
}

size_t StackChannel::GetStackSize() const noexcept
{
    return m_writerStack.size();
}

const FunctionInfo &StackChannel::Top() const noexcept
{
    assert(!m_writerStack.empty());
    return m_writerStack.back();
}

size_t StackChannel::GetSampleInfoCount() const noexcept
{
    return m_sampleInfoCount;
}

// TODO: We can return SampleInfo and StackTraceDiff in zero-copy manner
// with using of wrapped iterators for Ring Buffer. After sample processing
// separate method can be called to remove current sample information from
// Channel.
std::pair<SampleInfo, StackTraceDiff> StackChannel::GetNextSampleInfo() noexcept
{
    // XXX: exception in this call will terminate process!

    // NOTE: should be true for Writer methods optimizatiopn.
    assert(m_sampleInfoCount > 0);

    SampleInfo     info = {};
    StackTraceDiff diff = {};
    diff.m_matchPrefixSize = diff.m_stackSize = m_readerStack.size();

    std::lock_guard<decltype(m_mutex)> lock(m_mutex);
    while (!m_buffer.empty())
    {
        assert(diff.m_matchPrefixSize <= m_readerStack.size());

        StackEvent event = m_buffer.front();
        m_buffer.pop_front();

        switch (event.action)
        {
        case StackAction::PUSH:
            m_readerStack.push_back({
                event.data.ofPushAction.functionIid,
                event.data.ofPushAction.ip
            });
            continue;

        case StackAction::POP:
            assert(event.data.ofPopAction.count != 0);
            assert(event.data.ofPopAction.count <= m_readerStack.size());
            m_readerStack.erase(
                m_readerStack.end() - event.data.ofPopAction.count,
                m_readerStack.end());
            if (m_readerStack.size() < diff.m_matchPrefixSize)
            {
                diff.m_matchPrefixSize = m_readerStack.size();
            }
            continue;

        case StackAction::CHIP:
            assert(!m_readerStack.empty());
            assert(diff.m_matchPrefixSize == m_readerStack.size());
            m_readerStack.back().ip = diff.m_ip = event.data.ofChipAction.ip;
            continue;

        case StackAction::SAMPLE:
            info = event.data.ofSampleAction.info;
            m_sampleInfoCount--;
            break; // NOTE: end of loop!

        default:
            assert(!"The default case should not be reached.");
        }

        break;
    }

    diff.m_begin = m_readerStack.begin() + diff.m_matchPrefixSize;
    diff.m_end   = m_readerStack.end();

    return std::make_pair(info, diff);
}

#ifndef _LOG_H_
#define _LOG_H_

#include <iostream>
#include <fstream>

enum class LogLevel
{
    None,
    Fatal,
    Error,
    Warn,
    Info,
    Debug,
    Trace,
    All
};

static inline const char *LogLevelName(LogLevel level)
{
    switch (level)
    {
    case LogLevel::None:
        return "NONE";
    case LogLevel::Fatal:
        return "FATAL";
    case LogLevel::Error:
        return "ERROR";
    case LogLevel::Warn:
        return "WARN";
    case LogLevel::Info:
        return "INFO";
    case LogLevel::Debug:
        return "DEBUG";
    case LogLevel::Trace:
        return "TRACE";
    case LogLevel::All:
        return "ALL";
    default:
        return "";
    }
}

#ifdef _DEBUG

#include <utility>

class Log
{
private:
    class LogLine
    {
    public:
        LogLine(LogLevel level, std::ostream *stream = nullptr)
            : m_stream(stream)
        {
            if (m_stream)
            {
                *m_stream << "[" << LogLevelName(level) << "]\t";
            }
        }

        LogLine(const LogLine&) = delete;

        LogLine &operator=(const LogLine&) = delete;

        LogLine(LogLine &&other)
        {
            m_stream = other.m_stream;
            other.m_stream = nullptr;
        }

        LogLine &operator=(LogLine&&) = delete;

        ~LogLine()
        {
            if (m_stream)
            {
                *m_stream << std::endl;
            }
        }

        template<typename T>
        LogLine &operator<<(T value)
        {
            if (m_stream)
            {
                *m_stream << value;
            }
            return *this;
        }

    private:
        std::ostream *m_stream;
    };

public:
    Log(LogLevel level, const std::string &filename)
        : m_level(level)
        , m_stream(new std::ofstream())
        , m_stream_owner(true)
    {
        try
        {
            m_stream->exceptions(m_stream->exceptions() | std::ios::failbit);
            static_cast<std::ofstream*>(m_stream)->open(filename);
        }
        catch (...)
        {
            delete m_stream;
            throw;
        }
    }

    explicit Log(const std::string &filename)
        : Log(LogLevel::Warn, filename)
    {}

    Log(LogLevel level, std::ostream &stream)
        : m_level(level)
        , m_stream(&stream)
        , m_stream_owner(false)
    {}

    explicit Log(LogLevel level)
        : Log(level, std::cout)
    {}

    explicit Log(std::ostream &stream)
        : Log(LogLevel::Warn, stream)
    {}

    Log()
        : Log(LogLevel::Warn, std::cout)
    {}

    Log(const Log&) = delete;

    Log &operator=(const Log&) = delete;

    Log(Log &&other)
    {
        m_level        = other.m_level;
        m_stream       = other.m_stream;
        m_stream_owner = other.m_stream_owner;

        other.m_level        = LogLevel::None;
        other.m_stream       = nullptr;
        other.m_stream_owner = false;
    }

    Log &operator=(Log &&other)
    {
        Log(std::move(other)).swap(*this);
        return *this;
    }

    ~Log()
    {
        if (m_stream_owner)
        {
            delete m_stream;
        }
    }

    void swap(Log &other)
    {
        std::swap(m_level,        other.m_level);
        std::swap(m_stream,       other.m_stream);
        std::swap(m_stream_owner, other.m_stream_owner);
    }

    std::ostream::iostate exceptions() const
    {
        return m_stream->exceptions();
    }

    void exceptions(std::ostream::iostate except)
    {
        m_stream->exceptions(except);
    }

    LogLine Fatal()
    {
        return DoLog<LogLevel::Fatal>();
    }

    LogLine Error()
    {
        return DoLog<LogLevel::Error>();
    }

    LogLine Warn()
    {
        return DoLog<LogLevel::Warn>();
    }

    LogLine Info()
    {
        return DoLog<LogLevel::Info>();
    }

    LogLine Debug()
    {
        return DoLog<LogLevel::Debug>();
    }

    LogLine Trace()
    {
        return DoLog<LogLevel::Trace>();
    }

private:
    LogLevel      m_level;
    std::ostream *m_stream;
    bool          m_stream_owner;

    template<LogLevel L>
    LogLine DoLog()
    {
        // With RVO optimization LogLine destructor will be called only once.
        // Otherwise with overloaded move constructor only last destructor call
        // will print std::endl.
        if (m_level >= L)
        {
            return LogLine(L, m_stream);
        }
        else
        {
            return LogLine(L);
        }
    }
};

#else // !_DEBUG

class Log
{
private:
    class LogLine
    {
    public:
        LogLine() {}

        LogLine(const LogLine&) = delete;

        LogLine &operator=(const LogLine&) = delete;

        LogLine(LogLine&&) = default;

        LogLine &operator=(LogLine&&) = delete;

        template<typename T>
        LogLine &operator<<(T value)
        {
            return *this;
        }
    };

public:
    Log(LogLevel, const std::string&) {}

    explicit Log(const std::string&) {}

    Log(LogLevel, std::ostream&) {}

    explicit Log(LogLevel) {}

    explicit Log(std::ostream&) {}

    Log() {}

    Log(const Log&) = delete;

    Log &operator=(const Log&) = delete;

    Log(Log&&) = default;

    Log &operator=(Log&&) = default;

    void swap(Log &other) {}

    std::ostream::iostate exceptions() const
    {
        return std::ostream::goodbit;
    }

    void exceptions(std::ostream::iostate except) {}

    LogLine Fatal()
    {
        return LogLine();
    }

    LogLine Error()
    {
        return LogLine();
    }

    LogLine Warn()
    {
        return LogLine();
    }

    LogLine Info()
    {
        return LogLine();
    }

    LogLine Debug()
    {
        return LogLine();
    }

    LogLine Trace()
    {
        return LogLine();
    }
};

#endif // !_DEBUG

#endif // _LOG_H_

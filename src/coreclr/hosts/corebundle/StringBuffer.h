// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef STRING_BUFFER_H
#define STRING_BUFFER_H
// Dynamically expanding string buffer to hold TPA list

class StringBuffer {
    static const int m_defaultSize = 4096;
    char* m_buffer;
    size_t m_capacity;
    size_t m_length;

public:
    StringBuffer() : m_capacity(0), m_buffer(nullptr), m_length(0) {
    }

    ~StringBuffer() {
        delete[] m_buffer;
    }

    const char* CStr() const {
        return m_buffer;
    }

    void Append(const char* str, size_t strLen) {
        if (!m_buffer) {
            m_buffer = new char[m_defaultSize];
            m_capacity = m_defaultSize;
        }
        if (m_length + strLen + 1 > m_capacity) {
            size_t newCapacity = (m_length + strLen + 1) * 2;
            char* newBuffer = new char[newCapacity];
            strncpy_s(newBuffer, newCapacity, m_buffer, m_length);
            delete[] m_buffer;
            m_buffer = newBuffer;
            m_capacity = newCapacity;
        }
        strncpy_s(m_buffer + m_length, m_capacity - m_length, str, strLen);
        m_length += strLen;
    }
};

#endif // STRING_BUFFER_H

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "createdump.h"

DumpWriter::DumpWriter(DataTarget& dataTarget, CrashInfo& crashInfo) :
    m_ref(1),
    m_fd(-1),
    m_dataTarget(dataTarget),
    m_crashInfo(crashInfo)
{
    m_dataTarget.AddRef();
    m_crashInfo.AddRef();
}

DumpWriter::~DumpWriter()
{
    if (m_fd != -1)
    {
        close(m_fd);
        m_fd = -1;
    }
    m_dataTarget.Release();
    m_crashInfo.Release();
}

STDMETHODIMP
DumpWriter::QueryInterface(
    ___in REFIID InterfaceId,
    ___out PVOID* Interface)
{
    if (InterfaceId == IID_IUnknown)
    {
        *Interface = (IUnknown*)this;
        AddRef();
        return S_OK;
    }
    else
    {
        *Interface = NULL;
        return E_NOINTERFACE;
    }
}

STDMETHODIMP_(ULONG)
DumpWriter::AddRef()
{
    LONG ref = InterlockedIncrement(&m_ref);    
    return ref;
}

STDMETHODIMP_(ULONG)
DumpWriter::Release()
{
    LONG ref = InterlockedDecrement(&m_ref);
    if (ref == 0)
    {
        delete this;
    }
    return ref;
}

bool
DumpWriter::OpenDump(char* dumpFileName)
{
    m_fd = open(dumpFileName, O_WRONLY|O_CREAT|O_TRUNC, 0664);
    if (m_fd == -1)
    {
        fprintf(stderr, "Could not open output %s: %s\n", dumpFileName, strerror(errno));
        return false;
    }
    return true;
}

// Write the core dump file:
//   ELF header
//   Single section header (Shdr) for 64 bit program header count
//   Phdr for the PT_NOTE
//   PT_LOAD
//   PT_NOTEs
//      process info (prpsinfo_t)
//      NT_FILE entries
//      threads
//      alignment
//   memory blocks
bool
DumpWriter::WriteDump()
{
    // Write the ELF header
    Ehdr ehdr;
    memset(&ehdr, 0, sizeof(Ehdr));
    ehdr.e_ident[0] = ELFMAG0;
    ehdr.e_ident[1] = ELFMAG1;
    ehdr.e_ident[2] = ELFMAG2;
    ehdr.e_ident[3] = ELFMAG3;
    ehdr.e_ident[4] = ELF_CLASS;

    // Note: The sex is the current system running minidump-2-core
    //       Big or Little endian.  This means you have to create
    //       the core (minidump-2-core) on the system that matches
    //       your intent to debug properly.
    ehdr.e_ident[5] = sex() ? ELFDATA2MSB : ELFDATA2LSB;
    ehdr.e_ident[6] = EV_CURRENT;
    ehdr.e_ident[EI_OSABI] = ELFOSABI_LINUX;

    ehdr.e_type = ET_CORE;
    ehdr.e_machine = ELF_ARCH;
    ehdr.e_version = EV_CURRENT;
    ehdr.e_shoff = sizeof(Ehdr);
    ehdr.e_phoff = sizeof(Ehdr) + sizeof(Shdr);

    ehdr.e_ehsize = sizeof(Ehdr);
    ehdr.e_phentsize = sizeof(Phdr);
    ehdr.e_shentsize = sizeof(Shdr);

    // The ELF header only allows UINT16 for the number of program
    // headers. In a core dump this equates to PT_NODE and PT_LOAD.
    //
    // When more program headers than 65534 the first section entry
    // is used to store the actual program header count.

    // PT_NOTE + number of memory regions
    uint64_t phnum = 1 + m_crashInfo.MemoryRegions().size();

    if (phnum < PH_HDR_CANARY) {
        ehdr.e_phnum = phnum;
    }
    else {
        ehdr.e_phnum = PH_HDR_CANARY;
    }

    if (!WriteData(&ehdr, sizeof(Ehdr))) {
        return false;
    }

    size_t offset = sizeof(Ehdr) + sizeof(Shdr) + (phnum * sizeof(Phdr));
    size_t filesz = GetProcessInfoSize() + GetAuxvInfoSize() + GetThreadInfoSize() + GetNTFileInfoSize();

    // Add single section containing the actual count
    // of the program headers to be written.
    Shdr shdr;
    memset(&shdr, 0, sizeof(shdr));
    shdr.sh_info = phnum;
    // When section header offset is present but ehdr section num = 0
    // then is is expected that the sh_size indicates the size of the
    // section array or 1 in our case.
    shdr.sh_size = 1;
    if (!WriteData(&shdr, sizeof(shdr))) {
        return false;
    }

    // PT_NOTE header
    Phdr phdr;
    memset(&phdr, 0, sizeof(Phdr));
    phdr.p_type = PT_NOTE;
    phdr.p_offset = offset;
    phdr.p_filesz = filesz;

    if (!WriteData(&phdr, sizeof(phdr))) {
        return false;
    }

    // PT_NOTE sections must end on 4 byte boundary
    // We output the NT_FILE, AUX and Thread entries
    // AUX is aligned, NT_FILE is aligned and then we
    // check to pad end of the thread list
    phdr.p_type = PT_LOAD;
    phdr.p_align = 4096;

    size_t finalNoteAlignment = phdr.p_align - ((offset + filesz) % phdr.p_align);
    if (finalNoteAlignment == phdr.p_align) {
        finalNoteAlignment = 0;
    }
    offset += finalNoteAlignment;

    TRACE("Writing memory region headers to core file\n");

    // Write memory region note headers
    for (const MemoryRegion& memoryRegion : m_crashInfo.MemoryRegions())
    {
        phdr.p_flags = memoryRegion.Permissions();
        phdr.p_vaddr = memoryRegion.StartAddress();
        phdr.p_memsz = memoryRegion.Size();

        offset += filesz;
        phdr.p_filesz = filesz = memoryRegion.Size();
        phdr.p_offset = offset;

        if (!WriteData(&phdr, sizeof(phdr))) {
            return false;
        }
    }

    // Write process info data to core file
    if (!WriteProcessInfo()) {
        return false;
    }

    // Write auxv data to core file
    if (!WriteAuxv()) {
        return false;
    }

    // Write NT_FILE entries to the core file
    if (!WriteNTFileInfo()) {
        return false;
    }

    TRACE("Writing %ld thread entries to core file\n", m_crashInfo.Threads().size());

    // Write all the thread's state and registers
    for (const ThreadInfo* thread : m_crashInfo.Threads()) 
    {
        if (!WriteThread(*thread, SIGABRT)) {
            return false;
        }
    }

    // Zero out the end of the PT_NOTE section to the boundary
    // and then laydown the memory blocks
    if (finalNoteAlignment > 0) {
        assert(finalNoteAlignment < sizeof(m_tempBuffer));
        memset(m_tempBuffer, 0, finalNoteAlignment);
        if (!WriteData(m_tempBuffer, finalNoteAlignment)) {
            return false;
        }
    }

    TRACE("Writing %ld memory regions to core file\n", m_crashInfo.MemoryRegions().size());

    // Read from target process and write memory regions to core
    uint64_t total = 0;
    for (const MemoryRegion& memoryRegion : m_crashInfo.MemoryRegions())
    {
        uint32_t size = memoryRegion.Size();
        uint64_t address = memoryRegion.StartAddress();
        total += size;

        while (size > 0)
        {
            uint32_t bytesToRead = std::min(size, (uint32_t)sizeof(m_tempBuffer));
            uint32_t read = 0;

            if (FAILED(m_dataTarget.ReadVirtual(address, m_tempBuffer, bytesToRead, &read))) {
                fprintf(stderr, "ReadVirtual(%016lx, %08x) FAILED\n", address, bytesToRead);
                return false;
            }

            if (!WriteData(m_tempBuffer, read)) {
                return false;
            }

            address += read;
            size -= read;
        }
    }

    printf("Written %ld bytes (%ld pages) to core file\n", total, total >> PAGE_SHIFT);

    return true;
}

bool
DumpWriter::WriteProcessInfo()
{
    prpsinfo_t processInfo;
    memset(&processInfo, 0, sizeof(processInfo));
    processInfo.pr_sname = 'R';
    processInfo.pr_pid = m_crashInfo.Pid();
    processInfo.pr_ppid = m_crashInfo.Ppid();
    processInfo.pr_pgrp = m_crashInfo.Tgid();
    strcpy_s(processInfo.pr_fname, sizeof(processInfo.pr_fname), m_crashInfo.Name());

    Nhdr nhdr;
    memset(&nhdr, 0, sizeof(nhdr));
    nhdr.n_namesz = 5;
    nhdr.n_descsz = sizeof(prpsinfo_t);
    nhdr.n_type = NT_PRPSINFO;

    TRACE("Writing process information to core file\n");

    // Write process info data to core file
    if (!WriteData(&nhdr, sizeof(nhdr)) ||
        !WriteData("CORE\0PRP", 8) ||
        !WriteData(&processInfo, sizeof(prpsinfo_t))) {
        return false;
    }
    return true;
}

bool 
DumpWriter::WriteAuxv()
{
    Nhdr nhdr;
    memset(&nhdr, 0, sizeof(nhdr));
    nhdr.n_namesz = 5;
    nhdr.n_descsz = m_crashInfo.GetAuxvSize();
    nhdr.n_type = NT_AUXV;

    TRACE("Writing %ld auxv entries to core file\n", m_crashInfo.AuxvEntries().size());

    if (!WriteData(&nhdr, sizeof(nhdr)) ||
        !WriteData("CORE\0AUX", 8)) { 
        return false;
    }
    for (const auto& auxvEntry : m_crashInfo.AuxvEntries())
    {
        if (!WriteData(&auxvEntry, sizeof(auxvEntry))) {
            return false;
        }
    }
    return true;
}

struct NTFileEntry
{
    uint64_t StartAddress;
    uint64_t EndAddress;
    uint64_t Offset;
};

// Calculate the NT_FILE entries total size
size_t
DumpWriter::GetNTFileInfoSize(size_t* alignmentBytes)
{
    size_t count = m_crashInfo.ModuleMappings().size();
    size_t size = 0;

    // Header, CORE, entry count, page size
    size = sizeof(Nhdr) + sizeof(NTFileEntry);

    // start_address, end_address, offset
    size += count * sizeof(NTFileEntry);

    // \0 terminator for each filename
    size += count;

    // File name storage needed
    for (const MemoryRegion& image : m_crashInfo.ModuleMappings()) {
        size += strlen(image.FileName());
    }
    // Notes must end on 4 byte alignment
    size_t alignmentBytesNeeded = 4 - (size % 4);
    size += alignmentBytesNeeded;

    if (alignmentBytes != nullptr) {
        *alignmentBytes = alignmentBytesNeeded;
    }
    return size;
}

//  Write NT_FILE entries to the PT_NODE section
//
//  Nhdr (NT_FILE)
//  Total entries
//  Page size
//  [0] start_address end_address offset
//  [1] start_address end_address offset
//  [file name]\0[file name]\0...
bool 
DumpWriter::WriteNTFileInfo()
{
    Nhdr nhdr;
    memset(&nhdr, 0, sizeof(nhdr));

    // CORE + \0 and we align on 4 byte boundary
    // so we can use CORE\0FIL for easier hex debugging
    nhdr.n_namesz = 5;
    nhdr.n_type = NT_FILE;  // "FILE"

    // Size of payload for NT_FILE after CORE tag written
    size_t alignmentBytesNeeded = 0;
    nhdr.n_descsz = GetNTFileInfoSize(&alignmentBytesNeeded) - sizeof(nhdr) - 8;

    size_t count = m_crashInfo.ModuleMappings().size();
    size_t pageSize = PAGE_SIZE;

    TRACE("Writing %ld NT_FILE entries to core file\n", m_crashInfo.ModuleMappings().size());

    if (!WriteData(&nhdr, sizeof(nhdr)) ||
        !WriteData("CORE\0FIL", 8) ||
        !WriteData(&count, 8) ||
        !WriteData(&pageSize, 8)) {
        return false;
    }

    for (const MemoryRegion& image : m_crashInfo.ModuleMappings())
    {
        struct NTFileEntry entry { image.StartAddress(), image.EndAddress(), image.Offset() };
        if (!WriteData(&entry, sizeof(entry))) { 
            return false;
        }
    }

    for (const MemoryRegion& image : m_crashInfo.ModuleMappings())
    {
        if (!WriteData(image.FileName(), strlen(image.FileName())) ||
            !WriteData("\0", 1)) {
            return false;
        }
    }

    // Has to end on a 4 byte boundary.  Debugger, readelf and such
    // will automatically align on next 4 bytes and look for a PT_NOTE
    // header.
    if (alignmentBytesNeeded) {
        if (!WriteData("\0\0\0\0", alignmentBytesNeeded)) {
            return false;
        }
    }

    return true;
}

bool
DumpWriter::WriteThread(const ThreadInfo& thread, int fatal_signal)
{
    prstatus_t pr;
    memset(&pr, 0, sizeof(pr));

    pr.pr_info.si_signo = fatal_signal;
    pr.pr_cursig = fatal_signal;
    pr.pr_pid = thread.Tid();
    pr.pr_ppid = thread.Ppid();
    pr.pr_pgrp = thread.Tgid();
    memcpy(&pr.pr_reg, thread.GPRegisters(), sizeof(user_regs_struct));

    Nhdr nhdr;
    memset(&nhdr, 0, sizeof(nhdr));

    // Name size is CORE plus the NULL terminator
    // The format requires 4 byte alignment so the
    // value written in 8 bytes.  Stuff the last 3
    // bytes with the type of NT_PRSTATUS so it is
    // easier to debug in a hex editor.
    nhdr.n_namesz = 5;
    nhdr.n_descsz = sizeof(prstatus_t);
    nhdr.n_type = NT_PRSTATUS;
    if (!WriteData(&nhdr, sizeof(nhdr)) ||
        !WriteData("CORE\0THR", 8) ||
        !WriteData(&pr, sizeof(prstatus_t))) {
        return false;
    }

#if defined(__i386__) || defined(__x86_64__)
    nhdr.n_descsz = sizeof(user_fpregs_struct);
    nhdr.n_type = NT_FPREGSET;
    if (!WriteData(&nhdr, sizeof(nhdr)) ||
        !WriteData("CORE\0FLT", 8) ||
        !WriteData(thread.FPRegisters(), sizeof(user_fpregs_struct))) {
        return false;
    }
#endif

#if defined(__i386__)
    nhdr.n_descsz = sizeof(user_fpxregs_struct);
    nhdr.n_type = NT_PRXFPREG;
    if (!WriteData(&nhdr, sizeof(nhdr)) ||
        !WriteData("LINUX\0\0\0", 8) ||
        !WriteData(&thread.FPXRegisters(), sizeof(user_fpxregs_struct))) {
        return false;
    }
#endif

    return true;
}

// Write all of the given buffer, handling short writes and EINTR. Return true iff successful.
bool
DumpWriter::WriteData(const void* buffer, size_t length)
{
    const uint8_t* data = (const uint8_t*)buffer;

    size_t done = 0;
    while (done < length) {
        ssize_t written;
        do {
            written = write(m_fd, data + done, length - done);
        } while (written == -1 && errno == EINTR);

        if (written < 1) {
            fprintf(stderr, "WriteData FAILED %s\n", strerror(errno));
            return false;
        }
        done += written;
    }
    return true;
}

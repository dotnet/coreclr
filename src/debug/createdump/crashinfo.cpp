// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "createdump.h"

CrashInfo::CrashInfo(pid_t pid, ICLRDataTarget* dataTarget, bool sos) :
    m_ref(1),
    m_pid(pid),
    m_ppid(-1),
    m_name(nullptr),
    m_sos(sos),
    m_dataTarget(dataTarget)
{
    dataTarget->AddRef();
    m_auxvValues.fill(0);
}

CrashInfo::~CrashInfo()
{
    if (m_name != nullptr)
    {
        free(m_name);
    }
    // Clean up the threads
    for (ThreadInfo* thread : m_threads)
    {
        delete thread;
    }
    m_threads.clear();

    // Module and other mappings have a file name to clean up.
    for (const MemoryRegion& region : m_moduleMappings)
    {
        const_cast<MemoryRegion&>(region).Cleanup();
    }
    m_moduleMappings.clear();
    for (const MemoryRegion& region : m_otherMappings)
    {
        const_cast<MemoryRegion&>(region).Cleanup();
    }
    m_otherMappings.clear();
    m_dataTarget->Release();
}

STDMETHODIMP
CrashInfo::QueryInterface(
    ___in REFIID InterfaceId,
    ___out PVOID* Interface)
{
    if (InterfaceId == IID_IUnknown ||
        InterfaceId == IID_ICLRDataEnumMemoryRegionsCallback)
    {
        *Interface = (ICLRDataEnumMemoryRegionsCallback*)this;
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
CrashInfo::AddRef()
{
    LONG ref = InterlockedIncrement(&m_ref);    
    return ref;
}

STDMETHODIMP_(ULONG)
CrashInfo::Release()
{
    LONG ref = InterlockedDecrement(&m_ref);
    if (ref == 0)
    {
        delete this;
    }
    return ref;
}

HRESULT STDMETHODCALLTYPE
CrashInfo::EnumMemoryRegion( 
    /* [in] */ CLRDATA_ADDRESS address,
    /* [in] */ ULONG32 size)
{
    InsertMemoryRegion(address, size);
    return S_OK;
}

//
// Suspends all the threads and creating a list of them. Should be the first before 
// gather any info about the process.
//
bool 
CrashInfo::EnumerateAndSuspendThreads()
{
    char taskPath[128];
    snprintf(taskPath, sizeof(taskPath), "/proc/%d/task", m_pid);

    DIR* taskDir = opendir(taskPath);
    if (taskDir == nullptr)
    {
        fprintf(stderr, "opendir(%s) FAILED %s\n", taskPath, strerror(errno));
        return false;
    }

    struct dirent* entry;
    while ((entry = readdir(taskDir)) != nullptr)
    {
        pid_t tid = static_cast<pid_t>(strtol(entry->d_name, nullptr, 10));
        if (tid != 0)
        {
            // Don't suspend the threads if running under sos
            if (!m_sos)
            {
                //  Reference: http://stackoverflow.com/questions/18577956/how-to-use-ptrace-to-get-a-consistent-view-of-multiple-threads
                if (ptrace(PTRACE_ATTACH, tid, nullptr, nullptr) != -1)
                {
                    int waitStatus;
                    waitpid(tid, &waitStatus, __WALL);
                }
                else
                {
                    fprintf(stderr, "ptrace(ATTACH, %d) FAILED %s\n", tid, strerror(errno));
                    closedir(taskDir);
                    return false;
                }
            }
            // Add to the list of threads
            ThreadInfo* thread = new ThreadInfo(tid);
            m_threads.push_back(thread);
        }
    }

    closedir(taskDir);
    return true;
}

//
// Gather all the necessary crash dump info.
//
bool
CrashInfo::GatherCrashInfo(const char* programPath, MINIDUMP_TYPE minidumpType)
{
    // Get the process info
    if (!GetStatus(m_pid, &m_ppid, &m_tgid, &m_name))
    {
        return false;
    }
    // Get the info about the threads (registers, etc.)
    for (ThreadInfo* thread : m_threads)
    {
        if (!thread->Initialize(m_sos ? m_dataTarget : nullptr))
        {
            return false;
        }
    }
    // Get the auxv data
    if (!GetAuxvEntries())
    {
        return false;
    }
    // Gather all the module memory mappings (from /dev/$pid/maps)
    if (!EnumerateModuleMappings())
    {
        return false;
    }
    // Get shared module debug info
    if (!GetDSOInfo())
    {
        return false;
    }
    // If full memory dump, include everything regardless of permissions
    if (minidumpType & MiniDumpWithFullMemory)
    {
        for (const MemoryRegion& region : m_moduleMappings)
        {
            InsertMemoryBackedRegion(region);
        }
        for (const MemoryRegion& region : m_otherMappings)
        {
            InsertMemoryBackedRegion(region);
        }
    }
    // Add all the heap (read/write) memory regions (m_otherMappings contains the heaps)
    else if (minidumpType & MiniDumpWithPrivateReadWriteMemory)
    {
        for (const MemoryRegion& region : m_otherMappings)
        {
            if (region.Permissions() == (PF_R | PF_W))
            {
                InsertMemoryBackedRegion(region);
            }
        }
    }
    // Gather all the useful memory regions from the DAC
    if (!EnumerateMemoryRegionsWithDAC(programPath, minidumpType))
    {
        return false;
    }
    if ((minidumpType & MiniDumpWithFullMemory) == 0)
    {
        // Add the thread's stack and some code memory to core
        for (ThreadInfo* thread : m_threads)
        {
            uint64_t start;
            size_t size;

            // Add the thread's stack and some of the code 
            thread->GetThreadStack(*this, &start, &size);
            InsertMemoryRegion(start, size);

            thread->GetThreadCode(&start, &size);
            InsertMemoryRegion(start, size);
        }
        // All the regions added so far has been backed by memory. Now add the rest of 
        // mappings so the debuggers like lldb see that an address is code (PF_X) even
        // if it isn't actually in the core dump.
        for (const MemoryRegion& region : m_moduleMappings)
        {
            assert(!region.IsBackedByMemory());
            InsertMemoryRegion(region);
        }
        for (const MemoryRegion& region : m_otherMappings)
        {
            assert(!region.IsBackedByMemory());
            InsertMemoryRegion(region);
        }
    }
    // Join all adjacent memory regions
    CombineMemoryRegions();
    return true;
}

void
CrashInfo::ResumeThreads()
{
    if (!m_sos)
    {
        for (ThreadInfo* thread : m_threads)
        {
            thread->ResumeThread();
        }
    }
}

//
// Get the auxv entries to use and add to the core dump
//
bool 
CrashInfo::GetAuxvEntries()
{
    char auxvPath[128];
    snprintf(auxvPath, sizeof(auxvPath), "/proc/%d/auxv", m_pid);

    int fd = open(auxvPath, O_RDONLY, 0);
    if (fd == -1)
    {
        fprintf(stderr, "open(%s) FAILED %s\n", auxvPath, strerror(errno));
        return false;
    }
    bool result = false;
    elf_aux_entry auxvEntry;

    while (read(fd, &auxvEntry, sizeof(elf_aux_entry)) == sizeof(elf_aux_entry)) 
    {
        m_auxvEntries.push_back(auxvEntry);
        if (auxvEntry.a_type == AT_NULL) 
        {
            break;
        }
        if (auxvEntry.a_type < AT_MAX) 
        {
            m_auxvValues[auxvEntry.a_type] = auxvEntry.a_un.a_val;
            TRACE("AUXV: %lu = %016lx\n", auxvEntry.a_type, auxvEntry.a_un.a_val);
            result = true;
        }
    }

    close(fd);
    return result;
}

//
// Get the module mappings for the core dump NT_FILE notes
//
bool
CrashInfo::EnumerateModuleMappings()
{
    // Here we read /proc/<pid>/maps file in order to parse it and figure out what it says 
    // about a library we are looking for. This file looks something like this:
    //
    // [address]          [perms] [offset] [dev] [inode] [pathname] - HEADER is not preset in an actual file
    //
    // 35b1800000-35b1820000 r-xp 00000000 08:02 135522  /usr/lib64/ld-2.15.so
    // 35b1a1f000-35b1a20000 r--p 0001f000 08:02 135522  /usr/lib64/ld-2.15.so
    // 35b1a20000-35b1a21000 rw-p 00020000 08:02 135522  /usr/lib64/ld-2.15.so
    // 35b1a21000-35b1a22000 rw-p 00000000 00:00 0       [heap]
    // 35b1c00000-35b1dac000 r-xp 00000000 08:02 135870  /usr/lib64/libc-2.15.so
    // 35b1dac000-35b1fac000 ---p 001ac000 08:02 135870  /usr/lib64/libc-2.15.so
    // 35b1fac000-35b1fb0000 r--p 001ac000 08:02 135870  /usr/lib64/libc-2.15.so
    // 35b1fb0000-35b1fb2000 rw-p 001b0000 08:02 135870  /usr/lib64/libc-2.15.so
    char* line = NULL;
    size_t lineLen = 0;
    int count = 0;
    ssize_t read;

    // Making something like: /proc/123/maps
    char mapPath[128];
    int chars = snprintf(mapPath, sizeof(mapPath), "/proc/%d/maps", m_pid);
    assert(chars > 0 && chars <= sizeof(mapPath));

    FILE* mapsFile = fopen(mapPath, "r");
    if (mapsFile == NULL)
    {
        fprintf(stderr, "fopen(%s) FAILED %s\n", mapPath, strerror(errno));
        return false;
    }
    // linuxGateAddress is the beginning of the kernel's mapping of
    // linux-gate.so in the process.  It doesn't actually show up in the
    // maps list as a filename, but it can be found using the AT_SYSINFO_EHDR
    // aux vector entry, which gives the information necessary to special
    // case its entry when creating the list of mappings.
    // See http://www.trilithium.com/johan/2005/08/linux-gate/ for more
    // information.
    const void* linuxGateAddress = (const void*)m_auxvValues[AT_SYSINFO_EHDR];

    // Reading maps file line by line 
    while ((read = getline(&line, &lineLen, mapsFile)) != -1)
    {
        uint64_t start, end, offset;
        char* permissions = nullptr;
        char* moduleName = nullptr;

        int c = sscanf(line, "%lx-%lx %m[-rwxsp] %lx %*[:0-9a-f] %*d %ms\n", &start, &end, &permissions, &offset, &moduleName);
        if (c == 4 || c == 5)
        {
            // r = read
            // w = write
            // x = execute
            // s = shared
            // p = private (copy on write)
            uint32_t regionFlags = 0;
            if (strchr(permissions, 'r')) {
                regionFlags |= PF_R;
            }
            if (strchr(permissions, 'w')) {
                regionFlags |= PF_W;
            }
            if (strchr(permissions, 'x')) {
                regionFlags |= PF_X;
            }
            if (strchr(permissions, 's')) {
                regionFlags |= MEMORY_REGION_FLAG_SHARED;
            }
            if (strchr(permissions, 'p')) {
                regionFlags |= MEMORY_REGION_FLAG_PRIVATE;
            }
            MemoryRegion memoryRegion(regionFlags, start, end, offset, moduleName);

            if (moduleName != nullptr && *moduleName == '/') {
                m_moduleMappings.insert(memoryRegion);
            }
            else {
                m_otherMappings.insert(memoryRegion);
            }
            if (linuxGateAddress != nullptr && reinterpret_cast<void*>(start) == linuxGateAddress)
            {
                InsertMemoryBackedRegion(memoryRegion);
            }
            free(permissions);
        }
    }

    if (g_diagnostics)
    {
        TRACE("Module mappings:\n");
        for (const MemoryRegion& region : m_moduleMappings)
        {
            region.Trace();
        }
        TRACE("Other mappings:\n");
        for (const MemoryRegion& region : m_otherMappings)
        {
            region.Trace();
        }
    }

    free(line); // We didn't allocate line, but as per contract of getline we should free it
    fclose(mapsFile);

    return true;
}

//
// All the shared (native) module info to the core dump
//
bool
CrashInfo::GetDSOInfo()
{
    Phdr* phdrAddr = reinterpret_cast<Phdr*>(m_auxvValues[AT_PHDR]);
    int phnum = m_auxvValues[AT_PHNUM];
    assert(m_auxvValues[AT_PHENT] == sizeof(Phdr));
    assert(phnum != PN_XNUM);

    if (phnum <= 0 || phdrAddr == nullptr) {
        return false;
    }
    TRACE("DSO: phdr %p phnum %d\n", phdrAddr, phnum);

    // Search for the program PT_DYNAMIC header 
    ElfW(Dyn)* dynamicAddr = nullptr;
    for (int i = 0; i < phnum; i++, phdrAddr++)
    {
        Phdr ph;
        if (!ReadMemory(phdrAddr, &ph, sizeof(ph))) {
            fprintf(stderr, "ReadMemory(%p, %lx) phdr FAILED\n", phdrAddr, sizeof(ph));
            return false;
        }
        TRACE("DSO: phdr %p type %d (%x) vaddr %016lx memsz %016lx offset %016lx\n", 
            phdrAddr, ph.p_type, ph.p_type, ph.p_vaddr, ph.p_memsz, ph.p_offset);

        if (ph.p_type == PT_DYNAMIC) 
        {
            dynamicAddr = reinterpret_cast<ElfW(Dyn)*>(ph.p_vaddr);
        }
        else if (ph.p_type == PT_NOTE || ph.p_type == PT_GNU_EH_FRAME)
        {
            if (ph.p_vaddr != 0 && ph.p_memsz != 0)
            {
                InsertMemoryRegion(ph.p_vaddr, ph.p_memsz);
            }
        }
    }

    if (dynamicAddr == nullptr) {
        return false;
    }

    // Search for dynamic debug (DT_DEBUG) entry
    struct r_debug* rdebugAddr = nullptr;
    for (;;) {
        ElfW(Dyn) dyn;
        if (!ReadMemory(dynamicAddr, &dyn, sizeof(dyn))) {
            fprintf(stderr, "ReadMemory(%p, %lx) dyn FAILED\n", dynamicAddr, sizeof(dyn));
            return false;
        }
        TRACE("DSO: dyn %p tag %ld (%lx) d_ptr %016lx\n", dynamicAddr, dyn.d_tag, dyn.d_tag, dyn.d_un.d_ptr);
        if (dyn.d_tag == DT_DEBUG) {
            rdebugAddr = reinterpret_cast<struct r_debug*>(dyn.d_un.d_ptr);
        }
        else if (dyn.d_tag == DT_NULL) {
            break;
        }
        dynamicAddr++;
    }

    // Add the DSO r_debug entry
    TRACE("DSO: rdebugAddr %p\n", rdebugAddr);
    struct r_debug debugEntry;
    if (!ReadMemory(rdebugAddr, &debugEntry, sizeof(debugEntry))) {
        fprintf(stderr, "ReadMemory(%p, %lx) r_debug FAILED\n", rdebugAddr, sizeof(debugEntry));
        return false;
    }

    // Add the DSO link_map entries
    ArrayHolder<char> moduleName = new char[PATH_MAX];
    for (struct link_map* linkMapAddr = debugEntry.r_map; linkMapAddr != nullptr;) {
        struct link_map map;
        if (!ReadMemory(linkMapAddr, &map, sizeof(map))) {
            fprintf(stderr, "ReadMemory(%p, %lx) link_map FAILED\n", linkMapAddr, sizeof(map));
            return false;
        }
        // Read the module's name and make sure the memory is added to the core dump
        int i = 0;
        if (map.l_name != nullptr) {
            for (; i < PATH_MAX; i++)
            {
                if (!ReadMemory(map.l_name + i, &moduleName[i], 1)) {
                    TRACE("DSO: ReadMemory link_map name %p + %d FAILED\n", map.l_name, i);
                    break;
                }
                if (moduleName[i] == '\0') {
                    break;
                }
            }
        }
        moduleName[i] = '\0';
        TRACE("\nDSO: link_map entry %p l_ld %p l_addr (Ehdr) %lx %s\n", linkMapAddr, map.l_ld, map.l_addr, (char*)moduleName);

        // Read the ELF header and info adding it to the core dump
        if (!GetELFInfo(map.l_addr)) {
            return false;
        }
        linkMapAddr = map.l_next;
    }

    return true;
}

inline bool
NameCompare(const char* name, const char* sectionName)
{
    return strncmp(name, sectionName, strlen(sectionName) + 1) == 0;
}

//
// Add all the necessary ELF headers to the core dump
//
bool
CrashInfo::GetELFInfo(uint64_t baseAddress)
{
    if (baseAddress == 0) {
        return true;
    }
    Ehdr ehdr;
    if (!ReadMemory((void*)baseAddress, &ehdr, sizeof(ehdr))) {
        fprintf(stderr, "ReadMemory(%p, %lx) ehdr FAILED\n", (void*)baseAddress, sizeof(ehdr));
        return false;
    }
    int phnum = ehdr.e_phnum;
    int shnum = ehdr.e_shnum;
    assert(phnum != PN_XNUM);
    assert(shnum != SHN_XINDEX);
    assert(ehdr.e_shstrndx != SHN_XINDEX);
    assert(ehdr.e_phentsize == sizeof(Phdr));
    assert(ehdr.e_shentsize == sizeof(Shdr));
    assert(ehdr.e_ident[EI_CLASS] == ELFCLASS64);
    assert(ehdr.e_ident[EI_DATA] == ELFDATA2LSB);

    TRACE("ELF: type %d mach 0x%x ver %d flags 0x%x phnum %d phoff %016lx phentsize 0x%02x shnum %d shoff %016lx shentsize 0x%02x shstrndx %d\n",
        ehdr.e_type, ehdr.e_machine, ehdr.e_version, ehdr.e_flags, phnum, ehdr.e_phoff, ehdr.e_phentsize, shnum, ehdr.e_shoff, ehdr.e_shentsize, ehdr.e_shstrndx);

    if (ehdr.e_phoff != 0 && phnum > 0)
    {
        Phdr* phdrAddr = reinterpret_cast<Phdr*>(baseAddress + ehdr.e_phoff);

        // Add the program headers and search for the module's note and unwind info segments
        for (int i = 0; i < phnum; i++, phdrAddr++)
        {
            Phdr ph;
            if (!ReadMemory(phdrAddr, &ph, sizeof(ph))) {
                fprintf(stderr, "ReadMemory(%p, %lx) phdr FAILED\n", phdrAddr, sizeof(ph));
                return false;
            }
            TRACE("ELF: phdr %p type %d (%x) vaddr %016lx memsz %016lx paddr %016lx filesz %016lx offset %016lx align %016lx\n",
                phdrAddr, ph.p_type, ph.p_type, ph.p_vaddr, ph.p_memsz, ph.p_paddr, ph.p_filesz, ph.p_offset, ph.p_align);

            if (ph.p_type == PT_DYNAMIC || ph.p_type == PT_NOTE || ph.p_type == PT_GNU_EH_FRAME)
            {
                if (ph.p_vaddr != 0 && ph.p_memsz != 0)
                {
                    InsertMemoryRegion(baseAddress + ph.p_vaddr, ph.p_memsz);
                }
            }
        }
    }

    // Skip the "interpreter" module i.e. /lib64/ld-linux-x86-64.so.2 or ld-2.19.so. The in-memory section headers are 
    // not valid. Ignore all failures too because on debug builds of coreclr, the section headers are not in valid memory.
    if (baseAddress != m_auxvValues[AT_BASE] && ehdr.e_shoff != 0 && shnum > 0 && ehdr.e_shstrndx != SHN_UNDEF)
    {
        Shdr* shdrAddr = reinterpret_cast<Shdr*>(baseAddress + ehdr.e_shoff);

        // Get the string table section header
        Shdr stringTableSectionHeader;
        if (!ReadMemory(shdrAddr + ehdr.e_shstrndx, &stringTableSectionHeader, sizeof(stringTableSectionHeader))) {
            TRACE("ELF: %2d shdr %p ReadMemory string table section header FAILED\n", ehdr.e_shstrndx, shdrAddr + ehdr.e_shstrndx);
            return true;
        }
        // Get the string table
        ArrayHolder<char> stringTable = new char[stringTableSectionHeader.sh_size];
        if (!ReadMemory((void*)(baseAddress + stringTableSectionHeader.sh_offset), stringTable.GetPtr(), stringTableSectionHeader.sh_size)) {
            TRACE("ELF: %2d shdr %p ReadMemory string table FAILED\n", ehdr.e_shstrndx, (void*)(baseAddress + stringTableSectionHeader.sh_offset));
            return true;
        }
        // Add the section headers to the core dump
        for (int sectionIndex = 0; sectionIndex < shnum; sectionIndex++, shdrAddr++)
        {
            Shdr sh;
            if (!ReadMemory(shdrAddr, &sh, sizeof(sh))) {
                TRACE("ELF: %2d shdr %p ReadMemory FAILED\n", sectionIndex, shdrAddr);
                return true;
            }
            TRACE("ELF: %2d shdr %p type %2d (%x) addr %016lx offset %016lx size %016lx link %08x info %08x name %4d %s\n",
                sectionIndex, shdrAddr, sh.sh_type, sh.sh_type, sh.sh_addr, sh.sh_offset, sh.sh_size, sh.sh_link, sh.sh_info, sh.sh_name, &stringTable[sh.sh_name]);

            if (sh.sh_name != SHN_UNDEF && sh.sh_offset > 0 && sh.sh_size > 0) {
                char* name = &stringTable[sh.sh_name];

                // Add the .eh_frame/.eh_frame_hdr unwind info to the core dump
                if (NameCompare(name, ".eh_frame") ||
                    NameCompare(name, ".eh_frame_hdr") ||
                    NameCompare(name, ".note.gnu.build-id") ||
                    NameCompare(name, ".note.gnu.ABI-tag") ||
                    NameCompare(name, ".gnu_debuglink"))
                {
                    TRACE("ELF: %s %p size %016lx\n", name, (void*)(baseAddress + sh.sh_offset), sh.sh_size);
                    InsertMemoryRegion(baseAddress + sh.sh_offset, sh.sh_size);
                }
            }
        }
    }

    return true;
}

//
// Enumerate all the memory regions using the DAC memory region support given a minidump type
//
bool
CrashInfo::EnumerateMemoryRegionsWithDAC(const char* programPath, MINIDUMP_TYPE minidumpType)
{
    PFN_CLRDataCreateInstance pfnCLRDataCreateInstance = nullptr;
    ICLRDataEnumMemoryRegions* clrDataEnumRegions = nullptr;
    IXCLRDataProcess* clrDataProcess = nullptr;
    HMODULE hdac = nullptr;
    HRESULT hr = S_OK;
    bool result = false;

    // We assume that the DAC is in the same location as this createdump exe
    std::string dacPath;
    dacPath.append(programPath);
    dacPath.append("/");
    dacPath.append(MAKEDLLNAME_A("mscordaccore"));

    // Load and initialize the DAC
    hdac = LoadLibraryA(dacPath.c_str());
    if (hdac == nullptr)
    {
        fprintf(stderr, "LoadLibraryA(%s) FAILED %d\n", dacPath.c_str(), GetLastError());
        goto exit;
    }
    pfnCLRDataCreateInstance = (PFN_CLRDataCreateInstance)GetProcAddress(hdac, "CLRDataCreateInstance");
    if (pfnCLRDataCreateInstance == nullptr)
    {
        fprintf(stderr, "GetProcAddress(CLRDataCreateInstance) FAILED %d\n", GetLastError());
        goto exit;
    }
    if ((minidumpType & MiniDumpWithFullMemory) == 0)
    {
        hr = pfnCLRDataCreateInstance(__uuidof(ICLRDataEnumMemoryRegions), m_dataTarget, (void**)&clrDataEnumRegions);
        if (FAILED(hr))
        {
            fprintf(stderr, "CLRDataCreateInstance(ICLRDataEnumMemoryRegions) FAILED %08x\n", hr);
            goto exit;
        }
        // Calls CrashInfo::EnumMemoryRegion for each memory region found by the DAC
        hr = clrDataEnumRegions->EnumMemoryRegions(this, minidumpType, CLRDATA_ENUM_MEM_DEFAULT);
        if (FAILED(hr))
        {
            fprintf(stderr, "EnumMemoryRegions FAILED %08x\n", hr);
            goto exit;
        }
    }
    hr = pfnCLRDataCreateInstance(__uuidof(IXCLRDataProcess), m_dataTarget, (void**)&clrDataProcess);
    if (FAILED(hr))
    {
        fprintf(stderr, "CLRDataCreateInstance(IXCLRDataProcess) FAILED %08x\n", hr);
        goto exit;
    }
    if (!EnumerateManagedModules(clrDataProcess))
    {
        goto exit;
    }
    result = true;
exit:
    if (clrDataEnumRegions != nullptr)
    {
        clrDataEnumRegions->Release();
    }
    if (clrDataProcess != nullptr)
    {
        clrDataProcess->Release();
    }
    if (hdac != nullptr)
    {
        FreeLibrary(hdac);
    }
    return result;
}

//
// Enumerate all the managed modules and replace the module 
// mapping with the module name found.
//
bool
CrashInfo::EnumerateManagedModules(IXCLRDataProcess* clrDataProcess)
{
    IXCLRDataModule* clrDataModule = nullptr;
    CLRDATA_ENUM enumModules = 0;
    HRESULT hr = S_OK;

    if (FAILED(hr = clrDataProcess->StartEnumModules(&enumModules))) {
        fprintf(stderr, "StartEnumModules FAILED %08x\n", hr);
        return false;
    }
    while ((hr = clrDataProcess->EnumModule(&enumModules, &clrDataModule)) == S_OK)
    {
        DacpGetModuleData moduleData;
        if (SUCCEEDED(hr = moduleData.Request(clrDataModule)))
        {
            TRACE("MODULE: %016lx dyn %d inmem %d file %d pe %016lx pdb %016lx", moduleData.LoadedPEAddress, moduleData.IsDynamic, 
                moduleData.IsInMemory, moduleData.IsFileLayout, moduleData.PEFile, moduleData.InMemoryPdbAddress);

            if (!moduleData.IsDynamic && moduleData.LoadedPEAddress != 0)
            {
                ArrayHolder<WCHAR> wszUnicodeName = new WCHAR[MAX_LONGPATH + 1];
                if (SUCCEEDED(hr = clrDataModule->GetFileName(MAX_LONGPATH, NULL, wszUnicodeName)))
                {
                    char* pszName = (char*)malloc(MAX_LONGPATH + 1);
                    if (pszName == nullptr) {
                        fprintf(stderr, "Allocating module name FAILED\n");
                        return false;
                    }
                    sprintf_s(pszName, MAX_LONGPATH, "%S", (WCHAR*)wszUnicodeName);
                    TRACE(" %s\n", pszName);

                    // Change the module mapping name
                    ReplaceModuleMapping(moduleData.LoadedPEAddress, pszName);
                }
                else {
                    TRACE("\nModule.GetFileName FAILED %08x\n", hr);
                }
            }
            else {
                TRACE("\n");
            }
        }
        else {
            TRACE("moduleData.Request FAILED %08x\n", hr);
        }
        if (clrDataModule != nullptr) {
            clrDataModule->Release();
        }
    }
    if (enumModules != 0) {
        clrDataProcess->EndEnumModules(enumModules);
    }
    return true;
}

//
// Replace an existing module mapping with one with a different name.
//
void
CrashInfo::ReplaceModuleMapping(CLRDATA_ADDRESS baseAddress, const char* pszName)
{
    // Add or change the module mapping for this PE image. The managed assembly images are
    // already in the module mappings list but in .NET 2.0 they have the name "/dev/zero".
    MemoryRegion region(PF_R | PF_W | PF_X, baseAddress, baseAddress + PAGE_SIZE, 0, pszName);
    const auto& found = m_moduleMappings.find(region);
    if (found == m_moduleMappings.end())
    {
        m_moduleMappings.insert(region);

        if (g_diagnostics) {
            TRACE("MODULE: ADD ");
            region.Trace();
        }
    }
    else
    {
        // Create the new memory region with the managed assembly name.
        MemoryRegion newRegion(*found, pszName);

        // Remove and cleanup the old one
        m_moduleMappings.erase(found);
        const_cast<MemoryRegion&>(*found).Cleanup();

        // Add the new memory region
        m_moduleMappings.insert(newRegion);

        if (g_diagnostics) {
            TRACE("MODULE: REPLACE ");
            newRegion.Trace();
        }
    }
}

//
// ReadMemory from target and add to memory regions list
//
bool
CrashInfo::ReadMemory(void* address, void* buffer, size_t size)
{
    uint32_t read = 0;
    if (FAILED(m_dataTarget->ReadVirtual(reinterpret_cast<CLRDATA_ADDRESS>(address), reinterpret_cast<PBYTE>(buffer), size, &read)))
    {
        return false;
    }
    InsertMemoryRegion(reinterpret_cast<uint64_t>(address), size);
    return true;
}

//
// Add this memory chunk to the list of regions to be 
// written to the core dump.
//
void
CrashInfo::InsertMemoryRegion(uint64_t address, size_t size)
{
    assert(size < UINT_MAX);

    // Round to page boundary
    uint64_t start = address & PAGE_MASK;
    assert(start > 0);

    // Round up to page boundary
    uint64_t end = ((address + size) + (PAGE_SIZE - 1)) & PAGE_MASK;
    assert(end > 0);

    InsertMemoryRegion(MemoryRegion(GetMemoryRegionFlags(start) | MEMORY_REGION_FLAG_MEMORY_BACKED, start, end));
}

//
// Adds a memory backed flagged copy of the memory region. The file name is not preserved.
//
void
CrashInfo::InsertMemoryBackedRegion(const MemoryRegion& region)
{
    InsertMemoryRegion(MemoryRegion(region, region.Flags() | MEMORY_REGION_FLAG_MEMORY_BACKED));
}

//
// Add a memory region to the list
//
void
CrashInfo::InsertMemoryRegion(const MemoryRegion& region)
{
    // First check if the full memory region can be added without conflicts and is fully valid.
    const auto& found = m_memoryRegions.find(region);
    if (found == m_memoryRegions.end())
    {
        // If the region is valid, add the full memory region
        if (ValidRegion(region)) {
            m_memoryRegions.insert(region);
            return;
        }
    }
    else
    {
        // If the memory region is wholly contained in region found and both have the 
        // same backed by memory state, we're done.
        if (found->Contains(region) && (found->IsBackedByMemory() == region.IsBackedByMemory())) {
            return;
        }
    }
    // Either part of the region was invalid, part of it hasn't been added or the backed
    // by memory state is different.
    uint64_t start = region.StartAddress();

    // The region overlaps/conflicts with one already in the set so add one page at a 
    // time to avoid the overlapping pages.
    uint64_t numberPages = region.Size() >> PAGE_SHIFT;

    for (int p = 0; p < numberPages; p++, start += PAGE_SIZE)
    {
        MemoryRegion memoryRegionPage(region.Flags(), start, start + PAGE_SIZE);

        const auto& found = m_memoryRegions.find(memoryRegionPage);
        if (found == m_memoryRegions.end())
        {
            // All the single pages added here will be combined in CombineMemoryRegions()
            if (ValidRegion(memoryRegionPage)) {
                m_memoryRegions.insert(memoryRegionPage);
            }
        }
        else {
            assert(found->IsBackedByMemory() || !region.IsBackedByMemory());
        }
    }
}

//
// Get the memory region flags for a start address
//
uint32_t 
CrashInfo::GetMemoryRegionFlags(uint64_t start)
{
    const MemoryRegion* region = SearchMemoryRegions(m_moduleMappings, start);
    if (region != nullptr) {
        return region->Flags();
    }
    region = SearchMemoryRegions(m_otherMappings, start);
    if (region != nullptr) {
        return region->Flags();
    }
    TRACE("GetMemoryRegionFlags: FAILED\n");
    return PF_R | PF_W | PF_X;
}

//
// Validates a memory region
//
bool
CrashInfo::ValidRegion(const MemoryRegion& region)
{
    if (region.IsBackedByMemory())
    {
        uint64_t start = region.StartAddress();
        uint64_t numberPages = region.Size() >> PAGE_SHIFT;

        for (int p = 0; p < numberPages; p++, start += PAGE_SIZE)
        {
            BYTE buffer[1];
            uint32_t read;

            if (FAILED(m_dataTarget->ReadVirtual(start, buffer, 1, &read)))
            {
                return false;
            }
        }
    }
    return true;
}

//
// Combine any adjacent memory regions into one
//
void
CrashInfo::CombineMemoryRegions()
{
    assert(!m_memoryRegions.empty());

    std::set<MemoryRegion> memoryRegionsNew;

    // MEMORY_REGION_FLAG_SHARED and MEMORY_REGION_FLAG_PRIVATE are internal flags that 
    // don't affect the core dump so ignore them when comparing the flags.
    uint32_t flags = m_memoryRegions.begin()->Flags() & (MEMORY_REGION_FLAG_MEMORY_BACKED | MEMORY_REGION_FLAG_PERMISSIONS_MASK);
    uint64_t start = m_memoryRegions.begin()->StartAddress();
    uint64_t end = start;

    for (const MemoryRegion& region : m_memoryRegions)
    {
        // To combine a region it needs to be contiguous, same permissions and memory backed flag.
        if ((end == region.StartAddress()) && 
            (flags == (region.Flags() & (MEMORY_REGION_FLAG_MEMORY_BACKED | MEMORY_REGION_FLAG_PERMISSIONS_MASK))))
        {
            end = region.EndAddress();
        }
        else
        {
            MemoryRegion memoryRegion(flags, start, end);
            assert(memoryRegionsNew.find(memoryRegion) == memoryRegionsNew.end());
            memoryRegionsNew.insert(memoryRegion);

            flags = region.Flags() & (MEMORY_REGION_FLAG_MEMORY_BACKED | MEMORY_REGION_FLAG_PERMISSIONS_MASK);
            start = region.StartAddress();
            end = region.EndAddress();
        }
    }

    assert(start != end);
    MemoryRegion memoryRegion(flags, start, end);
    assert(memoryRegionsNew.find(memoryRegion) == memoryRegionsNew.end());
    memoryRegionsNew.insert(memoryRegion);

    m_memoryRegions = memoryRegionsNew;

    if (g_diagnostics)
    {
        TRACE("Memory Regions:\n");
        for (const MemoryRegion& region : m_memoryRegions)
        {
            region.Trace();
        }
    }
}

//
// Searches for a memory region given an address.
//
const MemoryRegion* 
CrashInfo::SearchMemoryRegions(const std::set<MemoryRegion>& regions, uint64_t start)
{
    std::set<MemoryRegion>::iterator found = regions.find(MemoryRegion(0, start, start + PAGE_SIZE));
    for (; found != regions.end(); found++)
    {
        if (start >= found->StartAddress() && start < found->EndAddress())
        {
            return &*found;
        }
    }
	return nullptr;
}

//
// Get the process or thread status
//
bool
CrashInfo::GetStatus(pid_t pid, pid_t* ppid, pid_t* tgid, char** name)
{
    char statusPath[128];
    snprintf(statusPath, sizeof(statusPath), "/proc/%d/status", pid);

    FILE *statusFile = fopen(statusPath, "r");
    if (statusFile == nullptr)
    {
        fprintf(stderr, "GetStatus fopen(%s) FAILED\n", statusPath);
        return false;
    }

    *ppid = -1;

    char *line = nullptr;
    size_t lineLen = 0;
    ssize_t read;
    while ((read = getline(&line, &lineLen, statusFile)) != -1)
    {
        if (strncmp("PPid:\t", line, 6) == 0)
        {
            *ppid = _atoi64(line + 6);
        }
        else if (strncmp("Tgid:\t", line, 6) == 0)
        {
            *tgid = _atoi64(line + 6);
        }
        else if (strncmp("Name:\t", line, 6) == 0)
        {
            if (name != nullptr)
            {
                char* n = strchr(line + 6, '\n');
                if (n != nullptr) 
                {
                    *n = '\0';
                }
                *name = strdup(line + 6);
            }
        }
    }

    free(line);
    fclose(statusFile);
    return true;
}

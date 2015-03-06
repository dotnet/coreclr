WCHAR* GCStatistics::logFileName = NULL;
FILE*  GCStatistics::logFile = NULL;

void GCStatistics::AddGCStats(const gc_mechanisms& settings, size_t timeInMSec)
{
#ifdef BACKGROUND_GC
    if (settings.concurrent)
    {
        bgc.Accumulate((DWORD)timeInMSec*1000);
        cntBGC++;
    }
    else if (settings.background_p)
    {
        fgc.Accumulate((DWORD)timeInMSec*1000);
        cntFGC++;
        if (settings.compaction)
            cntCompactFGC++;
        assert(settings.condemned_generation < max_generation);
        cntFGCGen[settings.condemned_generation]++;
    }
    else
#endif // BACKGROUND_GC
    {
        ngc.Accumulate((DWORD)timeInMSec*1000);
        cntNGC++;
        if (settings.compaction)
            cntCompactNGC++;
        cntNGCGen[settings.condemned_generation]++;
    }

    if (is_induced (settings.reason))
        cntReasons[(int)reason_induced]++;
    else if (settings.stress_induced)
        cntReasons[(int)reason_gcstress]++;
    else
        cntReasons[(int)settings.reason]++;

#ifdef BACKGROUND_GC
    if (settings.concurrent || !settings.background_p)
    {
#endif // BACKGROUND_GC
        RollOverIfNeeded();
#ifdef BACKGROUND_GC
    }
#endif // BACKGROUND_GC
}

void GCStatistics::Initialize()
{
    LIMITED_METHOD_CONTRACT;
    // for efficiency sake we're taking a dependency on the layout of a C++ object
    // with a vtable. protect against violations of our premise:
    static_assert(offsetof(GCStatistics, cntDisplay) == sizeof(void*),
            "The first field of GCStatistics follows the pointer sized vtable");

    int podOffs = offsetof(GCStatistics, cntDisplay);       // offset of the first POD field
    memset((BYTE*)(&g_GCStatistics)+podOffs, 0, sizeof(g_GCStatistics)-podOffs);
    memset((BYTE*)(&g_LastGCStatistics)+podOffs, 0, sizeof(g_LastGCStatistics)-podOffs);
}

void GCStatistics::DisplayAndUpdate()
{
    LIMITED_METHOD_CONTRACT;

    if (logFileName == NULL || logFile == NULL)
        return;

    {
        if (cntDisplay == 0)
            fprintf(logFile, "\nGCMix **** Initialize *****\n\n");
            
        fprintf(logFile, "GCMix **** Summary ***** %d\n", cntDisplay);

        // NGC summary (total, timing info)
        ngc.DisplayAndUpdate(logFile, "NGC ", &g_LastGCStatistics.ngc, cntNGC, g_LastGCStatistics.cntNGC, msec);

        // FGC summary (total, timing info)
        fgc.DisplayAndUpdate(logFile, "FGC ", &g_LastGCStatistics.fgc, cntFGC, g_LastGCStatistics.cntFGC, msec);

        // BGC summary
        bgc.DisplayAndUpdate(logFile, "BGC ", &g_LastGCStatistics.bgc, cntBGC, g_LastGCStatistics.cntBGC, msec);

        // NGC/FGC break out by generation & compacting vs. sweeping
        fprintf(logFile, "NGC   ");
        for (int i = max_generation; i >= 0; --i)
            fprintf(logFile, "gen%d %d (%d). ", i, cntNGCGen[i]-g_LastGCStatistics.cntNGCGen[i], cntNGCGen[i]);
        fprintf(logFile, "\n");

        fprintf(logFile, "FGC   ");
        for (int i = max_generation-1; i >= 0; --i)
            fprintf(logFile, "gen%d %d (%d). ", i, cntFGCGen[i]-g_LastGCStatistics.cntFGCGen[i], cntFGCGen[i]);
        fprintf(logFile, "\n");

        // Compacting vs. Sweeping break out
        int _cntSweep = cntNGC-cntCompactNGC;
        int _cntLastSweep = g_LastGCStatistics.cntNGC-g_LastGCStatistics.cntCompactNGC;
        fprintf(logFile, "NGC   Sweeping %d (%d) Compacting %d (%d)\n",
               _cntSweep - _cntLastSweep, _cntSweep,
               cntCompactNGC - g_LastGCStatistics.cntCompactNGC, cntCompactNGC);

        _cntSweep = cntFGC-cntCompactFGC;
        _cntLastSweep = g_LastGCStatistics.cntFGC-g_LastGCStatistics.cntCompactFGC;
        fprintf(logFile, "FGC   Sweeping %d (%d) Compacting %d (%d)\n",
               _cntSweep - _cntLastSweep, _cntSweep,
               cntCompactFGC - g_LastGCStatistics.cntCompactFGC, cntCompactFGC);

#ifdef TRACE_GC
        // GC reasons...
        for (int reason=(int)reason_alloc_soh; reason <= (int)reason_gcstress; ++reason)
        {
            if (cntReasons[reason] != 0)
                fprintf(logFile, "%s %d (%d). ", str_gc_reasons[reason], 
                    cntReasons[reason]-g_LastGCStatistics.cntReasons[reason], cntReasons[reason]);
        }
#endif // TRACE_GC
        fprintf(logFile, "\n\n");

        // flush the log file...
        fflush(logFile);
    }

    memcpy(&g_LastGCStatistics, this, sizeof(g_LastGCStatistics));

    ngc.Reset();
    fgc.Reset();
    bgc.Reset();
}
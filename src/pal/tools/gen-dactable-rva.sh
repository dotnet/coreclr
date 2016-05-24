nm $1 | grep g_dacTable | cut -f 1 -d' ' | head -n 1 | awk '{ print "#define DAC_TABLE_RVA 0x" $1}' > $2
nm $1 | grep g_coreclrProcessIsReady | cut -f 1 -d' ' | head -n 1 | awk '{ print "#define CORECLR_PROCESS_IS_READY_RVA 0x" $1}' >> $2




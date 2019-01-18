set CORE_LIBRARIES=%~dp1
%_DebuggerFullPath% "%CORE_ROOT%\corerun.exe" "%CORE_ROOT%\..\..\Common\runincontext\runincontext\runincontext.dll" %RunInContextExtraArgs% /referencespath:%CORE_ROOT%\ %1%2 %3 %4 %5 %6 %7 %8 %9


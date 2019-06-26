Param(
    [string] $SourceDirectory=$env:BUILD_SOURCESDIRECTORY,
    [string] $CoreRootDirectory,
    [string] $Architecture="x64",
    [string] $Framework="netcoreapp3.0",
    [string] $CompilationMode="Tiered",
    [string] $Repository=$env:BUILD_REPOSITORY_NAME,
    [string] $Branch=$env:BUILD_SOURCEBRANCH,
    [string] $CommitSha=$env:BUILD_SOURCEVERSION,
    [string] $BuildNumber=$env:BUILD_BUILDNUMBER,
    [string] $RunCategories="coreclr corefx",
    [string] $Csproj="src\benchmarks\micro\MicroBenchmarks.csproj",
    [switch] $Pr,
    [string] $Configurations
)

. $PSScriptRoot\..\pipeline-logging-functions.ps1

if ($Repository -eq "dotnet/performance") {
    $RunFromPerformanceRepo = $true;
}
else {
    $RunFromPerformanceRepo = $false;
}

if ($Configurations -eq [string]::Empty) {
    $Configurations="CompilationMode=$CompilationMode"
}

if ($CoreRootDirectory -eq [string]::Empty) {
    $UseCoreRun = $false
}
else {
    $UseCoreRun = $true
}

$PayloadDirectory = (Join-Path $SourceDirectory "Payload")
$PerformanceDirectory = (Join-Path $PayloadDirectory "performance")
$WorkItemDirectory = (Join-Path $SourceDirectory "workitem")
$Creator = ""

if ($Pr) {
    $Queue = "Windows.10.Amd64.ClientRS4.Open"
    $ExtraBenchmarkDotNetArguments = "--iterationCount 1 --warmupCount 0 --invocationCount 1 --unrollFactor 1 --strategy ColdStart --stopOnFirstError true"
    $Creator = $env:BUILD_DEFINITIONNAME
    $PerfLabArguments = ""
}
else {
    $Queue = "Windows.10.Amd64.ClientRS1.Perf"
    $PerfLabArguments = "--upload-to-perflab-container"
    $ExtraBenchmarkDotNetArguments = ""
    $Creator = ""
}

$CommonSetupArguments="--frameworks $Framework --queue $Queue --build-number $BuildNumber --build-configs $Configurations"

if ($RunFromPerformanceRepo) {
    $SetupArguments = "--perf-hash $CommitSha $CommonSetupArguments"
    
    robocopy $SourceDirectory $PerformanceDirectory /E /XD $PayloadDirectory $SourceDirectory\artifacts $SourceDirectory\.git
}
else {
    $SetupArguments = "--repository https://github.com/$Repository --branch $Branch --get-perf-hash --commit-sha $CommitSha $CommonSetupArguments"
    
    git clone --branch master --depth 1 --quiet https://github.com/dotnet/performance $PerformanceDirectory
}

if ($UseCoreRun) {
    $NewCoreRoot = (Join-Path $PayloadDirectory "Core_Root")
    Move-Item -Path $CoreRootDirectory -Destination $NewCoreRoot
}

$DocsDir = (Join-Path $PerformanceDirectory "docs")
robocopy $DocsDir $WorkItemDirectory

# Set variables that we will need to have in future steps
Write-Host "##vso[task.setvariable variable=UseCoreRun]$UseCoreRun"
Write-Host "##vso[task.setvariable variable=PayloadDirectory]$PayloadDirectory"
Write-Host "##vso[task.setvariable variable=PerformanceDirectory]$PerformanceDirectory"
Write-Host "##vso[task.setvariable variable=WorkItemDirectory]$WorkItemDirectory"
Write-Host "##vso[task.setvariable variable=Queue]$Queue"
Write-Host "##vso[task.setvariable variable=SetupArguments]$SetupArguments"
Write-Host "##vso[task.setvariable variable=Python]py -3"
Write-Host "##vso[task.setvariable variable=ExtraBenchmarkDotNetArguments]$ExtraBenchmarkDotNetArguments"
Write-Host "##vso[task.setvariable variable=BDNCategories]$RunCategories"
Write-Host "##vso[task.setvariable variable=TargetCsProj]$Csproj"
Write-Host "##vso[task.setvariable variable=RunFromPerfRepo]$RunFromPerformanceRepo"
Write-Host "##vso[task.setvariable variable=Creator]$Creator"
Write-Host "##vso[task.setvariable variable=PerfLabArguments]$PerfLabArguments"
Write-Host "##vso[task.setvariable variable=Architecture]$Architecture"

exit 0
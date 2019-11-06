# Working around issue https://github.com/dotnet/arcade/issues/2673
$script:DisableNativeToolsetInstalls = $true

function DotnetInstall()
{
  $repoRoot = $env:__RepoRootDir
  $coreClrRoot = $env:__ProjectDir
  $toolsScript = Join-Path $repoRoot "eng\common\tools.ps1"
  . $toolsScript
  $targetScript = Join-Path $coreClrRoot "bin\obj\set-dotnet-install-dir.cmd"
  InitializeBuildTool
  Set-Content -Path $targetScript -Value "set `"DOTNET_INSTALL_DIR=$env:DOTNET_INSTALL_DIR`""
}

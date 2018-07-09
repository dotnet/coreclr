[CmdletBinding()]
param(
  [string]$QueueId,
  [string]$Source,
  [string]$Type,
  [string]$Build,
  [string]$Attempt,
  [hashtable]$Properties
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"


$jobInfo = [pscustomobject]@{
  QueueId=$QueueId;
  Source=$Source;
  Type=$Type;
  Build=$Build;
  Properties=[pscustomobject]$Properties;
}

$jobInfoJson = $jobInfo | ConvertTo-Json

try {
  Write-Verbose "Job Info: $jobInfoJson"
  $jobToken = Invoke-RestMethod -Uri "https://helix.dot.net/api/2018-03-14/telemetry/job?access_token=$($env:HelixApiAccessToken)" -Method Post -ContentType "application/json" -Body $jobInfoJson

  $env:Helix_JobToken = $jobToken
  if (& "$PSScriptRoot/../is-vsts.ps1") {
    Write-Host "##vso[task.setvariable variable=Helix_JobToken;issecret=true;]$env:Helix_JobToken"
  }
}
catch {
  Write-Error $_
  Write-Error $_.Exception
  exit 1
}


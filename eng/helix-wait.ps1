# Script for checking on Helix job periodically

$finished = $false
$timeout = $false
$startTime = Get-Date

while (-Not $finished -And -Not $timeout) {
    $workItems = Invoke-WebRequest  -Headers @{"Accept"="application/json"} -Uri "https://helix.dot.net/api/2018-03-14/jobs/$env:HelixJobId/details?access_token=$env:HelixAccessToken" -UseBasicParsing |
    ConvertFrom-Json | Select -ExpandProperty "WorkItems"
    Write-Output "Waiting: $($workItems.Waiting); Running: $($workItems.Running)"
    if ($workItems.Waiting -eq 0 -And $workItems.Running -eq 0) {
        $finished = $true
    }
    $elapsedTime = New-Timespan $startTime $(Get-Date)
    Write-Output "Elapsed Time: $elapsedTime"
    if ($elapsedTime -gt (New-TimeSpan -Minutes 30)) {
        $timeout = $true
    }
}

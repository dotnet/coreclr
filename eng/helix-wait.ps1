# Script for checking on Helix job periodically

$finished = $false
$timeout = $false
$startTime = Get-Date

while (-Not $finished -And -Not $timeout) {
    $workItems = Invoke-WebRequest -Headers @{"Accept"="application/json"} -Uri "https://helix.dot.net/api/2018-03-14/jobs/$env:HelixJobId/details?access_token=$env:HelixAccessToken" -UseBasicParsing |
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

    Start-Sleep -s 10
}

if ($timeout) {
    Write-Host "##vso[task.logissue type=error;]Timed out while waiting for Helix job to complete"
    Write-Host "##vso[task.complete result=Failed;]TEIMOUT"
    exit 1
}

$aggregateStatus = Invoke-WebRequest -Headers @{"Accept"="application/json"} -Uri "https://helix.dot.net/api/2018-03-14/aggregate/workitems?groupBy=job.build&filter.name=$env:HelixJobId&access_token=$env:HelixAccessToken" -UseBasicParsing |
ConvertFrom-Json

$temp = "$($aggregateStatus.Key)" -match "=(?<buildId>[\d\.]+)}"
$buildId = $matches['buildId']

if ($aggregateStatus.Data.WorkItemStatus.fail -gt 0 -Or $aggregateStatus.Data.WorkItemStatus.none -gt 0) {
    Write-Host "##vso[task.logissue type=error;]Job failed -- see https://mc.dot.net/#/user/jonfortescue/pr~2Fcoreclr~2Fmaster/test~2Fstuff/$buildId"
    Write-Host "##vso[task.complete result=Failed;]FAILED"
    exit 1
}
elseif ($aggregateStatus.Data.Analysis.Status.fail -gt 0) {
    Write-Host "##vso[task.logissue type=error;]$($aggregateStatus.Data.Analysis.Status.fail) tests failed -- see https://mc.dot.net/#/user/jonfortescue/pr~2Fcoreclr~2Fmaster/test~2Fstuff/$buildId"
    Write-Host "##vso[task.complete result=Failed;]FAILED"
    exit 1
}
else {
    Write-Host "$($aggregateStatus.Data.Analysis.Status.pass + $aggregateStatus.Data.Analysis.Status.passonretry) tests passed; $($aggregateStatus.Data.Analysis.Status.skip) tests skipped."
}
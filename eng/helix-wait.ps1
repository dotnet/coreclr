# Script for checking on Helix job periodically
$finished = 0
$workItems = {}

while ($finished -lt $env:NumberOfWorkItems) {
    $workItems = Invoke-WebRequest -Headers @{"Accept"="application/json"} -Uri "https://helix.dot.net/api/2018-03-14/aggregate/workitems?groupBy=job.build&filter.name=$env:HelixJobId&access_token=$env:HelixAccessToken" -UseBasicParsing |
    ConvertFrom-Json

    $finished = $workItems.Data.WorkItemStatus.pass + $workItems.Data.WorkItemStatus.fail
    Write-Output "$finished work items finished out of $env:NumberOfWorkItems total work items."

    Start-Sleep -s 10
}

$temp = "$($workItems.Key)" -match "=(?<buildId>[\d\.]+)}"
$buildId = $matches['buildId']

if ($workItems.Data.WorkItemStatus.fail -gt 0 -Or $workItems.Data.WorkItemStatus.none -gt 0) {
    Write-Host "##vso[task.logissue type=error;]Some work items failed catastrophically failed -- see https://mc.dot.net/#/user/jonfortescue/pr~2Fcoreclr~2Fmaster/test~2Fstuff/$buildId"
    Write-Host "##vso[task.complete result=Failed;]FAILED"
    exit 1
}
elseif ($workItems.Data.Analysis.Status.fail -gt 0) {
    Write-Host "##vso[task.logissue type=error;]$($workItems.Data.Analysis.Status.fail) tests failed -- see https://mc.dot.net/#/user/jonfortescue/pr~2Fcoreclr~2Fmaster/test~2Fstuff/$buildId"
    Write-Host "##vso[task.complete result=Failed;]FAILED"
    exit 1
}
else {
    Write-Host "$($workItems.Data.Analysis.Status.pass + $workItems.Data.Analysis.Status.passonretry) tests passed; $($workItems.Data.Analysis.Status.skip) tests skipped."
}

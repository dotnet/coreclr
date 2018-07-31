Param(
  [string] $azCopyPath = "$PSScriptRoot\..\..\..\Tools\azcopy\AzCopy.exe",
  [string[]] $inputFiles,
  [string] $blobStorageUrl = "https://dotnetperfciblobs.blob.core.windows.net",
  [string] $container,
  [string] $fileName,
  
  [Parameter(Mandatory=$false)]
  [string] $sas
)

if (-not (Test-Path $azCopyPath)) 
{
    throw [System.IO.FileNotFoundException] "$azCopyPath not found."
}

$temp = "$env:TEMP\azcopy_"+[System.Guid]::NewGuid()
New-Item -ItemType directory -Path $temp | Out-Null

$zipped = "$temp\$fileName.zip"

Write-Output "Zipping the input files."
Compress-Archive -Path $inputFiles -DestinationPath $zipped

if (!$sas)
{
    $sas = $env:AZ_BLOB_LOGS_SAS_TOKEN
}

if (!$sas)
{
    throw [System.ArgumentException ] "SAS token not provided and not present in env var."
}

Write-Output "Uploading the input files to Azure Blob Storage."
Start-Process $azCopyPath -Argument "/Source:$zipped /Dest:$blobStorageUrl/$container/$fileName /DestSAS:$sas /V:.\bin\Logs\AzCopyLog.txt" -Wait

Write-Output "Removing temp files."
Remove-Item $temp -Recurse -Force
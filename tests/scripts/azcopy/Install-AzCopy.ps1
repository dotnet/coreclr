Param(
  [string] $toolsPath = "$PSScriptRoot\..\..\..\Tools\azcopy"
)

$azcopy = "$toolsPath\azcopy.exe"

if(Test-Path $azcopy){
	Write-Output "AzCopy.exe has already been installed."
} else {
	$temp = "$env:TEMP\azcopy_"+[System.Guid]::NewGuid()
	$unzipped = "$temp\unzipped"
	$msi = "$temp\MicrosoftAzureStorageTools.msi"

	New-Item -ItemType directory -Path $temp | Out-Null
	New-Item -ItemType directory -Path $unzipped | Out-Null

	Write-Output "Downloading AzCopy."
	Invoke-WebRequest -Uri "http://aka.ms/downloadazcopy" -OutFile $msi
	Unblock-File $msi

	Write-Host "Extracting AzCopy"
	Start-Process msiexec -Argument "/a $msi /qb TARGETDIR=$unzipped /quiet" -Wait

	Write-Host "Copying AzCopy to $toolsPath"
	New-Item -ItemType directory -Path $toolsPath | Out-Null
	Copy-Item "$unzipped\Microsoft SDKs\Azure\AzCopy\*" $toolsPath -Force

	Remove-Item $temp -Recurse -Force
}

# Display version of AzCopy.exe downloaded
Get-ChildItem $azcopy |% VersionInfo | Select ProductVersion,FileVersion
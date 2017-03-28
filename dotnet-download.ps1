param(
[parameter(Mandatory = $true)]
$DOTNET_REMOTE_PATH,
[parameter(Mandatory = $true)]
$DOTNET_LOCAL_PATH,
[parameter(Mandatory = $true)]
$DOTNET_PATH)

$retryCount = 0;
$success = $false;

do {
     try {
          Write-Output "Downloading from $DOTNET_REMOTE_PATH"
          (New-Object Net.WebClient).DownloadFile($DOTNET_REMOTE_PATH, $DOTNET_LOCAL_PATH);
          $success = $true;
        } catch { 
            if ($retryCount -ge 6) {
                Write-Output "Maximum of 5 retries exceeded. Aborting"
                throw;
            } 
            else { 
                $retryCount++;
                $retryTime = 5 * $retryCount;
                Write-Output "Download failed. Retrying in $retryTime seconds"
                Start-Sleep -Seconds (5 * $retryCount);
            }
        }
    }
while ($success -eq $false);

Write-Output "Download finished"
Add-Type -Assembly 'System.IO.Compression.FileSystem' -ErrorVariable AddTypeErrors; 

if ($AddTypeErrors.Count -eq 0) { 
    [System.IO.Compression.ZipFile]::ExtractToDirectory($DOTNET_LOCAL_PATH, $DOTNET_PATH) 
}
else { 
    (New-Object -com shell.application).namespace($DOTNET_PATH).CopyHere((new-object -com shell.application).namespace($DOTNET_LOCAL_PATH).Items(), 16) 
}

$localNugetReposetory = $env:LocalNugetReposetory

if ($localNugetReposetory -eq $null) {
    $localNugetReposetory = '~\Source\LocalNugetRepo'
}

if (Test-Path $localNugetReposetory) {
    Write-Host "Copy Files To $localNugetReposetory"
    Get-ChildItem -Directory | Where-Object {$_.Name -ne 'packages'} | Get-ChildItem -Filter *.nupkg  -Recurse | Copy-Item -Destination $localNugetReposetory -Verbose
    Get-ChildItem -Directory | Where-Object {$_.Name -ne 'packages'} | Get-ChildItem -Filter *.nupkg  -Recurse | Remove-Item -Verbose

    $nugetLocation = $nugetLocationOutput.SubString('global-packages:'.Length).Trim()

    if (Test-Path $nugetLocation) {
        Write-Host "Remove folder from local nuget $nugetLocation"
        Get-ChildItem $nugetLocation -Directory -Filter NDProperty* | Remove-Item -Recurse -Force -Verbose
    }

}
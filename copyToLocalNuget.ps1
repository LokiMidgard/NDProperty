


Get-ChildItem -Directory | Where-Object {$_.Name -ne 'packages'} | Get-ChildItem -Filter *.nupkg  -Recurse | Copy-Item -Destination '~\Source\LocalNugetRepo'
Get-ChildItem -Directory | Where-Object {$_.Name -ne 'packages'} | Get-ChildItem -Filter *.nupkg  -Recurse | Remove-Item
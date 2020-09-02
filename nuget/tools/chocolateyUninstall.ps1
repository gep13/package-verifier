$ErrorActionPreference = 'Stop'
$serviceName = 'package-verifier'

$binPath = Join-Path (Get-ToolsLocation) -ChildPath 'chocolatey-package-verifier'

Uninstall-ChocolateyWindowsService -Name $serviceName

Write-Warning "Any files stored in the service install path, '$binPath', will have to be removed manually."
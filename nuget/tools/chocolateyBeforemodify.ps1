$ErrorActionPreference = 'Stop'
$serviceName = 'package-verifier'

$pp = Get-PackageParameters
if (-not $pp.DoNotRestartService) {
  Stop-ChocolateyWindowsService -Name $serviceName
}

$ErrorActionPreference = 'Stop'

$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$serviceExe = 'package-verifier.exe'
$destinationPath = Join-Path (Get-ToolsLocation) -ChildPath 'chocolatey-package-verifier'
$configPath = Join-Path -Path $destinationPath -ChildPath 'package-verifier.exe.config'

$serviceParams = @{
  Name                  = 'package-verifier'
  DisplayName           = 'Chocolatey Package Verifier'
  Description           = 'Verifies newly submitted packages to Chocolatey.org.'
  StartupType           = 'Automatic'
  ServiceExecutablePath = Join-Path -Path $destinationPath -ChildPath $serviceExe
  DoNotReinstallService = $true
}

$pp = Get-PackageParameters

$isUpgrade = [bool](Get-Service -Name $serviceParams.Name -ErrorAction SilentlyContinue)
if (($isUpgrade -and $pp.ReinstallService) -or (-not $isUpgrade)) {
  if ($isUpgrade) {
    Write-Warning 'Service will be reinstalled after upgrade. This may cause issues and require a reboot and package upgrade run again.'
  }

  $serviceParams.DoNotReinstallService = $false
}

if ($pp.DoNotStartService) {
  Write-Warning 'Service will not be started after installation. This will need to be done manually.'
  $serviceParams.DoNotStartService = $true
}

if (-not $isUpgrade) {
  # check we have the parameters required
  if (-not ($pp.Username -and ($pp.Password -or $pp.EnterPassword))) {
    throw "You need to supply a username and password for the service to run under."
  }

  if ($pp.Username) {
    $serviceParams.Username = $pp.Username
  }

  if ($pp.Password) {
    $serviceParams.Password = $pp.Password
  }

  if ($pp.EnterPassword) {
    $serviceParams.Password = Read-Host "Enter password for $($packageArgs.Username):" -AsSecureString
  }
}

if (-not (Test-Path -Path $configPath)) {
  Write-Warning "Cannot find config file '$configPath'. Service will not be started after installation. This will need to be done manually if you do not reboot."
  $serviceParams.DoNotStartService = $true
}

$sourcePath = (Join-Path -Path $toolsDir -ChildPath "files")
Write-Verbose "Copying files from '$sourcePath' to '$destinationPath'"
Copy-Item -Path "$sourcePath\*" -Destination $destinationPath -Recurse -Force

Write-Verbose "Removing package files from '$sourcePath'"
Remove-Item -Path $sourcePath -Recurse -Force

# If the service is already installed this should simply restart it unless the
# $serviceParams.DoNotReinstallService is false in which case it will reinstall the service.
Install-ChocolateyWindowsService @serviceParams
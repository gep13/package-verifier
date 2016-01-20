$ChocoInstallPath = "$env:SystemDrive\ProgramData\Chocolatey\bin"

if (!(Test-Path $ChocoInstallPath)) {
  # Install chocolatey
  iex ((new-object net.webclient).DownloadString('https://chocolatey.org/installabsolutelatest.ps1'))
}

#Update-SessionEnvironment

choco feature enable -n autouninstaller
choco feature enable -n allowGlobalConfirmation

$localDirectory = (Split-Path -Parent $MyInvocation.MyCommand.Definition);

& $localDirectory\InstallNet4.ps1
& $localDirectory\InstallChocolatey.ps1
& $localDirectory\NotifyGuiAppsOfEnvironmentChanges.ps1
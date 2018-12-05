ls

if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) 
{ 
    "Run as Administrator"
    Start-Process powershell.exe "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit 
}

$devEnvPath = 'C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe'
& $devEnvPath ./source/as4.sln /Rebuild Release

while (!(Test-Path ./output/Eu.EDelivery.AS4.WindowsService.Installer.msi)) { Start-Sleep -Seconds 5 }
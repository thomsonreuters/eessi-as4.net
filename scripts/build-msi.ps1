$devEnvPath = 'C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe'

Start-Process $devEnvPath -ArgumentList "./source/as4.sln /Project ./source/eu.edelivery.as4.windowsservice.installer/eu.edelivery.as4.windowsservice.installer.vdproj /Rebuild Release /Out errors.txt" -Verb RunAs

while (!(Test-Path ./output/Eu.EDelivery.AS4.WindowsService.Installer.msi)) { Start-Sleep -Seconds 5 }

Start-Sleep -Seconds 10
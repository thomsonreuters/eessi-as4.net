#cd $env:BUILD_ARTIFACTstagingDIRECTORY

cd ..\output

if (Test-Path -Path .\staging) {
    Remove-Item  -Path .\staging -ErrorAction SilentlyContinue -Force
}

MkDir .\staging
MkDir .\staging\Assets
MkDir .\staging\bin
MkDir .\staging\service-setup
MkDir .\staging\config
MkDir .\staging\config\send-pmodes
MkDir .\staging\config\receive-pmodes
MkDir .\staging\documentation
MkDir .\staging\documentation\Schemas
MkDir .\staging\database
MkDir .\staging\logs
MkDir .\staging\messages
MkDir .\staging\messages\attachments
MkDir .\staging\messages\errors
MkDir .\staging\messages\exceptions
MkDir .\staging\messages\in
MkDir .\staging\messages\out
MkDir .\staging\messages\receipts
MkDir .\staging\samples
MkDir .\staging\samples\certificates
MkDir .\staging\samples\messages
MkDir .\staging\samples\pmodes
MkDir .\staging\samples\pmodes\eessi
MkDir .\staging\bin\x64
MkDir .\staging\bin\x86

Remove-Item .\*.pdb
Remove-Item .\*.xml -exclude Eu.EDelivery.AS4.Fe.xml, Eu.EDelivery.AS4.PayloadService.xml
Remove-Item .\xunit*.*
If (Test-Path .\users.sqlite ) {
    Remove-Item .\users.sqlite
}
Remove-Item .\Microsoft.VisualStudio.Quality*.*
Remove-Item .\Moq.*
Remove-Item .\*Tests.*
Remove-Item .\*TestUtils.*

Copy-Item -Path .\assets\*.* .\staging\assets\
Copy-Item -Path .\Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe .\staging\
Copy-Item -Path .\Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe.config .\staging\
Copy-Item -Path .\Eu.EDelivery.AS4.Fe.exe .\staging\
Copy-Item -Path .\Eu.EDelivery.AS4.Fe.exe.config .\staging\
Copy-Item -Path .\Eu.EDelivery.AS4.Fe.xml .\staging\bin
Copy-Item -Path .\Eu.EDelivery.AS4.PayloadService.exe .\staging\bin\
Copy-Item -Path .\Eu.EDelivery.AS4.PayloadService.exe.config .\staging\bin\
Copy-Item -Path .\Eu.EDelivery.AS4.PayloadService.xml .\staging\bin\
Copy-Item -Path .\appsettings.payloadservice.json .\staging\bin\
Copy-Item -Path .\Eu.EDelivery.AS4.WindowsService.exe .\staging\
Copy-Item -Path .\Eu.EDelivery.AS4.WindowsService.exe.config .\staging\
Copy-Item -Path .\Eu.EDelivery.AS4.WindowsService.SystemTray.exe .\staging\bin\
Copy-Item -Path .\Eu.EDelivery.AS4.WindowsService.SystemTray.exe.config .\staging\bin\
Copy-Item -Path ".\doc\AS4.NET - online documentation.url" .\staging\documentation\

If (Test-Path .\Eu.EDelivery.AS4.dll.config) {
    Move-Item -Path .\Eu.EDelivery.AS4.dll.config .\staging\bin\
}

$excludedLibraries = @("ModuleInit.dll", "NSubstitute.dll", "SimpleHttpMock.dll", "FsCheck.dll", "FsCheck.Xunit.dll", "FSharp.Core.dll")

Copy-Item -Exclude $excludedLibraries -Path .\*.dll -Destination .\staging\bin
Copy-Item -Path .\x86\*.* -Destination .\staging\bin\x86\
Copy-Item -Path .\x64\*.* -Destination .\staging\bin\x64\
Copy-Item -Path .\appsettings.inprocess.json .\staging\bin\
Copy-Item -Path .\appsettings.json .\staging\bin\
Copy-Item -Path .\config\settings.xml .\staging\config\
Copy-Item -Path .\config\settings-service.xml .\staging\config\
Copy-Item -Path .\messages\attachments\*.* .\staging\messages\attachments\
Copy-Item -Path .\samples\certificates\*.* .\staging\samples\certificates\
Copy-Item -Path .\samples\messages\*.* .\staging\samples\messages\
Copy-Item -Path .\samples\pmodes\*.* .\staging\samples\pmodes\
Copy-Item -Path .\samples\pmodes\eessi\*.* .\staging\samples\pmodes\eessi\
Copy-Item -Path .\install-windows-service.bat .\staging\service-setup
Copy-Item -Path .\uninstall-windows-service.bat .\staging\service-setup
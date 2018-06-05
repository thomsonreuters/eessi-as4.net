#cd $env:BUILD_ARTIFACTSTAGINGDIRECTORY

cd ..\output

MkDir .\Staging
MkDir .\Staging\Assets
MkDir .\Staging\bin
MkDir .\Staging\service-setup
MkDir .\Staging\config
MkDir .\Staging\config\send-pmodes
MkDir .\Staging\config\receive-pmodes
MkDir .\Staging\documentation
MkDir .\Staging\documentation\Schemas
MkDir .\Staging\database
MkDir .\Staging\logs
MkDir .\Staging\messages
MkDir .\Staging\messages\attachments
MkDir .\Staging\messages\errors
MkDir .\Staging\messages\exceptions
MkDir .\Staging\messages\in
MkDir .\Staging\messages\out
MkDir .\Staging\messages\receipts
MkDir .\Staging\samples
MkDir .\Staging\samples\certificates
MkDir .\Staging\samples\messages
MkDir .\Staging\samples\pmodes
MkDir .\Staging\samples\pmodes\eessi
MkDir .\Staging\bin\x64
MkDir .\Staging\bin\x86

Remove-Item .\*.pdb
Remove-Item .\*.xml -exclude Eu.EDelivery.AS4.Fe.xml,Eu.EDelivery.AS4.PayloadService.xml
Remove-Item .\xunit*.*
If (Test-Path .\users.sqlite ) {
	Remove-Item .\users.sqlite
}
Remove-Item .\Microsoft.VisualStudio.Quality*.*
Remove-Item .\Moq.*
Remove-Item .\*Tests.*
Remove-Item .\*TestUtils.*

Copy-Item -Path .\Assets\*.* .\Staging\Assets\
Copy-Item -Path .\Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe .\Staging\
Copy-Item -Path .\Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe.config .\Staging\
Copy-Item -Path .\Eu.EDelivery.AS4.Fe.exe .\Staging\bin\
Copy-Item -Path .\Eu.EDelivery.AS4.Fe.exe.config .\Staging\bin\
Copy-Item -Path .\Eu.EDelivery.AS4.Fe.xml .\Staging\bin\
Copy-Item -Path .\Eu.EDelivery.AS4.PayloadService.exe .\Staging\bin\
Copy-Item -Path .\Eu.EDelivery.AS4.PayloadService.xml .\Staging\bin\
Copy-Item -Path .\appsettings.payloadservice.json .\Staging\bin\
Copy-Item -Path .\Eu.EDelivery.AS4.WindowsService.exe .\Staging\
Copy-Item -Path .\Eu.EDelivery.AS4.WindowsService.exe.config .\Staging\
Copy-Item -Path ".\doc\AS4.NET - online documentation.url" .\Staging\documentation\

If (Test-Path .\Eu.EDelivery.AS4.dll.config) {
	Move-Item -Path .\Eu.EDelivery.AS4.dll.config .\Staging\bin\
}

$excludedLibraries = @("ModuleInit.dll", "NSubstitute.dll", "SimpleHttpMock.dll", "FsCheck.dll", "FsCheck.Xunit.dll", "FSharp.Core.dll")

Copy-Item -Exclude $excludedLibraries -Path .\*.dll -Destination .\Staging\bin
Copy-Item -Path .\x86\*.* -Destination .\Staging\bin\x86\
Copy-Item -Path .\x64\*.* -Destination .\Staging\bin\x64\
Copy-Item -Path .\appsettings.inprocess.json .\Staging\bin\
Copy-Item -Path .\appsettings.json .\Staging\bin\
Copy-Item -Path .\config\settings.xml .\Staging\config\
Copy-Item -Path .\config\settings-service.xml .\Staging\config\
Copy-Item -Path .\messages\attachments\*.* .\Staging\messages\attachments\
Copy-Item -Path .\samples\certificates\*.* .\Staging\samples\certificates\
Copy-Item -Path .\samples\messages\*.* .\Staging\samples\messages\
Copy-Item -Path .\samples\pmodes\*.* .\Staging\samples\pmodes\
Copy-Item -Path .\samples\pmodes\eessi\*.* .\Staging\samples\pmodes\eessi\
Copy-Item -Path .\install-windows-service.bat .\Staging\service-setup
Copy-Item -Path .\uninstall-windows-service.bat .\Staging\service-setup
#cd $env:BUILD_ARTIFACTSTAGINGDIRECTORY

cd ..\output

MkDir .\Staging
MkDir .\Staging\Assets
MkDir .\Staging\bin
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
MkDir .\Staging\x64
MkDir .\Staging\x86

Remove-Item .\*.pdb
Remove-Item .\*.xml
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
Copy-Item -Path .\Eu.EDelivery.AS4.Fe.exe.xml .\Staging\bin\
Copy-Item -Path .\Eu.EDelivery.AS4.PayloadService.exe .\Staging\bin\


If (Test-Path .\Eu.EDelivery.AS4.dll.config) {
	Move-Item -Path .\Eu.EDelivery.AS4.dll.config .\Staging\bin\
}

Copy-Item -Path .\*.dll -Destination .\Staging\bin
Copy-Item -Path .\x86\*.* -Destination .\Staging\x86\
Copy-Item -Path .\x64\*.* -Destination .\Staging\x64\
Copy-Item -Path .\appsettings.inprocess.json .\Staging\bin\
Copy-Item -Path .\appsettings.json .\Staging\bin\
Copy-Item -Path .\config\settings.xml .\Staging\config\
Copy-Item -Path .\messages\attachments\*.* .\Staging\messages\attachments\
Copy-Item -Path .\samples\certificates\*.* .\Staging\samples\certificates\
Copy-Item -Path .\samples\messages\*.* .\Staging\samples\messages\
Copy-Item -Path .\samples\pmodes\*.* .\Staging\samples\pmodes\
Copy-Item -Path .\samples\pmodes\eessi\*.* .\Staging\samples\pmodes\eessi\

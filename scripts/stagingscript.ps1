#cd $env:BUILD_ARTIFACTSTAGINGDIRECTORY

cd ..\output

MkDir .\Staging
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
MkDir .\Staging\x64
MkDir .\Staging\x86

Remove-Item .\*.pdb
Remove-Item .\*.xml
Remove-Item .\xunit*.*
Remove-Item .\users.sqlite
Remove-Item .\Microsoft.VisualStudio.Quality*.*
Remove-Item .\Moq.*
Remove-Item .\*UnitTests.*

Move-Item -Path .\Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe .\Staging\
Move-Item -Path .\Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe.config .\Staging\
Move-Item -Path .\Eu.EDelivery.AS4.Fe.exe .\Staging\bin\

Move-Item -Path .\*.dll -Destination .\Staging\bin
Move-Item -Path .\x86\*.* -Destination .\Staging\x86\
Move-Item -Path .\x64\*.* -Destination .\Staging\x64\
Move-Item -Path .\appsettings.inprocess.json .\Staging\bin\
Move-Item -Path .\appsettings.json .\Staging\bin\
Move-Item -Path .\Eu.EDelivery.AS4.dll.config .\Staging\bin\
Move-Item -Path .\config\settings.xml .\Staging\config\
Move-Item -Path .\messages\attachments\*.* .\Staging\messages\attachments\
Move-Item -Path .\samples\certificates\*.* .\Staging\samples\certificates\
Move-Item -Path .\samples\messages\*.* .\Staging\samples\messages\
Move-Item -Path .\samples\pmodes\*.* .\Staging\samples\pmodes\

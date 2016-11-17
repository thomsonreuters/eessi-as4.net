# Moves the assemblies to a new 'bin' created folder
# NOTE: This script should be run on the build server

cd $env:BUILD_ARTIFACTSTAGINGDIRECTORY

New-Item -Path .\bin -ItemType Directory
Move-Item -Path .\*.dll -Destination .\bin
Remove-Item .\*.pdb
Remove-Item .\*.xml

Get-ChildItem -Path .\ -Recurse -Include FluentValidation.resources.dll | % { Remove-Item $_.Directory.FullName -Force -Recurse }

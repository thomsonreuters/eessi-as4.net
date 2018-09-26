dotnet restore .\source\PayloadService\Eu.EDelivery.AS4.PayloadService.csproj

$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe'
& $msbuild '.\source\AS4.sln' /t:Rebuild /p:Configuration=Release /nologo /nr:false /verbosity:minimal

Set-Location "./source/Fe/Eu.EDelivery.AS4.FE/ui"
npm install
npm run build:aot:prod
Set-Location "..\..\..\..\"

& './scripts/VersionAssemblies.ps1'

Set-Location output
& '../scripts/add-probing.ps1'
& '../scripts/stagingscript.ps1'
Set-Location ..

Set-Location "source/FE/Eu.EDelivery.AS4.Fe/ui"
npm run copytooutput    
Set-Location "..\..\..\..\"

Set-Location "./scripts/"
& './GenerateXsd.ps1'
Set-Location ".."
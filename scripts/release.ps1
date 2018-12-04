Set-Location "./source/Eu.EDelivery.AS4.FE/ui"
npm install
npm run build:aot:prod
npm run copytooutput    
Set-Location "..\..\..\"

dotnet restore .\source\Eu.EDelivery.AS4.PayloadService\Eu.EDelivery.AS4.PayloadService.csproj

& './scripts/VersionAssemblies.ps1'

$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe'
& $msbuild '.\source\AS4.sln' /t:Rebuild /p:Configuration=Release /nologo /nr:false /verbosity:minimal

$devEnvPath = 'C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe'
$solutionPath = "./source/AS4.sln"
$projectPath = "./source/Eu.EDelivery.AS4.WindowsService.Installer/Eu.EDelivery.AS4.WindowsService.Installer.vdproj"
$parameters = "/Rebuild Release " + $solutionPath + " /Project " + $projectPath + " /ProjectConfig Release /Out errors.txt"
"Process to start [$devEnvPath $parameters]"
$process = [System.Diagnostics.Process]::Start($devEnvPath, $parameters)
$process.WaitForExit()
if (Test-Path errors.txt) {
    Get-Content errors.txt
}

Set-Location output
& '../scripts/add-probing.ps1'
& '../scripts/stagingscript.ps1'
Set-Location ..

Set-Location "./scripts/"
& './GenerateXsd.ps1'
Set-Location ".."
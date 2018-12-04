function build-msi (
    [string] $solutionPath, 
    [string] $projectPath, 
    [string] $devEnvPath)
{
    $parameters = "/Rebuild Release " + $solutionPath + " /Project " + $projectPath + " /ProjectConfig Release /Out errors.txt"
    "Process to start [$devEnvPath $parameters]"
    $process = [System.Diagnostics.Process]::Start($devEnvPath, $parameters)
    $process.WaitForExit()

    Get-Content -Path errors.txt
}

build-msi "./source/AS4.sln" "./source/Eu.EDelivery.AS4.WindowsService.Installer/Eu.EDelivery.AS4.WindowsService.Installer.vdproj" "C:/Program Files (x86)/Microsoft Visual Studio/2017/Enterprise/Common7/IDE/devenv.exe"

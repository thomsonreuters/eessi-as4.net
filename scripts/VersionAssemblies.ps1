# Versions the assemblies according to the BUILD_BUILDNUMBER variable
# NOTE: This script should be run on the build server
# BUILDNUMBER is major.minor.patch-prerelease.build

$versionRegex = "(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)[^\.]*\.(?<build>\d+)"
$buildNumber = $env:BUILD_BUILDNUMBER
if ($buildNumber -match $versionRegex -eq $true)
{
  $version = $matches["major"] + '.' + $matches["minor"] + '.' + $matches["patch"] + '.' + $matches["build"]
  Write-Host "Setting version to $version"
  Get-ChildItem -Recurse *AssemblyInfo* | % {
    Write-Host "Modifying $_.FullName"
    $content = Get-Content $_.FullName
    $content = $content -replace "\d+\.\d+\.\d+\.\d+",$version
    $content | Out-File $_.FullName -Force
  }
}
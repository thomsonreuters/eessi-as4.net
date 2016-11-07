Push-Location $PSScriptRoot

$xsdPath = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\xsd.exe"
$xmlPath = Join-Path $PSScriptRoot "..\..\Xml"

& $xsdPath /p:update-schemas.options.xml #/out:$xmlPath

mv *.cs (Join-Path $xmlPath Generated.cs) -Force
#mv $modelPath\Pmode.cs $modelPath\ProcessingMode.cs -Force

Pop-Location
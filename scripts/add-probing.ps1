$consoleHostConfig = "..\output\Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe.config"

$consoleHostXml = [xml](Get-Content $consoleHostConfig)
$consoleHostXml.configuration.runtime.InnerXml = '<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1"><probing privatePath="bin;"/></assemblyBinding>' + $consoleHostXml.configuration.runtime.InnerXml

$consoleHostXml.Save((Resolve-Path $consoleHostConfig))

$windowsServiceConfig = "..\output\Eu.EDelivery.AS4.WindowsService.exe.config"

$windowsServiceXml = [xml](Get-Content $windowsServiceConfig)
$windowsServiceXml.configuration.runtime.InnerXml = '<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1"><probing privatePath="bin;"/></assemblyBinding>' + $windowsServiceXml.configuration.runtime.InnerXml

$windowsServiceXml.Save((Resolve-Path $windowsServiceConfig))
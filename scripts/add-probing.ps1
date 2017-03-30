$configFile = "..\output\Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe.config"

$xml = [xml](Get-Content $configFile)
$xml.configuration.runtime.InnerXml = '<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1"><probing privatePath="bin;"/></assemblyBinding>' + $xml.configuration.runtime.InnerXml

$xml.Save((Resolve-Path $configFile))
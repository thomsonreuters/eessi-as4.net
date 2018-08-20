$ipV4 = Test-Connection -ComputerName (hostname) -Count 1  | Select -ExpandProperty IPV4Address
cd C:\agent\_work\1\s\ServiceHandler\Eu.EDelivery.AS4.IntegrationTests\bin\Release\config\pmodes

foreach($pmode in Get-ChildItem) {
$pmodeXml = [xml](Get-Content $pmode.FullName)
$pmodeXml.PMode.PushConfiguration.Protocol.Url = "http://$($ipV4.IPAddressToString.ToString()):9090"
$pmodeXml.Save($pmode.FullName)
}
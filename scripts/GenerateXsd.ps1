
$cmd = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\xsd.exe"
$output = Get-Location ../output/Staging/Documentation/Schemas

& $cmd ../output/Staging/bin/Eu.EDelivery.AS4.dll  /type:Eu.EDelivery.AS4.Model.Submit.SubmitMessage /o:../output/Staging/Documentation/Schemas
Rename-Item ../output/Staging/Documentation/Schemas/schema0.xsd submitmessage-schema.xsd

& $cmd ../output/Staging/bin/Eu.EDelivery.AS4.dll  /type:Eu.EDelivery.AS4.Model.Deliver.DeliverMessage /o:../output/Staging/Documentation/Schemas
Rename-Item ../output/Staging/Documentation/Schemas/schema0.xsd delivermessage-schema.xsd

& $cmd ../output/Staging/bin/Eu.EDelivery.AS4.dll  /type:Eu.EDelivery.AS4.Model.Notify.NotifyMessage /o:../output/Staging/Documentation/Schemas
Rename-Item ../output/Staging/Documentation/Schemas/schema0.xsd notifymessage-schema.xsd

& $cmd ../output/Staging/bin/Eu.EDelivery.AS4.dll  /type:Eu.EDelivery.AS4.Model.PMode.SendingProcessingMode /o:../output/Staging/Documentation/Schemas
Rename-Item ../output/Staging/Documentation/Schemas/schema0.xsd send-pmode-schema.xsd

& $cmd ../output/Staging/bin/Eu.EDelivery.AS4.dll  /type:Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode /o:../output/Staging/Documentation/Schemas
Rename-Item ../output/Staging/Documentation/Schemas/schema0.xsd receive-pmode-schema.xsd

Get-ChildItem $output -Filter '*.xsd' | % {
        $content = [xml](Get-Content $_.FullName)
     
        Select-Xml $content -XPath "//*[local-name()='any']" | % {
            $element = [System.Xml.XmlElement]$_.Node
            $element.SetAttribute("processContents", "lax")
        }

        $content.Save($_.FullName)
    }
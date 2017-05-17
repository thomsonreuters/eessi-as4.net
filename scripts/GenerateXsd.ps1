
$cmd = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\xsd.exe"

# Export new XSD files from the build assembly
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

# Update each 'any' element with the 'processContents' attribute set to 'lax'
Get-ChildItem ../output/Staging/Documentation/Schemas -Filter '*.xsd' | % {
        $content = [xml](Get-Content $_.FullName)
     
        Select-Xml $content -XPath "//*[local-name()='any']" | % {
            $element = [System.Xml.XmlElement]$_.Node
            $element.SetAttribute("processContents", "lax")
        }

        $content.Save($_.FullName)
    }

# Copy the new exported schemas to the current documentation folder
Copy-Item -Path ../output/Staging/Documentation/Schemas/submitmessage-schema.xsd -Destination ../output/doc/schemas/submitmessage-schema.xsd -Force
Copy-Item -Path ../output/Staging/Documentation/Schemas/delivermessage-schema.xsd -Destination ../output/doc/schemas/delivermessage-schema.xsd -Force
Copy-Item -Path ../output/Staging/Documentation/Schemas/notifymessage-schema.xsd -Destination ../output/doc/schemas/notifymessage-schema.xsd -Force
Copy-Item -Path ../output/Staging/Documentation/Schemas/send-pmode-schema.xsd -Destination ../output/doc/schemas/send-pmode-schema.xsd -Force
Copy-Item -Path ../output/Staging/Documentation/Schemas/receive-pmode-schema.xsd -Destination ../output/doc/schemas/receive-pmode-schema.xsd -Force
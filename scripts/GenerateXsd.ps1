param([string]$binDirectory = "../output/Staging/bin", [string]$outputDirectory = "../output/Staging/Documentation/Schemas")

$cmd = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\xsd.exe"

# Export new XSD files from the build assembly
& $cmd $binDirectory/Eu.EDelivery.AS4.dll  /type:Eu.EDelivery.AS4.Model.Submit.SubmitMessage /o:$outputDirectory
Move-Item $outputDirectory/schema0.xsd submitmessage-schema.xsd -Force

& $cmd $binDirectory/Eu.EDelivery.AS4.dll  /type:Eu.EDelivery.AS4.Model.Deliver.DeliverMessage /o:$outputDirectory
Move-Item $outputDirectory/schema0.xsd delivermessage-schema.xsd -Force

& $cmd $binDirectory/Eu.EDelivery.AS4.dll  /type:Eu.EDelivery.AS4.Model.Notify.NotifyMessage /o:$outputDirectory
Move-Item $outputDirectory/schema0.xsd notifymessage-schema.xsd -Force

& $cmd $binDirectory/Eu.EDelivery.AS4.dll  /type:Eu.EDelivery.AS4.Model.PMode.SendingProcessingMode /o:$outputDirectory
Move-Item $outputDirectory/schema0.xsd send-pmode-schema.xsd -Force

& $cmd $binDirectory/Eu.EDelivery.AS4.dll  /type:Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode /o:$outputDirectory
Move-Item $outputDirectory/schema0.xsd receive-pmode-schema.xsd -Force

# Update each 'any' element with the 'processContents' attribute set to 'lax'
Get-ChildItem $outputDirectory -Filter '*.xsd' | % {
        $content = [xml](Get-Content $_.FullName)
     
        Select-Xml $content -XPath "//*[local-name()='any']" | % {
            $element = [System.Xml.XmlElement]$_.Node
            $element.SetAttribute("processContents", "lax")
        }        
        
        $content.Save($_.FullName)
    }

# Copy the new exported schemas to the current documentation folder
Copy-Item -Path $outputDirectory/submitmessage-schema.xsd -Destination ../output/doc/schemas/submitmessage-schema.xsd -Force
Copy-Item -Path $outputDirectory/delivermessage-schema.xsd -Destination ../output/doc/schemas/delivermessage-schema.xsd -Force
Copy-Item -Path $outputDirectory/notifymessage-schema.xsd -Destination ../output/doc/schemas/notifymessage-schema.xsd -Force
Copy-Item -Path $outputDirectory/send-pmode-schema.xsd -Destination ../output/doc/schemas/send-pmode-schema.xsd -Force
Copy-Item -Path $outputDirectory/receive-pmode-schema.xsd -Destination ../output/doc/schemas/receive-pmode-schema.xsd -Force
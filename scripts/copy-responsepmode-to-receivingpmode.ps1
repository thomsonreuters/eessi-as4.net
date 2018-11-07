# Copies the signing and push-configuration from response pmodes into the receiving pmodes

param(
    [string]$sendingPModePath = ".\config\send-pmodes\",
    [string]$receivingPModePath = ".\config\receive-pmodes\"
)

if (!(Test-Path -Path $sendingPModePath)) {
    throw "Sending PMode path doesn't exists" 
}

if (!(Test-Path -Path $receivingPModePath)) {
    throw "Receiving PMode path doesn't exists"
}

Write-Host "Use Sending PModes at: $($sendingPModePath)" -ForegroundColor Blue
Write-Host "Use Receiving PModes at: $($receivingPModePath)" -ForegroundColor Blue

function Get-PModes ($path) {
    $table = @{}
    Get-ChildItem -File -Path $path | 
        ForEach-Object { 
        [xml] $xml = Get-Content $_.FullName
        $id = $xml.PMode.Id
        if ($null -ne $id) {
            Write-Host "Found PMode at $($_.FullName) with Id: $($id)" -ForegroundColor DarkGray
            $table.Add($id, $xml)
        } 
    }
    return $table
}

function Copy-Sending-To-Receiving ($sendingPMode, $xml) {
    $signing = $sendingPMode.PMode.Security.Signing
    if ($null -ne $signing) {
        $adapted = $xml.CreateElement("ResponseSigning", "eu:edelivery:as4:pmode")
        $signing.ChildNodes |
            ForEach-Object { 
            $adapted.AppendChild($xml.ImportNode($_, $true)) | Out-Null }
        $xml.PMode.ReplyHandling.AppendChild($adapted) | Out-Null
    }

    $pushConfig = $sendingPMode.PMode.PushConfiguration
    if ($null -ne $pushConfig) {
        $adapted = $xml.CreateElement("ResponseConfiguration", "eu:edelivery:as4:pmode")
        $pushConfig.ChildNodes |
            Foreach-Object { $adapted.AppendChild($xml.ImportNode($_, $true)) | Out-Null }
        $xml.PMode.ReplyHandling.AppendChild($adapted) | Out-Null
    }
}

$sendingPModes = Get-PModes $sendingPModePath

Get-ChildItem -File -Path $receivingPModePath |
    ForEach-Object { 
    [xml] $xml = Get-Content $_.FullName
    $responsePModeRef = $xml.PMode.ReplyHandling.SendingPMode
    if ($null -ne $responsePModeRef) {
        Write-Host "Receiving PMode at $($_.FullName) as reference to Sending PMode $($responsePModeRef)" -ForegroundColor DarkGray
        $xml.Save($_.FullName + ".backup")

        Write-Host "Include Sending PMode $($responsePModeRef) > $($_.FullName)"
        $sendingPMode = $sendingPModes.Item($responsePModeRef)
        Copy-Sending-To-Receiving $sendingPMode $xml

        $xml.Save($_.FullName)
    }
}


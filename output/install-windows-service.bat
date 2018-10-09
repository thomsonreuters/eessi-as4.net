if exist %WINDIR%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe (
	if exist .\..\Eu.EDelivery.AS4.WindowsService.exe (
		%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe .\..\Eu.EDelivery.AS4.WindowsService.exe	
	) else (
		%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe .\Eu.EDelivery.AS4.WindowsService.exe
	) 
) else (
	if exist .\..\Eu.EDelivery.AS4.WindowsService.exe (
		%WINDIR%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe .\..\Eu.EDelivery.AS4.WindowsService.exe
	) else (
		%WINDIR%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe .\Eu.EDelivery.AS4.WindowsService.exe
	)
)

if exist .\Eu.EDelivery.AS4.WindowsService.SystemTray.exe (
	.\Eu.EDelivery.AS4.WindowsService.SystemTray.exe
) else (
	if exist ..\bin\Eu.EDelivery.AS4.WindowsService.SystemTray.exe (
	..\bin\Eu.EDelivery.AS4.WindowsService.SystemTray.exe) 
)


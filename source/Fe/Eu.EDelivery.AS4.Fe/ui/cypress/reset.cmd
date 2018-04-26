cd ..\..\..\..\output

del .\database\users.sqlite
copy .\samples\pmodes\03-send-pmode.xml .\config\send-pmodes\
.\Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.exe

cd ..\source\Fe\Eu.EDelivery.AS4.Fe\ui
cd ..\..\..\..\output

del .\database\users.sqlite

if exist .\security exists (
    del .\security
)

copy .\samples\pmodes\*send-pmode.xml .\config\send-pmodes\
copy .\samples\pmodes\*receive-pmode.xml .\config\receive-pmodes\

cd ..\source\Fe\Eu.EDelivery.AS4.Fe\ui
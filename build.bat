:: Compile
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" .\source\AS4.sln /verbosity:minimal

:: Run Unit Tests
cd output
..\source\ServiceHandler\packages\xunit.runner.console.2.1.0\tools\xunit.console.exe Eu.EDelivery.AS4.UnitTests.dll 
cd ..

:: Code Coverage
tools\OpenCover\OpenCover.Console.exe -target:"source\ServiceHandler\packages\xunit.runner.console.2.1.0\tools\xunit.console.exe" -targetargs:"Eu.EDelivery.AS4.UnitTests.dll" -register:user -filter:"+[*]* -[Fluent*]* -[Polly*]*" -output:coverage.xml -targetdir:"output"

:: Code Coverage Results (html)
tools\ReportGenerator\ReportGenerator.exe -reports:coverage.xml -verbosity:Error -targetdir:coverage
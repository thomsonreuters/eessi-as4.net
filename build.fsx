#r "tools/FAKE/FakeLib.dll"

open Fake
open Fake.Testing
open Fake.OpenCoverHelper

Target "Compile" (fun _ -> 
    let buildMode = getBuildParamOrDefault "buildMode" "Release"
    let setParams defaults =
        { defaults with
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            Properties =
                [
                    "Optimize", "True"
                    "DebugSymbols", "True"
                    "Configuration", buildMode
                ]
         }
    build setParams "./source/AS4.sln"
          |> DoNothing
)



Target "Unit Tests" (fun _ ->
    ["./output/Eu.EDelivery.AS4.UnitTests.dll"; "./output/Eu.EDelivery.AS4.Fe.UnitTests.dll"]
        |> xUnit2 (fun p ->
            {p with
                ShadowCopy = false;
                Parallel = ParallelMode.NoParallelization;
                HtmlOutputPath = Some("./tests.html");
                XmlOutputPath = Some("./tests.xml")})
)

Target "Coverage" (fun _ ->
    OpenCover (fun p -> 
        {p with 
            TestRunnerExePath = "./source/packages/xunit.runner.console.2.2.0/tools/xunit-console.exe";
            ExePath = "./tools/OpenCover/OpenCover.Console.exe";
            Register = RegisterType.RegisterUser;
            Filter = "+[*]*" }) "./output/Eu.EDelivery.AS4.UnitTests.dll"
)

"Compile" 
    ==> "Unit Tests"

RunTargetOrDefault "Coverage"
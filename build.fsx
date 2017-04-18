#r "tools/FAKE/FakeLib.dll"

open Fake
open Fake.Testing
open Fake.DotCover
open Fake.DotNetCli

/// <summary>
/// Compile the Solution with a 'Release' configuraiton.
/// </summary>
Target "Compile" (fun _ ->
    let buildMode = getBuildParamOrDefault "buildMode" "Release"
    let setParams defaults =
        { defaults with
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            Properties = [ "Optimize", "True"; "DebugSymbols", "True"; "Configuration", buildMode]
         }
    build setParams "./source/AS4.sln" |> DoNothing
)

let unitTestAssemblies = ["./output/Eu.EDelivery.AS4.UnitTests.dll"; "./output/Eu.EDelivery.AS4.Fe.UnitTests.dll"]
let unitTestsParams p = { p with ShadowCopy = false; Parallel = ParallelMode.All;}

/// <summary>
/// Test the 'Unit Test' assemblies.
/// </summary>
Target "UnitTests" (fun _ -> unitTestAssemblies |> xUnit2 unitTestsParams)

/// <summary>
/// Test the 'Integration Test' assemblies.
/// </summary>
Target "IntegrationTests" (fun _ -> 
    let integrationTestsParams p = 
        { p with 
            ShadowCopy = false; 
            Parallel = ParallelMode.NoParallelization;
        }
    ["./output/Eu.EDelivery.AS4.IntegrationTests.dll"] |> xUnit2 integrationTestsParams
)

/// <summary>
/// Coverage the Solution with the Unit Tests.
/// </summary>
Target "Coverage" (fun _ ->
    let dotCoverSnapShot = "./output/dotCoverSnapshot.dcvr"

    let dotCoverParams defaults = 
        { defaults with 
            TargetExecutable = "./source/packages/xunit.runner.console.2.2.0/tools/xunit.console.exe"; 
            Output = dotCoverSnapShot; 
            Filters = "-:FluentValidation"}
    unitTestAssemblies |> DotCoverXUnit2 dotCoverParams unitTestsParams


    let dotReportParams defaults = 
        { defaults with 
            Source = dotCoverSnapShot; 
            Output = "./output/dotCoverReport.html"; 
            ReportType = DotCoverReportType.Html;}
    DotCoverReport dotReportParams true
)

/// <summary>
/// Inspect the Solution with FxCop.
/// </summary>
Target "Inspect" (fun _ -> 
    !! ("./output/Eu.EDelivery.AS4**.dll") 
        |> FxCop (fun p -> 
            { p with 
                ReportFileName = "./output/FxCopResult.xml"; 
                ToolPath = "./tools/FxCop/FxCopCmd.exe"; }
            )
)

"Compile" 
==> "UnitTests" 
==> "Coverage" 
==> "Inspect"

"Compile"
==> "IntegrationTests"

RunTargetOrDefault "Inspect"
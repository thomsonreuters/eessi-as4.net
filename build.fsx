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

let testAssemblies = ["./output/Eu.EDelivery.AS4.UnitTests.dll"; "./output/Eu.EDelivery.AS4.Fe.UnitTests.dll"]
let xUnitParams p = { p with ShadowCopy = false; Parallel = ParallelMode.All;}

/// <summary>
/// Test the 'Unit Test' assemblies.
/// </summary>
Target "UnitTests" (fun _ -> testAssemblies |> xUnit2 xUnitParams)

/// <summary>
/// Coverage the Solution with the Unit Tests.
/// </summary>
Target "Coverage" (fun _ ->
    let dotCoverSnapShot = "./output/dotCoverSnapshot.dcvr"

    let dotCoverParams defaults = {defaults with TargetExecutable = "./source/packages/xunit.runner.console.2.2.0/tools/xunit.console.exe"; Output = dotCoverSnapShot; Filters = "-:FluentValidation"}
    testAssemblies |> DotCoverXUnit2 dotCoverParams xUnitParams

    let dotReportParams defaults = {defaults with Source = dotCoverSnapShot; Output = "./output/dotCoverReport.html"; ReportType = DotCoverReportType.Html;}
    DotCoverReport dotReportParams true)

/// <summary>
/// Inspect the Solution with FxCop.
/// </summary>
Target "Inspect" (fun _ -> !! ("./output/Eu.EDelivery.AS4**.dll") |> FxCop (fun p -> {p with ReportFileName = "./output/FxCopResult.xml"; ToolPath = "./tools/FxCop/FxCopCmd.exe"; }))

"Compile" 
==> "UnitTests" 
==> "Coverage" 
==> "Inspect"

RunTargetOrDefault "Inspect"
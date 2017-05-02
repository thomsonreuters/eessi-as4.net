#r "tools/FAKE/FakeLib.dll"

open System
open Fake
open Fake.Testing
open Fake.DotCover
open Fake.DotNetCli
open Fake.Git.Information

let solution = "./source/AS4.sln"

/// <summary>
/// Restore the Solution packages.
/// </summary>
Target "Restore" (fun _ ->
    let dotnetResult = ExecProcess (fun info -> 
        info.FileName <- "dotnet"
        info.Arguments <- ("restore " + solution)) (TimeSpan.FromMinutes 5.0)
    
    if dotnetResult <> 0 then trace "dotnet returned with a non-zero exit code"

    let nugetResult = ExecProcess (fun info -> 
        info.FileName <- "tools/NuGet/nuget.exe"
        info.Arguments <- ("restore -NonInteractive " + solution)) (TimeSpan.FromMinutes 5.0)

    if nugetResult <> 0 then trace "NuGet.exe returned with a non-zero exit code"
)

/// <summary>
/// Compile the Solution with a 'Release' configuration.
/// </summary>
Target "Compile" (fun _ ->
    let buildMode = getBuildParamOrDefault "buildMode" "Release"
    let setParams defaults =
        { defaults with
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            NodeReuse = false
            NoLogo = true
            Properties = 
                [ "Optimize", "True" 
                  "DebugSymbols", "True" 
                  "Configuration", buildMode
                  "VisualStudioVersion", "15.0"
                  "Platform", "any cpu" ]
            RestorePackagesFlag = true
         }
    build setParams solution |> DoNothing
)

let unitTestAssemblies = ["./output/Eu.EDelivery.AS4.UnitTests.dll"; "./output/Eu.EDelivery.AS4.Fe.UnitTests.dll"]
let unitTestsParams p = { p with ShadowCopy = false; Parallel = ParallelMode.All; XmlOutputPath = Some "./output/testResults.xml" }

/// <summary>
/// Test the 'Unit Test' assemblies.
/// </summary>
Target "UnitTests" (fun _ -> unitTestAssemblies |> xUnit2 unitTestsParams)

let longRunningTestsParams p = { p with ShadowCopy = false; Parallel = NoParallelization; TimeOut = TimeSpan.FromMinutes 30.0 }

/// <summary>
/// Test the 'Integration Test' assemblies.
/// </summary>
Target "IntegrationTests" (fun _ -> 
    ["./output/Eu.EDelivery.AS4.IntegrationTests.dll"] |> xUnit2 longRunningTestsParams
)

/// <summary>
/// Test the 'Component Test' assemblies.
/// </summary>
Target "ComponentTests" (fun _ ->
    ["./output/Eu.EDelivery.AS4.ComponentTests.dll"] |> xUnit2 longRunningTestsParams
)

/// <summary>
/// Test the 'Performance Test' assemblies.
/// </summary>
Target "PerformanceTests" (fun _ ->
    ["./output/Eu.EDelivery.AS4.PerformanceTests.dll"] |> xUnit2 longRunningTestsParams
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
            Filters = "-:FluentValidation;-:type=*.Resources*;-:*.Resources.Schemas;-:type=Eu.EDelivery.AS4.Xml*;-:type=*Tests.TestData.*"
            AttributeFilters = "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute;"}
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

/// <summary>
/// Run the Post-Release PS scripts.
/// </summary>
Target "Release" (fun _ ->
    DeleteDir "./output/Staging"
    Shell.Exec("powershell.exe", "-File ../scripts/add-probing.ps1", "./output/") |> ignore
    Shell.Exec("powershell.exe", "-File ../scripts/stagingscript.ps1", "./output/") |> ignore
    Shell.Exec("powershell.exe", "-File GenerateXsd.ps1", "./scripts/") |> ignore

    !! "./output/Staging/**/*.*"
        -- "*.zip"
        |> Zip "./output/Staging" ("./output/AS4.NET - " + (getBranchName ".") + ".zip")
)

"Restore"
==> "Compile" 
==> "UnitTests" 
==> "Coverage" 
==> "Inspect"
==> "Release"

"Compile"
==> "IntegrationTests" <=> "ComponentTests" <=> "PerformanceTests"

RunTargetOrDefault "Inspect"
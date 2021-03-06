#r @"tools/FAKE/tools/FakeLib.dll"
open System.IO
open Fake
open Fake.AssemblyInfoFile
open Fake.Git.Information
open Fake.SemVerHelper
open System

let buildMode = getBuildParamOrDefault "buildMode" "Release"
let framework = "net472"
let buildArtifactPath = FullName "./artifacts"
let packagesPath = FullName "./tools"
let assemblyVersion = "1.2.0.0"
let baseVersion = "1.2.0"

let envVersion = (environVarOrDefault "APPVEYOR_BUILD_VERSION" (baseVersion + ".0"))
let buildVersion = (envVersion.Substring(0, envVersion.LastIndexOf('.')))

let semVersion : SemVerInfo = (parse buildVersion)

let Version = semVersion.ToString()

let branch = (fun _ ->
  (environVarOrDefault "APPVEYOR_REPO_BRANCH" (getBranchName "."))
)

let FileVersion = (environVarOrDefault "APPVEYOR_BUILD_VERSION" (Version + "." + "0"))

let informationalVersion = (fun _ ->
  let branchName = (branch ".")
  let label = if branchName="master" then "" else " (" + branchName + "/" + (getCurrentSHA1 ".").[0..7] + ")"
  (FileVersion + label)
)

let nugetVersion = (fun _ ->
  let branchName = (branch ".")
  let label = if branchName="master" then "" else "-" + branchName
  let version = if branchName="master" then Version else FileVersion
  (version + label)
)

let InfoVersion = informationalVersion()
let NuGetVersion = nugetVersion()

let versionArgs = [ @"/p:Version=""" + NuGetVersion + @""""; @"/p:AssemblyVersion=""" + FileVersion + @""""; @"/p:FileVersion=""" + FileVersion + @""""; @"/p:InformationalVersion=""" + InfoVersion + @"""" ]

printfn "Using version: %s" Version

Target "Clean" (fun _ ->
  ensureDirectory buildArtifactPath

  CleanDir buildArtifactPath
)

Target "RestorePackages" (fun _ -> 
  DotNetCli.Restore (fun p -> { p with Project = @".\src\TodoHarvester" } )
)

Target "Build" (fun _ ->
  DotNetCli.Build (fun p-> { p with Project = @".\src\TodoHarvester.sln"
                                    Configuration= buildMode
                                    AdditionalArgs = versionArgs })
)

Target "Test" (fun _ ->  
  !! "src/*.Tests/*.csproj" |> Seq.toArray
  |> Array.iter ( fun project ->    
    DotNetCli.Test
      (fun p -> 
            {
              p with             
                  Configuration = buildMode
                  Project = project                  
            }) 
  )       
)

Target "Package" (fun _ ->    
    let zipFiles = CreateZip "." (buildArtifactPath @@ ("TodoHarvester." + Version + ".zip")) "" DefaultZipLevel true
    !! (sprintf "./src/TodoHarvester/bin/%s/%s/*.dll" buildMode framework)
    ++ (sprintf "./src/TodoHarvester/bin/%s/%s/*.exe" buildMode framework)
    ++ (sprintf "./src/TodoHarvester/bin/%s/%s/*.exe.config" buildMode framework)    
    |> Seq.toArray
    |> zipFiles
)

Target "Default" (fun _ ->
  trace "Build starting..."
)

"Clean"
  ==> "RestorePackages"
  ==> "Build"
  ==> "Test"  
  ==> "Package"
  ==> "Default"

RunTargetOrDefault "Default"
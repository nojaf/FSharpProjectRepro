module FSharpProjectRepro

open System
open System.IO
open FSharp.Compiler.CodeAnalysis
open NUnit.Framework

let typeCheck implementation =

        let randomName = Guid.NewGuid().ToString "N"
        let tempFolder = Path.Combine (Path.GetTempPath (), randomName)

        try
            let dirInfo = DirectoryInfo tempFolder
            dirInfo.Create ()
            let implPath = Path.Combine (tempFolder, "A.fs")
            File.WriteAllText (implPath, implementation)

            let projectOptions : FSharpProjectOptions =
                {
                    ProjectFileName = "A"
                    ProjectId = None
                    SourceFiles = [| implPath |]
                    OtherOptions = [|"--optimize+" |]
                    ReferencedProjects = [||]
                    IsIncompleteTypeCheckEnvironment = false
                    UseScriptResolutionRules = false
                    LoadTime = DateTime.Now
                    UnresolvedReferences = None
                    OriginalLoadReferences = []
                    Stamp = None
                }

            let checker = FSharpChecker.Create ()
            let result = checker.ParseAndCheckProject projectOptions |> Async.RunSynchronously
            ()
        finally
            if Directory.Exists tempFolder then
                Directory.Delete (tempFolder, true)

[<Test>]
let ``active pattern choice return type`` () =
    typeCheck
        """
module Colour

open System

let (|Red|Blue|Yellow|) b =
    match b with
    | 0 -> Red("hey", DateTime.Now)
    | 1 -> Blue(9., [| 'a' |])
    | _ -> Yellow [ 1uy ]
"""

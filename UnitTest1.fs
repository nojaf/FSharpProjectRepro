module FSharpProjectRepro

open System
open System.IO
open FSharp.Compiler.CodeAnalysis
open NUnit.Framework

let typeCheck implementation signature =

        let randomName = Guid.NewGuid().ToString "N"
        let tempFolder = Path.Combine (Path.GetTempPath (), randomName)

        try
            let dirInfo = DirectoryInfo tempFolder
            dirInfo.Create ()
            let implPath = Path.Combine (tempFolder, "A.fs")
            File.WriteAllText (implPath, implementation)
            let sigFPath = Path.Combine (tempFolder, "A.fsi")
            File.WriteAllText (sigFPath, signature)

            let projectOptions : FSharpProjectOptions =
                {
                    ProjectFileName = "A"
                    ProjectId = None
                    SourceFiles = [| sigFPath ; implPath |]
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
            Assert.IsEmpty result.Diagnostics
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
        """
module Colour

open System
val (|Red|Blue|Yellow|): b: int -> Choice<(string * DateTime), (float * char[]), byte list>
"""
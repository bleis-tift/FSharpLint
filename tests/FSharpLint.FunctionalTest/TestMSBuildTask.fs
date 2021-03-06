﻿(*
    FSharpLint, a linter for F#.
    Copyright (C) 2014 Matthew Mcveigh

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

namespace FSharpLint.FunctionalTest

module TestMSBuildTask =

    open NUnit.Framework

    let msbuildProject projectFile =
        let startInfo = System.Diagnostics.ProcessStartInfo
                                (
                                    FileName = Fake.MSBuildHelper.msBuildExe,
                                    Arguments = projectFile,
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false)

        use app = System.Diagnostics.Process.Start(startInfo)

        let output = System.Text.StringBuilder()
        
        while not app.StandardOutput.EndOfStream do
            app.StandardOutput.ReadLine() |> output.Append |> ignore

        output.ToString()
    
    [<TestFixture(Category = "Acceptance Tests")>]
    type TestMSBuildTask() =
        [<SetUp>]
        member __.CopyFSharpLintTaskFiles() = TestPackageHelper.copyFSharpLintTaskFiles "FSharpLintMSBuildTaskTest"

        [<Test>]
        member __.FunctionalTestMSBuildTask() = 
            let projectFile = TestPackageHelper.getPath @"../../../FSharpLint.FunctionalTest.TestedProject/FSharpLint.FunctionalTest.TestedProjectMSBuildTask.fsproj"

            let output = msbuildProject projectFile

            let expectedErrors =
                [ "`not (a=b)` might be able to be refactored into `a<>b`."
                  "`not (a<>b)` might be able to be refactored into `a=b`."
                  "`fun x -> x` might be able to be refactored into `id`."
                  "`not true` might be able to be refactored into `false`."
                  "`not false` might be able to be refactored into `true`."
                  "`List.fold ( + ) 0` might be able to be refactored into `List.sum`."
                  "`a<>true` might be able to be refactored into `not a`."
                  "`x=null`; suggestion: Consider using pattern matching, or if you're using F# 4 then `isNull`."
                  "`List.head (List.sort x)` might be able to be refactored into `List.min x`." ]

            let allFound = List.forall (fun x -> output.Contains(x)) expectedErrors

            let failInfo = sprintf "MSBuild output didn't contain expected lint warnings. output: %s" output
                
            Assert.IsTrue(allFound, failInfo)
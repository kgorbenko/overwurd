namespace Overwurd.Infrastructure.Tests

open System.Threading
open NUnit.Framework
open Overwurd.Infrastructure
open FsUnit

module Course =

   open Overwurd.Infrastructure.Tests.Common

   [<Test>]
    let ``Empty database means no courses`` () =
        task {
            do! setupPrerequisites |> withConnectionAsync

            let! courses = Course.getAllCoursesAsync CancellationToken.None |> withConnectionAsync

            courses |> should be Empty
        }
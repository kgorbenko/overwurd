namespace Overwurd.Infrastructure.Tests

open System
open Overwurd.Domain
open Overwurd.Domain.User
open Overwurd.Domain.Course
open Overwurd.Infrastructure.Tests.Domain
open System.Threading
open NUnit.Framework
open Overwurd.Infrastructure
open FsUnit

module Course =

    open Overwurd.Infrastructure.Tests.Database

    let unwrap (userId: UserId option): int =
        match userId with
        | Some id -> UserId.unwrap id
        | None -> failwith "Entity is expected to be persisted, but has no Id."

    [<Test>]
    let ``User has no courses`` () =
        task {
            do! prepareDatabaseAsync |> withConnectionAsync

            let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
            let user = makeUser "TestLogin123" date

            let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUser user
            do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync

            let! courses = Course.getUserCoursesAsync (unwrap user.Id) CancellationToken.None |> withConnectionAsync

            courses |> should be Empty
        }

    [<Test>]
    let ``Single course`` () =
        task {
            do! prepareDatabaseAsync |> withConnectionAsync

            let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
            let course = makeCourse "Test course" date
            let user = makeUserWithCoursesAndPredefinedPassword "TestLogin123" [course] date

            let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUser user
            do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync

            let! result = Course.getUserCoursesAsync (unwrap user.Id) CancellationToken.None |> withConnectionAsync

            let actual: Course list =
                [ { Id = course.Id.Value
                    UserId = user.Id.Value
                    CreatedAt = course.CreatedAt
                    Name = course.Name
                    Description = course.Description } ]

            result |> should equal actual
        }
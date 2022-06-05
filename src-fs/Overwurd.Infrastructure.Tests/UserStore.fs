module Overwurd.Infrastructure.Tests.UserStore

open System
open System.Threading
open NUnit.Framework
open FsUnit

open Overwurd.Domain
open Overwurd.Domain.User
open Overwurd.Infrastructure
open Overwurd.Infrastructure.Tests.Domain
open Overwurd.Infrastructure.Tests.Domain.Building
open Overwurd.Infrastructure.Tests.Common
open Overwurd.Infrastructure.Tests.Common.Utils

let unwrap (userId: UserId option): UserId =
    match userId with
    | Some id -> id
    | None -> failwith "Entity is expected to be persisted, but has no Id."

[<Test>]
let ``No user should be found by Id in empty database`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let! actual = UserStore.getUserByIdAsync (UserId 1) CancellationToken.None |> withConnectionAsync

        actual |> should equal None
    }

[<Test>]
let ``Finds single User by Id`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let user = makeUser "TestLogin123" date

        let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUser user
        do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync

        let! actual = UserStore.getUserByIdAsync (unwrap user.Id) CancellationToken.None |> withConnectionAsync

        let expected: User option =
            Some { Id = user.Id.Value
                   CreatedAt = user.CreatedAt
                   Login = user.Login
                   PasswordHash = user.PasswordHash
                   PasswordSalt = user.PasswordSalt }

        actual |> should equal expected
    }

[<Test>]
let ``No user should be found by Login in empty database`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let! actual = UserStore.getUserByNormalizedLoginAsync "TestLogin123" CancellationToken.None |> withConnectionAsync

        actual |> should equal None
    }

[<Test>]
let ``Finds single User by Login`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let login = "TestLogin123"
        let user = makeUser login date

        let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUser user
        do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync

        let! actual = UserStore.getUserByNormalizedLoginAsync "TESTLOGIN123" CancellationToken.None |> withConnectionAsync

        let expected: User option =
            Some { Id = user.Id.Value
                   CreatedAt = user.CreatedAt
                   Login = user.Login
                   PasswordHash = user.PasswordHash
                   PasswordSalt = user.PasswordSalt }

        actual |> should equal expected
    }

[<Test>]
let ``Creates User`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)

        let login = Login.create "TestLogin123"
        let parameters: UserCreationParametersForPersistence =
            { CreatedAt = CreationDate.create date
              Login = login
              NormalizedLogin = NormalizedLogin.create login
              PasswordHash = "1"
              PasswordSalt = "Some salt"}

        let! createdId = UserStore.createUserAsync parameters CancellationToken.None |> withConnectionAsync

        let! actual = Database.getAllUsersAsync |> withConnectionAsync

        let expected: UserPersistentModel list =
            [ { Id = createdId
                CreatedAt = CreationDate.unwrap parameters.CreatedAt
                Login = Login.unwrap parameters.Login
                NormalizedLogin = NormalizedLogin.unwrap parameters.NormalizedLogin
                PasswordHash = parameters.PasswordHash
                PasswordSalt = parameters.PasswordSalt } ]

        actual |> should equal expected
    }
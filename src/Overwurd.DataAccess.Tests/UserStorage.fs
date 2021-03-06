module Overwurd.DataAccess.Tests.UserStorage

open System
open NUnit.Framework
open FsUnit

open Overwurd.Domain.Common
open Overwurd.Domain.Users
open Overwurd.Domain.Users.Entities
open Overwurd.DataAccess
open Overwurd.DataAccess.Tests.Domain
open Overwurd.DataAccess.Tests.Domain.Building
open Overwurd.DataAccess.Tests.Common
open Overwurd.DataAccess.Tests.Common.Utils
open Overwurd.DataAccess.UserStorage

let unwrap (userId: UserId option): UserId =
    match userId with
    | Some id -> id
    | None -> failwith "Entity is expected to be persisted, but has no Id."

[<Test>]
let ``No user should be found by Id in empty database`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let! actual = findUserByIdAsync (UserId 1) |> withConnectionAsync

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

        let! actual = findUserByIdAsync (unwrap user.Id) |> withConnectionAsync

        let expected: User option =
            Some { Id = user.Id.Value
                   CreatedAt = user.CreatedAt
                   Login = user.Login
                   Password =
                       { Hash = user.PasswordHash
                         Salt = user.PasswordSalt } }

        actual |> should equal expected
    }

[<Test>]
let ``No user should be found by Login in empty database`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let login =
            "TestLogin123"
            |> Login.create
            |> NormalizedLogin.create

        let! actual = findUserByNormalizedLoginAsync login |> withConnectionAsync

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

        let! actual = findUserByNormalizedLoginAsync user.NormalizedLogin |> withConnectionAsync

        let expected: User option =
            Some { Id = user.Id.Value
                   CreatedAt = user.CreatedAt
                   Login = user.Login
                   Password =
                       { Hash = user.PasswordHash
                         Salt = user.PasswordSalt } }

        actual |> should equal expected
    }

[<Test>]
let ``Creates User`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)

        let login = Login.create "TestLogin123"
        let parameters: UserCreationParametersForPersistence =
            { CreatedAt = UtcDateTime.create date
              Login = login
              NormalizedLogin = NormalizedLogin.create login
              PasswordHash = "1"
              PasswordSalt = "Some salt"}

        let! createdId = createUserAsync parameters |> withConnectionAsync

        let! actual = Database.getAllUsersAsync |> withConnectionAsync

        let expected: UserPersistentModel list =
            [ { Id = UserId.unwrap createdId
                CreatedAt = UtcDateTime.unwrap parameters.CreatedAt
                Login = Login.unwrap parameters.Login
                NormalizedLogin = NormalizedLogin.unwrap parameters.NormalizedLogin
                PasswordHash = parameters.PasswordHash
                PasswordSalt = parameters.PasswordSalt } ]

        actual |> should equal expected
    }
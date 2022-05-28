module Overwurd.Infrastructure.Tests.JwtRefreshTokenStore

open System
open System.Threading
open NUnit.Framework
open FsUnit

open Overwurd.Domain
open Overwurd.Domain.User
open Overwurd.Infrastructure
open Overwurd.Infrastructure.Tests.Domain
open Overwurd.Infrastructure.Tests.Domain.Building
open Overwurd.Infrastructure.Tests.Common.Utils

let unwrap (userId: UserId option): int =
    match userId with
    | Some id -> UserId.unwrap id
    | None -> failwith "Entity is expected to be persisted, but has no Id."

[<Test>]
let ``Getting User tokens when User has no tokens`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let user = makeUser "TestLogin123" date

        let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUser user
        do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync

        let! tokens = JwtRefreshTokenStore.getUserRefreshTokensAsync (unwrap user.Id) CancellationToken.None |> withConnectionAsync
        tokens |> should be Empty
    }

[<Test>]
let ``Getting User tokens when token does not have refresh date`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let token = makeRefreshToken date
        let user = makeUserWithRefreshTokens "TestLogin123" [token] date

        let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUser user
        do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync

        let! tokens = JwtRefreshTokenStore.getUserRefreshTokensAsync (unwrap user.Id) CancellationToken.None |> withConnectionAsync

        let expected: JwtRefreshToken list =
            [ { Id = token.Id.Value
                AccessTokenId = token.AccessTokenId
                Value = token.Value
                UserId = user.Id.Value
                CreatedAt = token.CreatedAt
                RefreshedAt = token.RefreshedAt
                ExpiresAt = token.ExpiresAt
                IsRevoked = token.IsRevoked } ]

        tokens |> should equal expected
    }

[<Test>]
let ``Getting User tokens when token has a refresh date`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let refreshDate = DateTime(year = 2022, month = 1, day = 2, hour = 1, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let token: JwtRefreshTokenSnapshot =
            { Id = None
              AccessTokenId = JwtAccessTokenId (Guid.NewGuid())
              Value = Guid.NewGuid()
              CreatedAt = CreationDate.create date
              RefreshedAt = Some(RefreshDate.create refreshDate)
              ExpiresAt = ExpiryDate.create (date.AddMinutes 5)
              IsRevoked = false }
        let user = makeUserWithRefreshTokens "TestLogin123" [token] date

        let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUser user
        do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync

        let! tokens = JwtRefreshTokenStore.getUserRefreshTokensAsync (unwrap user.Id) CancellationToken.None |> withConnectionAsync

        let expected: JwtRefreshToken list =
            [ { Id = token.Id.Value
                AccessTokenId = token.AccessTokenId
                Value = token.Value
                UserId = user.Id.Value
                CreatedAt = token.CreatedAt
                RefreshedAt = token.RefreshedAt
                ExpiresAt = token.ExpiresAt
                IsRevoked = token.IsRevoked } ]

        tokens |> should equal expected
    }

[<Test>]
let ``Getting User tokens should filter by User`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let date1 = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let token1 = makeRefreshToken date1
        let user1 = makeUserWithRefreshTokens "TestLogin111" [token1] date1

        let date2 = DateTime(year = 2022, month = 1, day = 3, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let token2 = makeRefreshToken date2
        let user2 = makeUserWithRefreshTokens "TestLogin222" [token2] date2

        let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUsers [user1; user2]
        do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync

        let! tokens = JwtRefreshTokenStore.getUserRefreshTokensAsync (unwrap user2.Id) CancellationToken.None |> withConnectionAsync

        let expected: JwtRefreshToken list =
            [ { Id = token2.Id.Value
                AccessTokenId = token2.AccessTokenId
                Value = token2.Value
                UserId = user2.Id.Value
                CreatedAt = token2.CreatedAt
                RefreshedAt = token2.RefreshedAt
                ExpiresAt = token2.ExpiresAt
                IsRevoked = token2.IsRevoked } ]

        tokens |> should equal expected
    }
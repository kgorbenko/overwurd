module Overwurd.Infrastructure.Tests.JwtRefreshTokenStore

open System
open System.Threading
open NUnit.Framework
open FsUnit

open Overwurd.Domain
open Overwurd.Domain.Jwt
open Overwurd.Domain.User
open Overwurd.Domain.CreationDate
open Overwurd.Infrastructure
open Overwurd.Infrastructure.Tests.Domain
open Overwurd.Infrastructure.Tests.Common
open Overwurd.Infrastructure.Tests.Domain.Building
open Overwurd.Infrastructure.Tests.Common.Utils

let unwrap (userId: UserId option): UserId =
    match userId with
    | Some id -> id
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

[<Test>]
let ``Token by User and Access Token is not found`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync
        
        let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let user = makeUser "TestLogin222" date
        
        let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUser user
        do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync
        
        let! result = JwtRefreshTokenStore.getRefreshTokenByUserAndAccessTokenAsync (unwrap user.Id) (Guid.NewGuid() |> JwtAccessTokenId) CancellationToken.None |> withConnectionAsync
        
        result |> should equal None
    }

[<Test>]
let ``Token by User and Access Token is found`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync
        
        let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let token = makeRefreshToken date
        let user = makeUserWithRefreshTokens "TestLogin222" [token] date
        
        let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUser user
        do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync
        
        let expected: JwtRefreshToken option =
            Some { Id = token.Id.Value
                   AccessTokenId = token.AccessTokenId
                   Value = token.Value
                   UserId = user.Id.Value
                   CreatedAt = token.CreatedAt
                   RefreshedAt = token.RefreshedAt
                   ExpiresAt = token.ExpiresAt
                   IsRevoked = token.IsRevoked }
        
        let! actual = JwtRefreshTokenStore.getRefreshTokenByUserAndAccessTokenAsync (unwrap user.Id) token.AccessTokenId CancellationToken.None |> withConnectionAsync
        
        actual |> should equal expected
    }

[<Test>]
let ``Update Refresh Token`` () =
    task {
        do! prepareDatabaseAsync |> withConnectionAsync

        let date = DateTime(year = 2022, month = 1, day = 2, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let token = makeRefreshToken date
        let user = makeUserWithRefreshTokens "TestLogin222" [token] date
        
        let snapshot = DomainSnapshot.create () |> DomainSnapshot.appendUser user
        do! DomainPersister.persistSnapshotAsync snapshot |> withConnectionAsync
        
        let refreshDate = DateTime(year = 2022, month = 1, day = 3, hour = 0, minute = 0, second = 0, kind = DateTimeKind.Utc)
        let updateParameters =
            { AccessTokenId = Guid.NewGuid() |> JwtAccessTokenId
              RefreshedAt = RefreshDate.create refreshDate }
        
        do! JwtRefreshTokenStore.updateRefreshTokenAsync token.Id.Value updateParameters CancellationToken.None |> withConnectionAsync
        
        let expectedTokens: JwtRefreshTokenPersistentModel list =
            [ { Id = JwtRefreshTokenId.unwrap token.Id.Value
                AccessTokenId = JwtAccessTokenId.unwrap updateParameters.AccessTokenId
                Value = token.Value
                UserId = UserId.unwrap user.Id.Value
                CreatedAt = CreationDate.unwrap token.CreatedAt
                RefreshedAt = RefreshDate.unwrap updateParameters.RefreshedAt |> Some
                ExpiresAt = ExpiryDate.unwrap token.ExpiresAt
                IsRevoked = token.IsRevoked } ]
            
        let! actualTokens = Database.getAllRefreshTokensAsync |> withConnectionAsync
        
        actualTokens |> should equal expectedTokens
    }
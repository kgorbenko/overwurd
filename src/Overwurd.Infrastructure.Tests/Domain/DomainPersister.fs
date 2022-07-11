module Overwurd.Infrastructure.Tests.Domain.DomainPersister

open System.Threading.Tasks

open Overwurd.Domain.Common.Persistence
open Overwurd.Domain.Jwt
open Overwurd.Domain.Users
open Overwurd.Infrastructure.Tests.Common
open Overwurd.Infrastructure.Tests.Domain

let private ensureTransient =
    function
    | Some id -> failwith $"Entity should be transient, but had an id: {id}"
    | _ -> ()

let private persistRefreshTokenAsync (token: JwtRefreshTokenSnapshot)
                                     (userId: UserId)
                                     (session: DbSession)
                                     : unit Task =
    task {
        ensureTransient token.Id

        let tokenCreationParameters: JwtRefreshTokenCreationParameters =
            { AccessTokenId = token.AccessTokenId
              Value = token.Value
              UserId = userId
              CreatedAt = token.CreatedAt
              RefreshedAt = token.RefreshedAt
              ExpiresAt = token.ExpiresAt
              IsRevoked = token.IsRevoked }

        let! tokenId = Database.createRefreshTokenAsync tokenCreationParameters session
        token.Id <- Some tokenId
    }

let private persistUserAsync (user: UserSnapshot)
                             (session: DbSession)
                             : unit Task =
    task {
        ensureTransient user.Id

        let userCreationParameters =
            { CreatedAt = user.CreatedAt
              Login = user.Login
              NormalizedLogin = user.NormalizedLogin
              PasswordHash = user.PasswordHash
              PasswordSalt = user.PasswordSalt}

        let! userId = Database.createUserAsync userCreationParameters session
        user.Id <- Some userId

        for token in user.JwtRefreshTokens do
            do! persistRefreshTokenAsync token userId session
    }

let persistSnapshotAsync (snapshot: DomainSnapshot)
                         (session: DbSession)
                         : unit Task =
    task {
        for user in snapshot.Users do
            do! persistUserAsync user session
    }

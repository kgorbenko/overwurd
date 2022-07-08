module internal Overwurd.Infrastructure.Tests.Common.Database

open Dapper
open System.Threading.Tasks

open Overwurd.Domain
open Overwurd.Domain.Common.Persistence
open Overwurd.Domain.Jwt
open Overwurd.Domain.Users
open Overwurd.Domain.Users.Entities
open Overwurd.Domain.Jwt.Entities
open Overwurd.Infrastructure.UserStorage
open Overwurd.Infrastructure.JwtStorage

let clearAsync (session: DbSession): unit Task =
    task {
        let sql = """
truncate "overwurd"."Users" restart identity cascade;
"""

        let command = CommandDefinition(commandText = sql, transaction = session.Transaction)
        let! _ = session.Connection.ExecuteAsync command

        ()
    }

let getAllUsersAsync (session: DbSession): UserPersistentModel list Task =
    task {
        let sql = """
select
    "Id",
    "CreatedAt",
    "Login",
    "NormalizedLogin",
    "PasswordHash",
    "PasswordSalt"
from "overwurd"."Users"
"""

        let command = CommandDefinition(commandText = sql, transaction = session.Transaction)
        let! result = session.Connection.QueryAsync<UserPersistentModel>(command)

        return result
            |> List.ofSeq
    }

let getAllRefreshTokensAsync (session: DbSession): JwtRefreshTokenPersistentModel list Task =
    task {
        let sql = """
select
    "Id",
    "AccessTokenId",
    "Value",
    "UserId",
    "CreatedAt",
    "RefreshedAt",
    "ExpiresAt",
    "IsRevoked"
  from "overwurd"."JwtRefreshTokens"
"""
    
        let command = CommandDefinition(commandText = sql, transaction = session.Transaction)
        let! result = session.Connection.QueryAsync<JwtRefreshTokenPersistentModel> command
        
        return result
            |> List.ofSeq
    }

let createUserAsync (creationParameters: UserCreationParametersForPersistence)
                    (session: DbSession)
                    : UserId Task =
    task {
        let sql = """
insert into "overwurd"."Users" (
    "CreatedAt",
    "Login",
    "NormalizedLogin",
    "PasswordHash",
    "PasswordSalt"
) values (
    @CreatedAt,
    @Login,
    @NormalizedLogin,
    @PasswordHash,
    @PasswordSalt
) returning "Id"
"""
        let parameters =
            {| CreatedAt = UtcDateTime.unwrap creationParameters.CreatedAt
               Login = Login.unwrap creationParameters.Login
               NormalizedLogin = NormalizedLogin.unwrap creationParameters.NormalizedLogin
               PasswordHash = creationParameters.PasswordHash
               PasswordSalt = creationParameters.PasswordSalt |}
               
        let command = CommandDefinition(commandText = sql, parameters = parameters, transaction = session.Transaction)
        let! id = session.Connection.QuerySingleAsync<int> command

        return (UserId id)
    }

let createRefreshTokenAsync (creationParameters: JwtRefreshTokenCreationParameters)
                            (session: DbSession)
                            : JwtRefreshTokenId Task =
    task {
        let sql = """
insert into "overwurd"."JwtRefreshTokens" (
    "AccessTokenId",
    "Value",
    "UserId",
    "CreatedAt",
    "RefreshedAt",
    "ExpiresAt",
    "IsRevoked"
) values (
    @AccessTokenId,
    @Value,
    @UserId,
    @CreatedAt,
    @RefreshedAt,
    @ExpiresAt,
    @IsRevoked
) returning "Id"
"""

        let parameters =
            {| AccessTokenId = JwtAccessTokenId.unwrap creationParameters.AccessTokenId
               Value = creationParameters.Value
               UserId = UserId.unwrap creationParameters.UserId
               CreatedAt = UtcDateTime.unwrap creationParameters.CreatedAt
               RefreshedAt = creationParameters.RefreshedAt |> Option.map UtcDateTime.unwrap
               ExpiresAt = UtcDateTime.unwrap creationParameters.ExpiresAt
               IsRevoked = creationParameters.IsRevoked |}
        let command = CommandDefinition(commandText = sql, parameters = parameters, transaction = session.Transaction)
        let! id = session.Connection.QuerySingleAsync<int> command

        return (JwtRefreshTokenId id)
    }

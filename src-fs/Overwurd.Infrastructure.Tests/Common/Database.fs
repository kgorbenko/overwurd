module internal Overwurd.Infrastructure.Tests.Common.Database

open Dapper
open System.Threading.Tasks

open Overwurd.Domain
open Overwurd.Domain.User
open Overwurd.Infrastructure
open Overwurd.Infrastructure.Database

let clearAsync (session: Session): unit Task =
    task {
        let sql = """
truncate "overwurd"."Users" restart identity cascade;
"""

        let command = CommandDefinition(commandText = sql, transaction = session.Transaction)
        let! _ = session.Connection.ExecuteAsync command

        ()
    }

let getAllUsersAsync (session: Session): UserPersistentModel list Task =
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

let createUserAsync (creationParameters: UserCreationParametersForPersistence)
                    (session: Session)
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
            {| CreatedAt = CreationDate.unwrap creationParameters.CreatedAt
               Login = Login.unwrap creationParameters.Login
               NormalizedLogin = NormalizedLogin.unwrap creationParameters.NormalizedLogin
               PasswordHash = creationParameters.PasswordHash
               PasswordSalt = creationParameters.PasswordSalt |}
               
        let command = CommandDefinition(commandText = sql, parameters = parameters, transaction = session.Transaction)
        let! id = session.Connection.QuerySingleAsync<int> command

        return (UserId id)
    }

let createCourseAsync (creationParameters: CourseCreationParametersForPersistence)
                      (userId: int)
                      (session: Session)
                      : CourseId Task =
    task {
        let sql = """
insert into "overwurd"."Courses" (
    "CreatedAt",
    "UserId",
    "Name",
    "Description"
) values (
    @CreatedAt,
    @UserId,
    @Name,
    @Description
) returning "Id"
"""

        let parameters =
            {| CreatedAt = creationParameters.CreatedAt
               UserId = userId
               Name = creationParameters.Name
               Description = creationParameters.Description |}

        let command = CommandDefinition(commandText = sql, parameters = parameters, transaction = session.Transaction)
        let! id = session.Connection.QuerySingleAsync<int> command

        return (CourseId id)
    }

let createRefreshTokenAsync (creationParameters: JwtRefreshTokenCreationParametersForPersistence)
                            (session: Session)
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
            {| AccessTokenId = creationParameters.AccessTokenId
               Value = creationParameters.Value
               UserId = creationParameters.UserId
               CreatedAt = creationParameters.CreatedAt
               RefreshedAt = creationParameters.RefreshedAt
               ExpiresAt = creationParameters.ExpiresAt
               IsRevoked = creationParameters.IsRevoked |}
        let command = CommandDefinition(commandText = sql, parameters = parameters, transaction = session.Transaction)
        let! id = session.Connection.QuerySingleAsync<int> command

        return (JwtRefreshTokenId id)
    }

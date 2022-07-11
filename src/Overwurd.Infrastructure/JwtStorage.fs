module Overwurd.Infrastructure.JwtStorage

open System

open Overwurd.Domain
open Overwurd.Domain.Common.Persistence
open Overwurd.Domain.Users
open Overwurd.Domain.Users.Entities
open Overwurd.Domain.Jwt
open Overwurd.Domain.Jwt.Entities
open Overwurd.Infrastructure.Database.Dapper

type internal JwtRefreshTokenPersistentModel =
    { Id: int
      AccessTokenId: Guid
      Value: Guid
      UserId: int
      CreatedAt: DateTime
      RefreshedAt: DateTime option
      ExpiresAt: DateTime
      IsRevoked: bool }

let private toDomain (model: JwtRefreshTokenPersistentModel)
                     : JwtRefreshToken =
    { Id = JwtRefreshTokenId model.Id
      AccessTokenId = JwtAccessTokenId model.AccessTokenId
      Value = model.Value
      UserId = UserId model.Id
      CreatedAt = UtcDateTime.create model.CreatedAt
      RefreshedAt = model.RefreshedAt |> Option.map UtcDateTime.create
      ExpiresAt = UtcDateTime.create model.ExpiresAt
      IsRevoked = model.IsRevoked }

let getUserRefreshTokensAsync: GetUserRefreshTokensAsync =
    fun (userId: UserId)
        (session: DbSession) ->
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
 where "UserId" = @UserId
"""

            let parameters = {| UserId = UserId.unwrap userId |}
            let command = makeSqlCommandWithParameters sql parameters session
            let! result = session.Connection |> queryAsync<JwtRefreshTokenPersistentModel> command

            return result
                |> List.map toDomain
        }

let getRefreshTokenByUserAndAccessTokenAsync: GetRefreshTokenByUserAndAccessTokenAsync =
    fun (userId: UserId)
        (accessTokenId: JwtAccessTokenId)
        (session: DbSession) ->
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
 where "UserId" = @UserId and "AccessTokenId" = @AccessTokenId
"""

            let parameters =
                {| UserId = UserId.unwrap userId
                   AccessTokenId = JwtAccessTokenId.unwrap accessTokenId |}
            let command = makeSqlCommandWithParameters sql parameters session
            let! result = session.Connection |> findAsync<JwtRefreshTokenPersistentModel> command
            
            return result |> Option.map toDomain
        }

let removeRefreshTokensAsync: RemoveRefreshTokensAsync =
    fun (refreshTokenIdsToRemove: JwtRefreshTokenId list)
        (session: DbSession) ->
        task {
            let sql = """
delete from "overwurd"."JwtRefreshTokens"
 where "Id" = any (@TokenIdsToRemove)
"""

            let parameters = {| TokenIdsToRemove = refreshTokenIdsToRemove |> List.map JwtRefreshTokenId.unwrap |> Array.ofList |}
            let command = makeSqlCommandWithParameters sql parameters session
            do! session.Connection |> executeAsync command
        }

let createRefreshTokenAsync: CreateRefreshTokenAsync =
    fun (parameters: JwtRefreshTokenCreationParameters)
        (session: DbSession) ->
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
                {| AccessTokenId = JwtAccessTokenId.unwrap parameters.AccessTokenId
                   Value = parameters.Value
                   UserId = UserId.unwrap parameters.UserId
                   CreatedAt = UtcDateTime.unwrap parameters.CreatedAt
                   RefreshedAt = parameters.RefreshedAt |> Option.map UtcDateTime.unwrap
                   ExpiresAt = UtcDateTime.unwrap parameters.ExpiresAt
                   IsRevoked = parameters.IsRevoked |}
            let command = makeSqlCommandWithParameters sql parameters session
            let! id = session.Connection |> querySingleAsync<int> command

            return JwtRefreshTokenId id
        }

let updateRefreshTokenAsync: UpdateRefreshTokenAsync =
    fun (tokenId: JwtRefreshTokenId)
        (updateParameters: JwtRefreshTokenUpdateParameters)
        (session: DbSession) ->
        task {
            let sql = """
update "overwurd"."JwtRefreshTokens"
   set
    "AccessTokenId" = @AccessTokenId,
    "RefreshedAt" = @RefreshedAt
 where "Id" = @Id
"""

            let parameters =
                {| Id = JwtRefreshTokenId.unwrap tokenId
                   AccessTokenId = JwtAccessTokenId.unwrap updateParameters.AccessTokenId
                   RefreshedAt = UtcDateTime.unwrap updateParameters.RefreshedAt |}
            let command = makeSqlCommandWithParameters sql parameters session
            do! session.Connection |> executeAsync command
        }
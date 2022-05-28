namespace Overwurd.Infrastructure

open System
open System.Threading
open System.Threading.Tasks

open Overwurd.Domain
open Overwurd.Infrastructure.Database
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

module JwtRefreshTokenStore =

    let private toDomain (model: JwtRefreshTokenPersistentModel)
                         : JwtRefreshToken =
        { Id = JwtRefreshTokenId model.Id
          AccessTokenId = JwtAccessTokenId model.AccessTokenId
          Value = model.Value
          UserId = UserId model.Id
          CreatedAt = CreationDate.create model.CreatedAt
          RefreshedAt = model.RefreshedAt |> Option.map RefreshDate.create
          ExpiresAt = ExpiryDate.create model.ExpiresAt
          IsRevoked = model.IsRevoked }

    let getUserRefreshTokensAsync (userId: int)
                                  (cancellationToken: CancellationToken)
                                  (session: Session)
                                  : JwtRefreshToken list Task =
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

            let parameters = {| UserId = userId |}
            let command = makeSqlCommandWithParameters sql parameters session.Transaction cancellationToken
            let! result = session.Connection |> queryAsync<JwtRefreshTokenPersistentModel> command

            return result
                |> List.map toDomain
        }

    let removeRefreshTokensAsync (refreshTokenIdsToRemove: int list)
                                 (cancellationToken: CancellationToken)
                                 (session: Session)
                                 : unit Task =
        task {
            let sql = """
delete from "overwurd"."JwtRefreshTokens"
 where "Id" in @TokenIdsToRemove
"""

            let parameters = {| TokenIdsToRemove = refreshTokenIdsToRemove |}
            let command = makeSqlCommandWithParameters sql parameters session.Transaction cancellationToken
            do! session.Connection |> executeAsync command
        }

    let createRefreshTokenAsync (parameters: JwtRefreshTokenCreationParametersForPersistence)
                                (cancellationToken: CancellationToken)
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
                {| AccessTokenId = parameters.AccessTokenId
                   Value = parameters.Value
                   UserId = parameters.UserId
                   CreatedAt = parameters.CreatedAt
                   RefreshedAt = parameters.RefreshedAt
                   ExpiresAt = parameters.ExpiresAt
                   IsRevoked = parameters.IsRevoked |}
            let command = makeSqlCommandWithParameters sql parameters session.Transaction cancellationToken
            let! id = session.Connection |> querySingleAsync<int> command

            return JwtRefreshTokenId id
        }
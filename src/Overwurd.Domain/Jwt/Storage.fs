namespace Overwurd.Domain.Jwt

open System
open System.Threading.Tasks

open Overwurd.Domain.Common
open Overwurd.Domain.Users
open Overwurd.Domain.Common.Persistence

type JwtRefreshTokenCreationParameters =
    { AccessTokenId: JwtAccessTokenId
      Value: Guid
      UserId: UserId
      CreatedAt: UtcDateTime
      RefreshedAt: UtcDateTime option
      ExpiresAt: UtcDateTime
      IsRevoked: bool }

type JwtRefreshTokenUpdateParameters =
    { AccessTokenId: JwtAccessTokenId
      RefreshedAt: UtcDateTime }

type GetUserRefreshTokensAsync =
    UserId -> DbSession -> JwtRefreshToken list Task

type GetRefreshTokenByUserAndAccessTokenAsync =
    UserId -> JwtAccessTokenId -> DbSession -> JwtRefreshToken option Task

type RemoveRefreshTokensAsync =
    JwtRefreshTokenId list -> DbSession -> unit Task

type CreateRefreshTokenAsync =
    JwtRefreshTokenCreationParameters -> DbSession -> JwtRefreshTokenId Task

type UpdateRefreshTokenAsync =
    JwtRefreshTokenId -> JwtRefreshTokenUpdateParameters -> DbSession -> unit Task

type JwtStorage =
    { GetUserRefreshTokensAsync: GetUserRefreshTokensAsync
      GetRefreshTokenByUserAndAccessTokenAsync: GetRefreshTokenByUserAndAccessTokenAsync
      CreateRefreshTokenAsync: CreateRefreshTokenAsync
      UpdateRefreshTokenAsync: UpdateRefreshTokenAsync
      RemoveRefreshTokensAsync: RemoveRefreshTokensAsync }
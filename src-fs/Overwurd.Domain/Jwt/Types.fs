namespace Overwurd.Domain.Jwt

open System
open Microsoft.IdentityModel.Tokens

open Overwurd.Domain
open Overwurd.Domain.Users

type JwtRefreshTokenId =
    JwtRefreshTokenId of int

type JwtAccessTokenId =
    JwtAccessTokenId of Guid

type JwtRefreshToken =
    { Id: JwtRefreshTokenId
      AccessTokenId: JwtAccessTokenId
      Value: Guid
      UserId: UserId
      CreatedAt: UtcDateTime
      RefreshedAt: UtcDateTime option
      ExpiresAt: UtcDateTime
      IsRevoked: bool }

type JwtTokensPair =
    { AccessTokenValue: string
      RefreshTokenValue: string }

type JwtConfiguration =
    { TokensConfiguration: JwtTokensConfiguration
      ClaimsOptions: ClaimsOptions
      TokenValidationParameters: TokenValidationParameters } 
and JwtTokensConfiguration =
    { MaxTokensPerUser: int
      SecurityAlgorithmSignature: string
      SigningKey: string
      Issuer: string
      Audience: string
      AccessTokenExpirationInMinutes: int
      RefreshTokenExpirationInDays: int }
and ClaimsOptions =
    { RoleClaimType: string
      UserNameClaimType: string
      UserIdClaimType: string
      EmailClaimType: string }
    
type JwtGenerationResult =
    { UserId: UserId
      Tokens: JwtTokensPair
      AccessTokenExpiresAt: UtcDateTime
      RefreshTokenExpiresAt: UtcDateTime }

type RefreshError =
    | AccessTokenValidationError of ErrorMessage: string
    | UserIdOrAccessTokenIdClaimsAreMissing
    | RefreshTokenNotFound
    | RefreshTokenNotValid
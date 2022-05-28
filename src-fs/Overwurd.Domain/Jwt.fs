namespace Overwurd.Domain

open System
open System.Text
open System.Security.Claims
open System.Threading.Tasks
open System.IdentityModel.Tokens.Jwt
open Microsoft.IdentityModel.Tokens

open Overwurd.Domain
open Overwurd.Domain.User
open Overwurd.Domain.Common.Utils

type JwtRefreshTokenId =
    JwtRefreshTokenId of int

type JwtAccessTokenId =
    JwtAccessTokenId of Guid

type JwtRefreshToken =
    { Id: JwtRefreshTokenId
      AccessTokenId: JwtAccessTokenId
      Value: Guid
      UserId: UserId
      CreatedAt: CreationDate
      RefreshedAt: RefreshDate option
      ExpiresAt: ExpiryDate
      IsRevoked: bool }

type JwtTokensPair =
    { AccessTokenValue: string
      RefreshTokenValue: string }

type JwtConfiguration =
    { MaxTokensPerUser: int
      SecurityAlgorithmSignature: string
      SigningKey: string
      Issuer: string
      Audience: string
      AccessTokenExpirationInMinutes: int
      RefreshTokenExpirationInDays: int }

type JwtRefreshTokenCreationParametersForPersistence =
    { AccessTokenId: Guid
      Value: Guid
      UserId: int
      CreatedAt: DateTime
      RefreshedAt: DateTime option
      ExpiresAt: DateTime
      IsRevoked: bool }

type GenerateGuid =
    unit -> Guid

type GetUserRefreshTokensAsync =
    UserId -> JwtRefreshToken list Task

type RemoveRefreshTokensAsync =
    JwtRefreshTokenId list -> unit Task

type CreateRefreshTokenAsync =
    JwtRefreshTokenCreationParametersForPersistence -> JwtRefreshTokenId Task

module Jwt =

    module JwtRefreshTokenId =

        let unwrap (tokenId: JwtRefreshTokenId): int =
            match tokenId with
            | JwtRefreshTokenId value -> value

    module JwtAccessTokenId =

        let unwrap (tokenId: JwtAccessTokenId): Guid =
            match tokenId with
            | JwtAccessTokenId value -> value

    let private getBytesFromSigningKey (key: string) =
        Encoding.ASCII.GetBytes(key);

    let private createAccessToken (configuration: JwtConfiguration)
                                  (tokenId: Guid)
                                  (userId: UserId)
                                  (claims: Claim list)
                                  (now: DateTime)
                                  : JwtSecurityToken =
        let defaultClaims =
            [ Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString())
              Claim(JwtRegisteredClaimNames.Sub, (UserId.unwrap userId).ToString()) ]

        JwtSecurityToken(
            issuer = configuration.Issuer,
            audience = configuration.Audience,
            claims = defaultClaims @ claims,
            expires = now.AddMinutes configuration.AccessTokenExpirationInMinutes,
            signingCredentials = SigningCredentials(
                key = SymmetricSecurityKey(getBytesFromSigningKey configuration.SigningKey),
                algorithm = configuration.SecurityAlgorithmSignature
            )
        )

    let private getTokenIdsToRemove (maxTokensPerUser: int)
                                    (now: DateTime)
                                    (userRefreshTokens: JwtRefreshToken list)
                                    : JwtRefreshTokenId Set =
        let expiredTokenIds =
            userRefreshTokens
            |> List.filter (fun x -> ExpiryDate.unwrap x.ExpiresAt < now)
            |> List.map (fun x -> x.Id)

        let excessiveTokenIds =
            if userRefreshTokens.Length >= maxTokensPerUser then
                let numberToRemove = userRefreshTokens.Length - maxTokensPerUser + 1

                userRefreshTokens
                |> List.sortBy (fun x -> x.RefreshedAt)
                |> List.take numberToRemove
                |> List.map (fun x -> x.Id)
            else []

        expiredTokenIds @ excessiveTokenIds |> Set

    let generateTokensAsync (generateGuid: GenerateGuid)
                            (getUserTokensAsync: GetUserRefreshTokensAsync)
                            (removeTokensAsync: RemoveRefreshTokensAsync)
                            (insertTokenAsync: CreateRefreshTokenAsync)
                            (configuration: JwtConfiguration)
                            (userId: UserId)
                            (claims: Claim list)
                            (now: DateTime)
                            : JwtTokensPair Task =
        task {
            let utcNow = now |> ensureUtc

            let tokenHandler = JwtSecurityTokenHandler ()
            let tokenId = generateGuid ()
            let jwtToken = createAccessToken configuration tokenId userId claims utcNow
            let jwtTokenEncrypted = tokenHandler.WriteToken jwtToken

            let! userRefreshTokens = getUserTokensAsync userId
            let tokenIdsToRemove = getTokenIdsToRemove configuration.MaxTokensPerUser utcNow userRefreshTokens

            if not <| Set.isEmpty tokenIdsToRemove then
                do! removeTokensAsync (tokenIdsToRemove |> Set.toList)

            let refreshTokenValue = generateGuid ()
            let expiryDate = utcNow.AddDays configuration.RefreshTokenExpirationInDays
            let refreshTokenCreationParameters =
                { AccessTokenId = tokenId
                  UserId = UserId.unwrap userId
                  Value = refreshTokenValue
                  CreatedAt = utcNow
                  RefreshedAt = None
                  ExpiresAt = expiryDate
                  IsRevoked = false }

            let! _ = insertTokenAsync refreshTokenCreationParameters

            return
                { AccessTokenValue = jwtTokenEncrypted
                  RefreshTokenValue = refreshTokenValue.ToString() }
        }
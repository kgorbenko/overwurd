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
      EmailClaimType: string
      SecurityStampClaimType: string }

type JwtRefreshTokenCreationParametersForPersistence =
    { AccessTokenId: JwtAccessTokenId
      Value: Guid
      UserId: UserId
      CreatedAt: UtcDateTime
      RefreshedAt: UtcDateTime option
      ExpiresAt: UtcDateTime
      IsRevoked: bool }

type JwtRefreshTokenUpdateParametersForPersistence =
    { AccessTokenId: JwtAccessTokenId
      RefreshedAt: UtcDateTime }

type GenerateGuid =
    unit -> Guid

type GetUserRefreshTokensAsync =
    UserId -> JwtRefreshToken list Task

type GetRefreshTokenByUserAndAccessTokenAsync =
    UserId -> JwtAccessTokenId -> JwtRefreshToken option Task

type RemoveRefreshTokensAsync =
    JwtRefreshTokenId list -> unit Task

type CreateRefreshTokenAsync =
    JwtRefreshTokenCreationParametersForPersistence -> JwtRefreshTokenId Task

type UpdateRefreshTokenAsync =
    JwtRefreshTokenId -> JwtRefreshTokenUpdateParametersForPersistence -> unit Task

type JwtRefreshTokensPersister =
    { GetUserRefreshTokensAsync: GetUserRefreshTokensAsync
      GetRefreshTokenByUserAndAccessTokenAsync: GetRefreshTokenByUserAndAccessTokenAsync
      CreateRefreshTokenAsync: CreateRefreshTokenAsync
      UpdateRefreshTokenAsync: UpdateRefreshTokenAsync
      RemoveRefreshTokensAsync: RemoveRefreshTokensAsync }

type GenerateTokensPairAsync =
    UserId -> Claim list -> UtcDateTime -> JwtTokensPair Task

type JwtDependencies =
    { GenerateGuid: GenerateGuid
      JwtConfiguration: JwtConfiguration
      RefreshTokensPersister: JwtRefreshTokensPersister }

type RefreshAccessTokenError =
    | AccessTokenValidationError of ErrorMessage: string
    | UserIdOrAccessTokenIdClaimsAreMissing
    | RefreshTokenNotFound
    | RefreshTokenNotValid

module Jwt =

    module JwtRefreshTokenId =

        let unwrap (tokenId: JwtRefreshTokenId): int =
            match tokenId with
            | JwtRefreshTokenId value -> value

    module JwtAccessTokenId =

        let tryParse (tokenId: string): JwtAccessTokenId option =
            match Guid.TryParse tokenId with
            | true, value -> Some (JwtAccessTokenId value)
            | false, _ -> None

        let unwrap (tokenId: JwtAccessTokenId): Guid =
            match tokenId with
            | JwtAccessTokenId value -> value

    let private getBytesFromSigningKey (key: string) =
        Encoding.ASCII.GetBytes(key);

    let private createAccessToken (jwtConfiguration: JwtConfiguration)
                                  (tokenId: Guid)
                                  (userId: int)
                                  (claims: Claim list)
                                  (now: DateTime)
                                  : JwtSecurityToken =
        let defaultClaims =
            [ Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString())
              Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) ]

        JwtSecurityToken(
            issuer = jwtConfiguration.TokensConfiguration.Issuer,
            audience = jwtConfiguration.TokensConfiguration.Audience,
            claims = defaultClaims @ claims,
            expires = now.AddMinutes jwtConfiguration.TokensConfiguration.AccessTokenExpirationInMinutes,
            signingCredentials = SigningCredentials(
                key = SymmetricSecurityKey(getBytesFromSigningKey jwtConfiguration.TokensConfiguration.SigningKey),
                algorithm = jwtConfiguration.TokensConfiguration.SecurityAlgorithmSignature
            )
        )

    let private getTokenIdsToRemove (maxTokensPerUser: int)
                                    (now: DateTime)
                                    (userRefreshTokens: JwtRefreshToken list)
                                    : JwtRefreshTokenId Set =
        let expiredTokenIds =
            userRefreshTokens
            |> List.filter (fun x -> UtcDateTime.unwrap x.ExpiresAt < now)
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

    let generateTokensPairAsync (dependencies: JwtDependencies)
                                (userId: UserId)
                                (claims: Claim list)
                                (now: UtcDateTime)
                                : JwtTokensPair Task =
        task {
            let nowUnwrapped = UtcDateTime.unwrap now
            let tokensConfiguration = dependencies.JwtConfiguration.TokensConfiguration

            let tokenHandler = JwtSecurityTokenHandler ()
            let tokenId = dependencies.GenerateGuid ()
            let jwtToken = createAccessToken dependencies.JwtConfiguration tokenId (UserId.unwrap userId) claims nowUnwrapped
            let jwtTokenEncrypted = tokenHandler.WriteToken jwtToken

            let getUserRefreshTokensAsync =
                dependencies
                    .RefreshTokensPersister
                    .GetUserRefreshTokensAsync
            let! userRefreshTokens = getUserRefreshTokensAsync userId
            let tokenIdsToRemove = getTokenIdsToRemove tokensConfiguration.MaxTokensPerUser nowUnwrapped userRefreshTokens

            let removeRefreshTokensAsync =
                dependencies
                    .RefreshTokensPersister
                    .RemoveRefreshTokensAsync
            
            if not <| Set.isEmpty tokenIdsToRemove then
                do! removeRefreshTokensAsync (tokenIdsToRemove |> Set.toList)

            let refreshTokenValue = dependencies.GenerateGuid ()
            let expiryDate = nowUnwrapped.AddDays tokensConfiguration.RefreshTokenExpirationInDays
            let refreshTokenCreationParameters =
                { AccessTokenId = JwtAccessTokenId tokenId
                  UserId = userId
                  Value = refreshTokenValue
                  CreatedAt = now
                  RefreshedAt = None
                  ExpiresAt = UtcDateTime.create expiryDate
                  IsRevoked = false }

            let createRefreshTokensAsync =
                dependencies
                    .RefreshTokensPersister
                    .CreateRefreshTokenAsync
            let! _ = createRefreshTokensAsync refreshTokenCreationParameters

            return
                { AccessTokenValue = jwtTokenEncrypted
                  RefreshTokenValue = refreshTokenValue.ToString() }
        }

    type private AccessTokenDecryptionResult =
        { AccessTokenDecrypted: JwtSecurityToken }

    let private decryptAccessTokenAsync (dependencies: JwtDependencies)
                                        (accessTokenEncrypted: string)
                                        : Result<AccessTokenDecryptionResult, RefreshAccessTokenError> Task =
        task {
            let parameters = dependencies.JwtConfiguration.TokenValidationParameters.Clone ()
            parameters.ValidateLifetime <- false

            let tokenHandler = JwtSecurityTokenHandler ()
            let! validationResult = tokenHandler.ValidateTokenAsync(accessTokenEncrypted, parameters)

            return
                if validationResult.IsValid then
                    Ok { AccessTokenDecrypted = validationResult.SecurityToken :?> JwtSecurityToken }
                else
                    Error (AccessTokenValidationError validationResult.Exception.Message)
        }

    type private ParsingResult =
        { UserId: UserId
          AccessTokenId: JwtAccessTokenId }

    type private UserIdAndAccessTokenParsingResult =
        { DecryptionResult: AccessTokenDecryptionResult
          ParsingResult: ParsingResult }

    let private parseUserIdAndAccessTokenId (decryptionResult: AccessTokenDecryptionResult)
                                            : Result<UserIdAndAccessTokenParsingResult, RefreshAccessTokenError> =
        let userIdClaimValue =
            decryptionResult.AccessTokenDecrypted.Claims
            |> Seq.filter (fun x -> x.Type = JwtRegisteredClaimNames.Sub)
            |> Seq.tryExactlyOne
            |> Option.bind (fun x -> UserId.tryParse x.Value)

        let accessTokenIdClaimValue = decryptionResult.AccessTokenDecrypted.Id |> JwtAccessTokenId.tryParse

        match userIdClaimValue, accessTokenIdClaimValue with
        | Some userId, Some accessTokenId ->
            Ok { DecryptionResult = decryptionResult
                 ParsingResult =
                     { UserId = userId
                       AccessTokenId = accessTokenId } }
        | _ -> Error UserIdOrAccessTokenIdClaimsAreMissing

    type private RefreshToken =
        { Value: JwtRefreshToken }

    type private RefreshTokenResult =
        { DecryptionResult: AccessTokenDecryptionResult
          ParsingResult: ParsingResult
          RefreshToken: RefreshToken }

    let private getActualRefreshTokenAsync (dependencies: JwtDependencies)
                                           (parsingResult: UserIdAndAccessTokenParsingResult)
                                           : Result<RefreshTokenResult, RefreshAccessTokenError> Task =
        task {
            let parsedIds = parsingResult.ParsingResult
            let getTokenByUserAndAccessToken =
                dependencies
                    .RefreshTokensPersister
                    .GetRefreshTokenByUserAndAccessTokenAsync

            let! refreshToken = getTokenByUserAndAccessToken parsedIds.UserId parsedIds.AccessTokenId

            return
                match refreshToken with
                | Some token ->
                    Ok { DecryptionResult = parsingResult.DecryptionResult
                         ParsingResult = parsingResult.ParsingResult
                         RefreshToken = { Value = token } }
                | None ->
                    Error RefreshTokenNotFound
        }

    type private ValidationResult =
        { IsValid: bool }

    type private RefreshTokenValidationResult =
        { DecryptionResult: AccessTokenDecryptionResult
          ParsingResult: ParsingResult
          RefreshToken: RefreshToken
          ValidationResult: ValidationResult }

    let private validateRefreshToken (providedTokenValue: string)
                                     (now: UtcDateTime)
                                     (refreshTokenResult: RefreshTokenResult)
                                     : Result<RefreshTokenValidationResult, RefreshAccessTokenError> =
        let refreshToken = refreshTokenResult.RefreshToken.Value
        let doesTokenValueMatchToProvided = refreshToken.Value.ToString() = providedTokenValue
        let isNotExpired = refreshToken.ExpiresAt > now
        let isNotRevoked = not refreshToken.IsRevoked

        let isTokenValid = doesTokenValueMatchToProvided && isNotExpired && isNotRevoked

        if isTokenValid then
            Ok { DecryptionResult = refreshTokenResult.DecryptionResult
                 ParsingResult = refreshTokenResult.ParsingResult
                 RefreshToken = refreshTokenResult.RefreshToken
                 ValidationResult = { IsValid = true } }
        else
            Error RefreshTokenNotValid

    let private updateTokens (dependencies: JwtDependencies)
                             (now: UtcDateTime)
                             (validationResult: RefreshTokenValidationResult)
                             : Result<JwtTokensPair, RefreshAccessTokenError> Task =
        task {
            let getUserClaims (claims: Claim list) =
                let userClaimTypes =
                    [ dependencies.JwtConfiguration.ClaimsOptions.EmailClaimType
                      dependencies.JwtConfiguration.ClaimsOptions.RoleClaimType
                      dependencies.JwtConfiguration.ClaimsOptions.SecurityStampClaimType
                      dependencies.JwtConfiguration.ClaimsOptions.UserIdClaimType
                      dependencies.JwtConfiguration.ClaimsOptions.UserNameClaimType ]
                    |> Set.ofList
                claims |> List.filter (fun x -> userClaimTypes |> Set.contains x.Type)

            let newAccessTokenId = dependencies.GenerateGuid ()
            let userId = validationResult.ParsingResult.UserId
            let claims = getUserClaims (validationResult.DecryptionResult.AccessTokenDecrypted.Claims |> List.ofSeq)
            let newAccessToken = createAccessToken dependencies.JwtConfiguration newAccessTokenId (UserId.unwrap userId) claims (UtcDateTime.unwrap now)

            let tokenHandler = JwtSecurityTokenHandler ()
            let newAccessTokenEncrypted = tokenHandler.WriteToken newAccessToken

            let refreshTokenUpdateParameters =
                { AccessTokenId = JwtAccessTokenId newAccessTokenId
                  RefreshedAt = now }

            let refreshToken = validationResult.RefreshToken.Value

            let updateRefreshTokenAsync =
                dependencies
                    .RefreshTokensPersister
                    .UpdateRefreshTokenAsync
            do! updateRefreshTokenAsync refreshToken.Id refreshTokenUpdateParameters

            return
                Ok { AccessTokenValue = newAccessTokenEncrypted
                     RefreshTokenValue = refreshToken.Value.ToString() }
        }

    let refreshAccessTokenAsync (dependencies: JwtDependencies)
                                (accessTokenEncrypted: string)
                                (refreshTokenValue: string)
                                (now: UtcDateTime)
                                : Result<JwtTokensPair, RefreshAccessTokenError> Task =
        task {
            return!
                accessTokenEncrypted
                |> decryptAccessTokenAsync dependencies
                |> AsyncResult.synchronouslyBindTask parseUserIdAndAccessTokenId
                |> AsyncResult.asynchronouslyBindTask (getActualRefreshTokenAsync dependencies)
                |> AsyncResult.synchronouslyBindTask (validateRefreshToken refreshTokenValue now)
                |> AsyncResult.asynchronouslyBindTask (updateTokens dependencies now)
        }
module Overwurd.Domain.Jwt.Actions

open System
open System.Text
open System.Security.Claims
open System.Threading.Tasks
open System.IdentityModel.Tokens.Jwt
open Microsoft.IdentityModel.Tokens

open Overwurd.Domain
open Overwurd.Domain.Jwt
open Overwurd.Domain.Users
open Overwurd.Domain.Common
open Overwurd.Domain.Jwt.Entities
open Overwurd.Domain.Users.Entities
open Overwurd.Domain.Common.Persistence

type GenerateGuid =
    unit -> Guid

type Dependencies =
    { GenerateGuid: GenerateGuid
      JwtConfiguration: JwtConfiguration
      JwtStorage: JwtStorage }

let getBytesFromSigningKey (key: string) =
    Encoding.UTF8.GetBytes key

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

type private AccessTokenDecryptionResult =
    { AccessTokenDecrypted: JwtSecurityToken }

let private decryptAccessTokenAsync (dependencies: Dependencies)
                                    (accessTokenEncrypted: string)
                                    : Result<AccessTokenDecryptionResult, RefreshError> Task =
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
                                        : Result<UserIdAndAccessTokenParsingResult, RefreshError> =
    let userIdClaimValue =
        decryptionResult.AccessTokenDecrypted.Claims
        |> Seq.filter (fun x -> x.Type = JwtRegisteredClaimNames.Sub)
        |> Seq.tryExactlyOne
        |> Option.bind (fun x -> UserId.tryParse x.Value)

    let accessTokenIdClaimValue = decryptionResult.AccessTokenDecrypted.Id |> JwtAccessTokenId.tryParse

    match userIdClaimValue, accessTokenIdClaimValue with
    | Some userId, Some accessTokenId ->
        { DecryptionResult = decryptionResult
          ParsingResult =
            { UserId = userId
              AccessTokenId = accessTokenId } }
        |> Ok
    | _ -> Error UserIdOrAccessTokenIdClaimsAreMissing

type private RefreshToken =
    { Value: JwtRefreshToken }

type private RefreshTokenResult =
    { DecryptionResult: AccessTokenDecryptionResult
      ParsingResult: ParsingResult
      RefreshToken: RefreshToken }

let private getActualRefreshTokenAsync (dependencies: Dependencies)
                                       (session: DbSession)
                                       (parsingResult: UserIdAndAccessTokenParsingResult)
                                       : Result<RefreshTokenResult, RefreshError> Task =
    task {
        let parsedIds = parsingResult.ParsingResult

        let! refreshToken =
            let getTokenByUserAndAccessToken =
                dependencies
                    .JwtStorage
                    .GetRefreshTokenByUserAndAccessTokenAsync
            getTokenByUserAndAccessToken parsedIds.UserId parsedIds.AccessTokenId session

        return
            match refreshToken with
            | Some token ->
                { DecryptionResult = parsingResult.DecryptionResult
                  ParsingResult = parsingResult.ParsingResult
                  RefreshToken = { Value = token } }
                |> Ok
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

let private validateRefreshTokenAsync (providedTokenValue: string)
                                      (now: UtcDateTime)
                                      (refreshTokenResult: RefreshTokenResult)
                                      : Result<RefreshTokenValidationResult, RefreshError> =
    let refreshToken = refreshTokenResult.RefreshToken.Value
    let doesTokenValueMatchToProvided = refreshToken.Value.ToString() = providedTokenValue
    let isNotExpired = refreshToken.ExpiresAt > now
    let isNotRevoked = not refreshToken.IsRevoked

    let isTokenValid = doesTokenValueMatchToProvided && isNotExpired && isNotRevoked

    if isTokenValid then
        { DecryptionResult = refreshTokenResult.DecryptionResult
          ParsingResult = refreshTokenResult.ParsingResult
          RefreshToken = refreshTokenResult.RefreshToken
          ValidationResult = { IsValid = true } }
        |> Ok
    else
        Error RefreshTokenNotValid

type private UpdateResult =
    { Tokens: JwtTokensPair
      AccessTokenExpiresAt: UtcDateTime
      RefreshTokenExpiresAt: UtcDateTime }

type private UpdateTokensResult =
    { DecryptionResult: AccessTokenDecryptionResult
      ParsingResult: ParsingResult
      RefreshToken: RefreshToken
      ValidationResult: ValidationResult
      UpdateResult: UpdateResult }

let private updateTokensAsync (dependencies: Dependencies)
                              (now: UtcDateTime)
                              (session: DbSession)
                              (validationResult: RefreshTokenValidationResult)
                              : Result<UpdateTokensResult, RefreshError> Task =
    task {
        let getUserClaims (claims: Claim list) =
            let userClaimTypes =
                [ dependencies.JwtConfiguration.ClaimsOptions.EmailClaimType
                  dependencies.JwtConfiguration.ClaimsOptions.RoleClaimType
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

        do!
            let updateRefreshTokenAsync =
                dependencies
                    .JwtStorage
                    .UpdateRefreshTokenAsync
            updateRefreshTokenAsync refreshToken.Id refreshTokenUpdateParameters session

        return
            { DecryptionResult = validationResult.DecryptionResult
              ParsingResult = validationResult.ParsingResult
              RefreshToken = validationResult.RefreshToken
              ValidationResult = validationResult.ValidationResult
              UpdateResult =
                  { Tokens =
                        { AccessTokenValue = newAccessTokenEncrypted
                          RefreshTokenValue = refreshToken.Value.ToString() }
                    AccessTokenExpiresAt = UtcDateTime.create (newAccessToken.ValidTo.ToUniversalTime())
                    RefreshTokenExpiresAt = refreshToken.ExpiresAt } }
            |> Ok
    }

let generateTokensPairAsync (dependencies: Dependencies)
                            (userId: UserId)
                            (claims: Claim list)
                            (now: UtcDateTime)
                            (session: DbSession)
                            : JwtGenerationResult Task =
    task {
        let nowUnwrapped = UtcDateTime.unwrap now
        let tokensConfiguration = dependencies.JwtConfiguration.TokensConfiguration

        let tokenHandler = JwtSecurityTokenHandler ()
        let tokenId = dependencies.GenerateGuid ()
        let jwtToken = createAccessToken dependencies.JwtConfiguration tokenId (UserId.unwrap userId) claims nowUnwrapped
        let jwtTokenEncrypted = tokenHandler.WriteToken jwtToken

        let! userRefreshTokens =
            let getUserRefreshTokensAsync =
                dependencies
                    .JwtStorage
                    .GetUserRefreshTokensAsync
            getUserRefreshTokensAsync userId session
        let tokenIdsToRemove = getTokenIdsToRemove tokensConfiguration.MaxTokensPerUser nowUnwrapped userRefreshTokens

        if not <| Set.isEmpty tokenIdsToRemove then
            do!
                let removeRefreshTokensAsync =
                    dependencies
                        .JwtStorage
                        .RemoveRefreshTokensAsync
                removeRefreshTokensAsync (tokenIdsToRemove |> Set.toList) session

        let refreshTokenValue = dependencies.GenerateGuid ()
        let refreshTokenExpiryDate = nowUnwrapped.AddDays tokensConfiguration.RefreshTokenExpirationInDays |> UtcDateTime.create
        let refreshTokenCreationParameters =
            { AccessTokenId = JwtAccessTokenId tokenId
              UserId = userId
              Value = refreshTokenValue
              CreatedAt = now
              RefreshedAt = None
              ExpiresAt = refreshTokenExpiryDate
              IsRevoked = false }
            
        let! _ =
            let createRefreshTokensAsync =
                dependencies
                    .JwtStorage
                    .CreateRefreshTokenAsync
            createRefreshTokensAsync refreshTokenCreationParameters session

        return
            { UserId = userId
              Tokens =
                { AccessTokenValue = jwtTokenEncrypted
                  RefreshTokenValue = refreshTokenValue.ToString() }
              AccessTokenExpiresAt = jwtToken.ValidTo.ToUniversalTime() |> UtcDateTime.create
              RefreshTokenExpiresAt = refreshTokenExpiryDate }
    }

let refreshAccessTokenAsync (dependencies: Dependencies)
                            (tokenValues: JwtTokensPair)
                            (now: UtcDateTime)
                            (session: DbSession)
                            : Result<JwtGenerationResult, RefreshError> Task =
    task {
        return!
            tokenValues.AccessTokenValue
            |> decryptAccessTokenAsync dependencies
            |> AsyncResult.synchronouslyBindTask parseUserIdAndAccessTokenId
            |> AsyncResult.asynchronouslyBindTask (getActualRefreshTokenAsync dependencies session)
            |> AsyncResult.synchronouslyBindTask (validateRefreshTokenAsync tokenValues.RefreshTokenValue now)
            |> AsyncResult.asynchronouslyBindTask (updateTokensAsync dependencies now session)
            |> AsyncResult.synchronouslyBindTask
                   (fun x ->
                        { UserId = x.ParsingResult.UserId
                          Tokens = x.UpdateResult.Tokens
                          AccessTokenExpiresAt = x.UpdateResult.AccessTokenExpiresAt
                          RefreshTokenExpiresAt = x.UpdateResult.RefreshTokenExpiresAt }
                        |> Ok)
    }
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

type ClaimsIdentityOptions =
    { RoleClaimType: string
      UserNameClaimType: string
      UserIdClaimType: string
      EmailClaimType: string
      SecurityStampClaimType: string }

type JwtRefreshTokenCreationParametersForPersistence =
    { AccessTokenId: JwtAccessTokenId
      Value: Guid
      UserId: UserId
      CreatedAt: CreationDate
      RefreshedAt: RefreshDate option
      ExpiresAt: ExpiryDate
      IsRevoked: bool }

type JwtRefreshTokenUpdateParametersForPersistence =
    { AccessTokenId: JwtAccessTokenId
      RefreshedAt: RefreshDate }

type RefreshAccessTokenError =
    | AccessTokenValidationError of ErrorMessage: string
    | UserIdOrAccessTokenIdClaimsAreMissing
    | RefreshTokenNotFound
    | RefreshTokenNotValid

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

type UpdateJwtRefreshTokenAsync =
    JwtRefreshTokenId -> JwtRefreshTokenUpdateParametersForPersistence -> unit Task

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

    let private createAccessToken (configuration: JwtConfiguration)
                                  (tokenId: Guid)
                                  (userId: int)
                                  (claims: Claim list)
                                  (now: DateTime)
                                  : JwtSecurityToken =
        let defaultClaims =
            [ Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString())
              Claim(JwtRegisteredClaimNames.Sub, userId.ToString()) ]

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
                            (now: UtcDateTime)
                            : JwtTokensPair Task =
        task {
            let nowUnwrapped = UtcDateTime.unwrap now

            let tokenHandler = JwtSecurityTokenHandler ()
            let tokenId = generateGuid ()
            let jwtToken = createAccessToken configuration tokenId (UserId.unwrap userId) claims nowUnwrapped
            let jwtTokenEncrypted = tokenHandler.WriteToken jwtToken

            let! userRefreshTokens = getUserTokensAsync userId
            let tokenIdsToRemove = getTokenIdsToRemove configuration.MaxTokensPerUser nowUnwrapped userRefreshTokens

            if not <| Set.isEmpty tokenIdsToRemove then
                do! removeTokensAsync (tokenIdsToRemove |> Set.toList)

            let refreshTokenValue = generateGuid ()
            let expiryDate = nowUnwrapped.AddDays configuration.RefreshTokenExpirationInDays
            let refreshTokenCreationParameters =
                { AccessTokenId = JwtAccessTokenId tokenId
                  UserId = userId
                  Value = refreshTokenValue
                  CreatedAt = CreationDate.create nowUnwrapped
                  RefreshedAt = None
                  ExpiresAt = ExpiryDate.create expiryDate
                  IsRevoked = false }

            let! _ = insertTokenAsync refreshTokenCreationParameters

            return
                { AccessTokenValue = jwtTokenEncrypted
                  RefreshTokenValue = refreshTokenValue.ToString() }
        }

    type private AccessTokenDecryptionResult =
        { AccessTokenDecrypted: JwtSecurityToken }

    let private decryptAccessTokenAsync (validationParameters: TokenValidationParameters)
                                        (accessTokenEncrypted: string)
                                        : Result<AccessTokenDecryptionResult, RefreshAccessTokenError> Task =
        task {
            let parameters = validationParameters.Clone ()
            parameters.ValidateLifetime <- false

            let tokenHandler = JwtSecurityTokenHandler ()
            let! validationResult = tokenHandler.ValidateTokenAsync(accessTokenEncrypted, validationParameters)

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

    let private getActualRefreshTokenAsync (getRefreshTokenByUserAndAccessTokenIdAsync: GetRefreshTokenByUserAndAccessTokenAsync)
                                           (parsingResult: UserIdAndAccessTokenParsingResult)
                                           : Result<RefreshTokenResult, RefreshAccessTokenError> Task =
        task {
            let parsedIds = parsingResult.ParsingResult
            let! refreshToken = getRefreshTokenByUserAndAccessTokenIdAsync parsedIds.UserId parsedIds.AccessTokenId

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
                                     (now: DateTime)
                                     (refreshTokenResult: RefreshTokenResult)
                                     : Result<RefreshTokenValidationResult, RefreshAccessTokenError> =
        let refreshToken = refreshTokenResult.RefreshToken.Value
        let doesTokenValueMatchToProvided = refreshToken.Value.ToString() = providedTokenValue
        let isNotExpired = ExpiryDate.unwrap refreshToken.ExpiresAt > now
        let isNotRevoked = not refreshToken.IsRevoked

        let isTokenValid = doesTokenValueMatchToProvided && isNotExpired && isNotRevoked

        if isTokenValid then
            Ok { DecryptionResult = refreshTokenResult.DecryptionResult
                 ParsingResult = refreshTokenResult.ParsingResult
                 RefreshToken = refreshTokenResult.RefreshToken
                 ValidationResult = { IsValid = true } }
        else
            Error RefreshTokenNotValid

    let private updateTokens (updateRefreshTokenAsync: UpdateJwtRefreshTokenAsync)
                             (generateGuid: GenerateGuid)
                             (configuration: JwtConfiguration)
                             (claimsIdentityOptions: ClaimsIdentityOptions)
                             (now: DateTime)
                             (validationResult: RefreshTokenValidationResult)
                             : Result<JwtTokensPair, RefreshAccessTokenError> Task =
        task {
            let getUserClaims (claims: Claim list) =
                let userClaimTypes =
                    [ claimsIdentityOptions.EmailClaimType
                      claimsIdentityOptions.RoleClaimType
                      claimsIdentityOptions.SecurityStampClaimType
                      claimsIdentityOptions.UserIdClaimType
                      claimsIdentityOptions.UserNameClaimType ]
                    |> Set.ofList
                claims |> List.filter (fun x -> userClaimTypes |> Set.contains x.Type)

            let newAccessTokenId = generateGuid ()
            let userId = validationResult.ParsingResult.UserId
            let claims = getUserClaims (validationResult.DecryptionResult.AccessTokenDecrypted.Claims |> List.ofSeq)
            let newAccessToken = createAccessToken configuration newAccessTokenId (UserId.unwrap userId) claims now

            let tokenHandler = JwtSecurityTokenHandler ()
            let newAccessTokenEncrypted = tokenHandler.WriteToken newAccessToken

            let refreshTokenUpdateParameters =
                { AccessTokenId = JwtAccessTokenId newAccessTokenId
                  RefreshedAt = RefreshDate.create now }

            let refreshToken = validationResult.RefreshToken.Value
            do! updateRefreshTokenAsync refreshToken.Id refreshTokenUpdateParameters

            return
                Ok { AccessTokenValue = newAccessTokenEncrypted
                     RefreshTokenValue = refreshToken.Value.ToString() }
        }

    let refreshAccessTokenAsync (getRefreshTokenByUserAndAccessTokenIdAsync: GetRefreshTokenByUserAndAccessTokenAsync)
                                (updateRefreshTokenAsync: UpdateJwtRefreshTokenAsync)
                                (generateGuid: GenerateGuid)
                                (configuration: JwtConfiguration)
                                (claimsIdentityOptions: ClaimsIdentityOptions)
                                (validationParameters: TokenValidationParameters)
                                (accessTokenEncrypted: string)
                                (refreshTokenValue: string)
                                (now: UtcDateTime)
                                : Result<JwtTokensPair, RefreshAccessTokenError> Task =
        task {
            let nowUnwrapped = UtcDateTime.unwrap now

            return!
                accessTokenEncrypted
                |> decryptAccessTokenAsync validationParameters
                |> AsyncResult.synchronouslyBindTask parseUserIdAndAccessTokenId
                |> AsyncResult.asynchronouslyBindTask (getActualRefreshTokenAsync getRefreshTokenByUserAndAccessTokenIdAsync)
                |> AsyncResult.synchronouslyBindTask (validateRefreshToken refreshTokenValue nowUnwrapped)
                |> AsyncResult.asynchronouslyBindTask (updateTokens updateRefreshTokenAsync generateGuid configuration claimsIdentityOptions nowUnwrapped)
        }
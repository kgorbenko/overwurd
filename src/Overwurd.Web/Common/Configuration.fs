module Overwurd.Web.Common.Configuration

open FsConfig
open System.Security.Claims
open Microsoft.Extensions.Configuration
open Microsoft.IdentityModel.Tokens

open Overwurd.Domain.Jwt
open Overwurd.Domain.Jwt.Actions

type AppConfig =
    { Jwt: TokensConfig }
and TokensConfig =
    { MaxTokensPerUser: int
      SigningKey: string
      Issuer: string
      Audience: string
      AccessTokenExpirationInMinutes: int
      RefreshTokenExpirationInDays: int }

let parseConfig (configuration: IConfiguration): AppConfig =
    let configurationRoot = configuration :?> IConfigurationRoot
    let appConfig = AppConfig configurationRoot

    let parsingResult = appConfig.Get<AppConfig>()

    match parsingResult with
    | Ok config -> config
    | Error error ->
        match error with
        | NotFound variableName ->
            failwith $"Required configuration variable {variableName} not found"
        | BadValue (variableName, value) ->
            failwith $"Configuration variable {variableName} has invalid value {value}"
        | NotSupported msg ->
            failwith msg

let connectionStringSection = "Default"

let getTokensConfiguration (configuration: IConfiguration): JwtTokensConfiguration =
    let appConfig = parseConfig configuration

    { SecurityAlgorithmSignature = SecurityAlgorithms.HmacSha256Signature
      MaxTokensPerUser = appConfig.Jwt.MaxTokensPerUser
      SigningKey = appConfig.Jwt.SigningKey
      Issuer = appConfig.Jwt.Issuer
      Audience = appConfig.Jwt.Audience
      AccessTokenExpirationInMinutes = appConfig.Jwt.AccessTokenExpirationInMinutes
      RefreshTokenExpirationInDays = appConfig.Jwt.RefreshTokenExpirationInDays }

let getValidationParameters (configuration: IConfiguration): TokenValidationParameters =
    let tokensConfiguration = getTokensConfiguration configuration

    TokenValidationParameters(
        ValidateIssuer = true,
        ValidIssuer = tokensConfiguration.Issuer,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = SymmetricSecurityKey(getBytesFromSigningKey tokensConfiguration.SigningKey),
        ValidateAudience = true,
        ValidAudience = tokensConfiguration.Audience,
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ValidAlgorithms = [ tokensConfiguration.SecurityAlgorithmSignature ]
    )

let getClaimsOptions (): ClaimsOptions =
    { RoleClaimType = ClaimTypes.Role
      UserNameClaimType = ClaimTypes.Name
      UserIdClaimType = ClaimTypes.NameIdentifier
      EmailClaimType = ClaimTypes.Email }
module Overwurd.Web.Handlers.Authentication

open System
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open Overwurd.Domain
open Overwurd.Domain.Jwt
open Overwurd.Domain.Users.Entities
open Overwurd.Domain.Features.Authentication
open Overwurd.Web.Common.Utils
open Overwurd.Web.Handlers.Common
open Overwurd.Web.DomainIntegration.Authentication

let loggingCategory = "Authentication"

type SignUpParameters =
    { Login: string
      Password: string }

type SignInParameters =
    { Login: string
      Password: string }

type RefreshParameters =
    { AccessToken: string
      RefreshToken: string }

type AuthResult =
    { Id: int
      Login: string
      AccessToken: string
      RefreshToken: string
      AccessTokenExpiresAt: DateTime }

type RefreshResult =
    { AccessToken: string
      ExpiresAt: DateTime }

let private makeResponseData (authData: SuccessfulAuthenticationData) =
    { Id = UserId.unwrap authData.User.Id
      Login = Login.unwrap authData.User.Login
      AccessToken = authData.Tokens.AccessTokenValue
      RefreshToken = authData.Tokens.RefreshTokenValue
      AccessTokenExpiresAt = UtcDateTime.unwrap authData.AccessTokenExpiresAt }

let private signUp (parameters: SignUpParameters): HttpHandler =
    fun (_: HttpFunc) (ctx: HttpContext) ->
        task {
            let now = UtcDateTime.now ()

            let credentials: RawCredentials =
                { Login = parameters.Login
                  Password = parameters.Password }

            let! signUpResult =
                signUpAsync now credentials ctx
                |> withConnectionAsync ctx

            let logger = ctx.GetLogger(loggingCategory)
            match signUpResult with
            | Ok data ->
                logger.LogInformation("User (#{Id} '{Login}') has signed up", UserId.unwrap data.User.Id, Login.unwrap data.User.Login)
                return! json (makeResponseData data) earlyReturn ctx
            | Error (ValidationError messages) ->
                logger.LogInformation("Unsuccessful attempt to sign up with login '{Login}': either login or password were not valid", credentials.Login)
                return! RequestErrors.badRequest (json {| Errors = messages |}) earlyReturn ctx
            | Error LoginIsOccupied ->
                logger.LogInformation("Unsuccessful attempt to sign up with login '{Login}': login is occupied", credentials.Login)
                return! RequestErrors.badRequest (json {| Errors = [ "Login is occupied." ] |}) earlyReturn ctx
        }

let private signIn (parameters: SignInParameters): HttpHandler =
    fun (_: HttpFunc) (ctx: HttpContext) ->
        task {
            let now = UtcDateTime.now ()

            let credentials: RawCredentials =
                { Login = parameters.Login
                  Password = parameters.Password }

            let! signInResult =
                signInAsync credentials now ctx
                |> withConnectionAsync ctx

            let logger = ctx.GetLogger(loggingCategory)
            match signInResult with
            | Ok data ->
                logger.LogInformation("User (#{Id} '{Login}') has signed in", UserId.unwrap data.User.Id, Login.unwrap data.User.Login)
                return! json (makeResponseData data) earlyReturn ctx
            | Error UserDoesNotExist ->
                logger.LogInformation("Unsuccessful attempt to sign in by login '{Login}': user with such login does not exist", credentials.Login)
                return! RequestErrors.badRequest (json {| Errors = [ "Invalid login or password." ] |}) earlyReturn ctx
            | Error InvalidPassword ->
                logger.LogInformation("Unsuccessful attempt to sign in by login '{Login}': password is invalid", credentials.Login)
                return! RequestErrors.badRequest (json {| Errors = [ "Invalid login or password." ] |}) earlyReturn ctx
        }

let private refresh (parameters: RefreshParameters): HttpHandler =
    fun (_: HttpFunc) (ctx: HttpContext) ->
        task {
            let now = UtcDateTime.now ()

            let tokenValuesPair: JwtTokensPair =
                { AccessTokenValue = parameters.AccessToken
                  RefreshTokenValue = parameters.RefreshToken }

            let! refreshResult =
                refreshAsync tokenValuesPair now ctx
                |> withConnectionAsync ctx

            let logger = ctx.GetLogger(loggingCategory)
            match refreshResult with
            | Ok data ->
                logger.LogInformation("User (#{Id}) has refreshed access token", data.User.Id)
                return! json (makeResponseData data) earlyReturn ctx
            | Error (AccessTokenValidationError errorMessage) ->
                logger.LogInformation("Refresh attempt failed by token '{Token}': {ErrorMessage}", parameters.AccessToken, errorMessage)
                return! setStatusCode StatusCodes.Status400BadRequest earlyReturn ctx
            | Error UserIdOrAccessTokenIdClaimsAreMissing ->
                logger.LogInformation("Refresh attempt failed by token '{Token}': either UserId or AccessTokenId claims are missing", parameters.AccessToken)
                return! setStatusCode StatusCodes.Status400BadRequest earlyReturn ctx
            | Error RefreshTokenNotFound ->
                logger.LogInformation("Refresh attempt failed by token '{Token}': refresh token is missing", parameters.AccessToken)
                return! setStatusCode StatusCodes.Status400BadRequest earlyReturn ctx
            | Error RefreshTokenNotValid ->
                logger.LogInformation("Refresh attempt failed by token '{Token}': refresh token is not valid", parameters.AccessToken)
                return! setStatusCode StatusCodes.Status400BadRequest earlyReturn ctx
        }

let handle: HttpHandler =
    choose [
        POST >=> route "/api/auth/signup" >=> bind<SignUpParameters> signUp
        POST >=> route "/api/auth/signin" >=> bind<SignInParameters> signIn
        POST >=> route "/api/auth/refresh" >=> bind<RefreshParameters> refresh
    ]
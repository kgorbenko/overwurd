module Overwurd.Web.Handlers.Authentication

open System
open Giraffe
open Microsoft.AspNetCore.Http

open Overwurd.Web.Handlers.Common
open Overwurd.Domain
open Overwurd.Domain.Features.Authentication
open Overwurd.Domain.Jwt
open Overwurd.Domain.Users.Entities
open Overwurd.Web.Common.Utils
open Overwurd.Web.DomainIntegration.Authentication

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

let private makeSuccessfulResponse (authData: SuccessfulAuthenticationData) =
    { Id = UserId.unwrap authData.User.Id
      Login = Login.unwrap authData.User.Login
      AccessToken = authData.Tokens.AccessTokenValue
      RefreshToken = authData.Tokens.RefreshTokenValue
      AccessTokenExpiresAt = UtcDateTime.unwrap authData.AccessTokenExpiresAt }

let signUp: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! parameters = ctx.BindJsonAsync<SignUpParameters>()
            let now = UtcDateTime.now ()

            let! signUpResult =
                signUpAsync now parameters.Login parameters.Password ctx
                |> withConnectionAsync ctx

            match signUpResult with
            | Ok data ->
                let authResult = makeSuccessfulResponse data
                return! json authResult finish ctx
            | Error (ValidationError messages) ->
                return!
                    setStatusCode StatusCodes.Status400BadRequest
                    >=> json messages
                    <|| (finish, ctx)
            | Error LoginIsOccupied ->
                return!
                    setStatusCode StatusCodes.Status409Conflict
                    >=> json [ "Login has already been taken." ]
                    <|| (finish, ctx)
        }

let signIn: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! parameters = ctx.BindJsonAsync<SignInParameters>()
            let now = UtcDateTime.now ()

            let credentials: Credentials =
                { Login = parameters.Login
                  Password = parameters.Password }
            
            let! signInResult =
                signInAsync credentials now ctx
                |> withConnectionAsync ctx

            match signInResult with
            | Ok data ->
                let authResult = makeSuccessfulResponse data
                return! json authResult finish ctx
            | Error _ ->
                return! setStatusCode StatusCodes.Status400BadRequest finish ctx
        }

let refresh: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! parameters = ctx.BindJsonAsync<RefreshParameters>()
            let now = UtcDateTime.now ()
            
            let tokenValuesPair: JwtTokensPair =
                { AccessTokenValue = parameters.AccessToken
                  RefreshTokenValue = parameters.RefreshToken }
            
            let! refreshResult =
                refreshAsync tokenValuesPair now ctx
                |> withConnectionAsync ctx
            
            match refreshResult with
            | Ok data ->
                let authResult = makeSuccessfulResponse data
                return! json authResult finish ctx
            | Error _ ->
                return! setStatusCode StatusCodes.Status400BadRequest finish ctx
        }
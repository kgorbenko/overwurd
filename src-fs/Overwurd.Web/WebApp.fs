module Overwurd.Web.WebApp

open Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer

open Overwurd.Web.Handlers.Status
open Overwurd.Web.Handlers.Authentication

let authorize: HttpHandler =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let webApp: HttpHandler =
    choose [
        GET >=> route "/api/status" >=> status
        POST >=> route "/api/auth/signup" >=> signUp
        POST >=> route "/api/auth/signin" >=> signIn
        POST >=> route "/api/auth/refresh" >=> refresh
        setStatusCode 404 >=> text "Not Found"
    ]
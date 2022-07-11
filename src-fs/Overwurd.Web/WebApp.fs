module Overwurd.Web.WebApp

open Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer

open Overwurd.Web.Handlers

let authorize: HttpHandler =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let webApp: HttpHandler =
    choose [
        Status.handle
        Authentication.handle
    ]
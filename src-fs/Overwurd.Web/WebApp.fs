module Overwurd.Web.WebApp

open Giraffe.Core

open Overwurd.Web.Handlers

let webApp: HttpHandler =
    choose [
        Status.handle
        setStatusCode 404 >=> text "Not Found"
    ]
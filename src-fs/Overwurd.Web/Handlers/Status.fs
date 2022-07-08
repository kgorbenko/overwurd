module Overwurd.Web.Handlers.Status

open Giraffe.Core
open Microsoft.AspNetCore.Http

open Overwurd.Web.Handlers.Common
open Overwurd.Web.Common.Utils
open Overwurd.Web.DomainIntegration.Status

type ApplicationStatus =
    { ApplicationVersion: string
      DatabaseVersion: string }

let status: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! status =
                getApplicationStatus
                |> withConnectionAsync ctx

            let jsonStatus =
                { ApplicationVersion = status.ApplicationVersion.ToString()
                  DatabaseVersion = status.DatabaseVersion.ToString() }
            
            return! json jsonStatus finish ctx
        }
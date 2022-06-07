module Overwurd.Web.Handlers.Status

open Giraffe.Core
open Giraffe.Routing
open System.Reflection
open Microsoft.AspNetCore.Http

open Overwurd.Infrastructure.Database
open Overwurd.Web.Common.Utils

type ApplicationStatus =
    { ApplicationVersion: string
      DatabaseVersion: string }

let private handleStatus: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let appVersion =
                Assembly.GetExecutingAssembly()
                        .GetName()
                        .Version
                        
            let! dbVersion =
                StatusStore.getDatabaseVersionAsync
                |> withConnectionAsync ctx
            
            let status =
                { ApplicationVersion = appVersion.ToString()
                  DatabaseVersion = dbVersion.ToString() }
                
            return! json status next ctx
        }

let handle: HttpHandler =
    choose [
        GET >=> route "/api/status" >=> handleStatus
    ]
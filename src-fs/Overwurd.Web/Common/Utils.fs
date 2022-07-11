module Overwurd.Web.Common.Utils

open System.Data
open System.Threading
open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration

open Overwurd.Infrastructure.Database

let getConnectionString (ctx: HttpContext): string =
    let configuration = ctx.GetService<IConfiguration> ()
    configuration.GetConnectionString(Configuration.connectionStringSection)

let withConnectionAsync (ctx: HttpContext) (queryAsync: CancellationToken -> IDbConnection -> 'a Task) =
    task {
        let connectionString = ctx |> getConnectionString
        return! queryAsync ctx.RequestAborted |> Connection.withConnectionAsync connectionString
    }
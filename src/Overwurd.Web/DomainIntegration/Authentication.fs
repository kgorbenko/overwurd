module Overwurd.Web.DomainIntegration.Authentication

open System.Data
open System.Threading
open Microsoft.AspNetCore.Http
open Overwurd.Domain.Common
open Overwurd.Domain.Features.Authentication
open Overwurd.Domain.Jwt
open Overwurd.Web.DomainIntegration.Common
open Overwurd.Domain.Features.Authentication.Workflows

let signUpAsync
    (now: UtcDateTime)
    (credentials: RawCredentials)
    (ctx: HttpContext)
    (cancellationToken: CancellationToken)
    (connection: IDbConnection) =
        task {
            let dependencies = getAuthDependencies ctx
            return! signUpAsync dependencies now credentials connection cancellationToken
        }

let refreshAsync
    (tokenValuesPair: JwtTokensPair)
    (now: UtcDateTime)
    (ctx: HttpContext)
    (cancellationToken: CancellationToken)
    (connection: IDbConnection) =
        task {
            let dependencies = getAuthDependencies ctx
            return! refreshAsync dependencies tokenValuesPair now connection cancellationToken
        }

let signInAsync
    (credentials: RawCredentials)
    (now: UtcDateTime)
    (ctx: HttpContext)
    (cancellationToken: CancellationToken)
    (connection: IDbConnection) =
        task {
            let dependencies = getAuthDependencies ctx
            return! signInAsync dependencies credentials now connection cancellationToken
        }
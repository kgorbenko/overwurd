module Overwurd.Web.DomainIntegration.Status

open System.Data
open System.Threading
open System.Threading.Tasks
open Overwurd.Domain.Features.Status
open Overwurd.Domain.Features.Status.Workflows
open Overwurd.Infrastructure.Database

let getApplicationStatus
    (cancellationToken: CancellationToken)
    (connection: IDbConnection)
    : ApplicationStatus Task =
        task {
            return! getApplicationStatus
                StatusStorage.getDatabaseVersionAsync
                cancellationToken
                connection
        }
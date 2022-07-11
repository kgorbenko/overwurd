module Overwurd.Domain.Features.Status.Workflows

open System
open System.Data
open System.Reflection
open System.Threading

open System.Threading.Tasks
open Overwurd.Domain.Common.Persistence
open Overwurd.Domain.Features.Status

type GetDatabaseVersionAsync =
    DbSession -> Version Task

let getApplicationStatus (getDatabaseVersionAsync: GetDatabaseVersionAsync)
                         (cancellationToken: CancellationToken)
                         (connection: IDbConnection)
                         : ApplicationStatus Task =
    task {
        let! databaseVersion =
            getDatabaseVersionAsync
            |> inTransactionAsync connection cancellationToken

        let applicationVersion =
            Assembly.GetExecutingAssembly()
                .GetName()
                .Version

        return
            { DatabaseVersion = databaseVersion
              ApplicationVersion = applicationVersion }
    }
module Overwurd.Infrastructure.Tests.Common.Utils

open System.Threading
open Microsoft.Extensions.Configuration
open NUnit.Framework
open Npgsql

open Overwurd.Infrastructure.Database
open System.Threading.Tasks
open Overwurd.Domain.Common.Persistence
open Overwurd.Infrastructure.Tests.Common.Database

let private getConnectionString () =
    let configuration =
        ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("appsettings.tests.json")
            .AddEnvironmentVariables()
            .Build()

    configuration.GetConnectionString("Default")

let withConnectionAsync (queryAsync: DbSession -> 'a Task): 'a Task =
    task {
        let connectionString = getConnectionString ()
        use connection = new NpgsqlConnection(connectionString)
        do! connection.OpenAsync()

        use! transaction = connection.BeginTransactionAsync()
        let session =
            { Connection = connection
              Transaction = transaction
              CancellationToken = CancellationToken.None }

        let! result = queryAsync session
        do! transaction.CommitAsync()

        return result
    }

let prepareDatabaseAsync (session: DbSession): unit Task =
    task {
        do Dapper.registerTypeHandlers()
        do! clearAsync session
        ()
    }
module Overwurd.Infrastructure.Tests.Common.Utils

open Microsoft.Extensions.Configuration
open NUnit.Framework
open System.Threading.Tasks

open Overwurd.Infrastructure.Database
open Overwurd.Infrastructure.Database.Database
open Overwurd.Infrastructure.Tests.Common.Database

let private getConnectionString () =
    let configuration =
        ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("appsettings.tests.json")
            .AddEnvironmentVariables()
            .Build()

    configuration.GetConnectionString("Default")

let withConnectionAsync (queryAsync: Session -> 'a Task): 'a Task =
    task {
        let connectionString = getConnectionString ()
        return! withConnectionAsync queryAsync connectionString
    }

let prepareDatabaseAsync (session: Session): unit Task =
    task {
        do! clearAsync session
        ()
    }
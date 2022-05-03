namespace Overwurd.Infrastructure.Tests

module Common =

    open System.Threading.Tasks

    open Microsoft.Extensions.Configuration
    open Npgsql
    open NUnit.Framework

    open Overwurd.Infrastructure.Common

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
            use connection = new NpgsqlConnection(connectionString)
            do! connection.OpenAsync()

            use transaction = connection.BeginTransaction()
            let session = { Connection = connection; Transaction = transaction; }

            let! result = queryAsync session
            transaction.Commit()

            return result
        }

    let setupPrerequisites (session: Session): unit Task =
        task {
            do! Database.clear session
            ()
        }

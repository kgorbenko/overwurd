namespace Overwurd.DataAccess.Database

open System.Data
open System.Threading.Tasks
open Npgsql

module Connection =

    let private makeConnectionAsync (queryAsync: IDbConnection -> 'a Task)
                                    (connectionString: string)
                                    : 'a Task =
        task {
            use connection = new NpgsqlConnection(connectionString)
            do! connection.OpenAsync()

            let! result = queryAsync connection

            return result
        }

    let withConnectionAsync (connectionString: string)
                            (queryAsync: IDbConnection -> 'a Task)
                            : 'a Task =
        task {
            return! makeConnectionAsync queryAsync connectionString
        }
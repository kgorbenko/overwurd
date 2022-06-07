﻿namespace Overwurd.Infrastructure.Database

open System.Data
open System.Threading.Tasks
open Npgsql

type Session =
    { Connection: IDbConnection
      Transaction: IDbTransaction }

module Connection =

    let private makeConnectionAsync (queryAsync: Session -> 'a Task)
                                    (connectionString: string)
                                    : 'a Task =
        task {
            use connection = new NpgsqlConnection(connectionString)
            do! connection.OpenAsync()

            use transaction = connection.BeginTransaction()
            let session = { Connection = connection; Transaction = transaction; }

            let! result = queryAsync session
            transaction.Commit()

            return result
        }

    let withConnectionAsync (connectionString: string)
                            (queryAsync: Session -> 'a Task)
                            : 'a Task =
        task {
            return! makeConnectionAsync queryAsync connectionString
        }
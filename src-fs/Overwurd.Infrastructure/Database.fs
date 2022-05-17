namespace Overwurd.Infrastructure

open System.Threading

module Dapper =

    open System
    open Dapper

    type OptionHandler<'T>() =
        inherit SqlMapper.TypeHandler<option<'T>>()

        override this.SetValue(param, value) =
            let valueOrNull =
                match value with
                | Some x -> box x
                | None -> null

            param.Value <- valueOrNull

        override this.Parse value =
            if isNull value || value = box DBNull.Value
            then None
            else Some (value :?> 'T)

    let registerTypeHandlers () =
        SqlMapper.AddTypeHandler (OptionHandler<Guid>())
        SqlMapper.AddTypeHandler (OptionHandler<int64>())
        SqlMapper.AddTypeHandler (OptionHandler<int>())
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        SqlMapper.AddTypeHandler (OptionHandler<DateTimeOffset>())
        ()

module Database =

    open System.Data
    open System.Threading.Tasks
    open Npgsql

    module Utils =

        open Dapper
        open System

        let makeSqlCommand (sql: string)
                           (transaction: IDbTransaction)
                           (cancellationToken: CancellationToken)
                           : CommandDefinition =
            CommandDefinition (
                commandText = sql,
                    transaction = transaction,
                        cancellationToken = cancellationToken
            )

        let makeSqlCommandWithParameters (sql: string)
                                         (parameters: Object)
                                         (transaction: IDbTransaction)
                                         (cancellationToken: CancellationToken)
                                         : CommandDefinition =
            CommandDefinition (
                commandText = sql,
                    parameters = parameters,
                        transaction = transaction,
                            cancellationToken = cancellationToken
            )

    type Session = {
        Connection: IDbConnection
        Transaction: IDbTransaction
    }

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

    let withConnectionAsync (queryAsync: Session -> 'a Task)
                            (connectionString: string)
                            : 'a Task =
        task {
            do Dapper.registerTypeHandlers ()
            return! makeConnectionAsync queryAsync connectionString
        }


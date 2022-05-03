namespace Overwurd.Infrastructure.Tests

module Utils =

    open System
    open System.Data
    open Dapper

    let makeCommand (sql: string)
                    (transaction: IDbTransaction)
                    : CommandDefinition =
        CommandDefinition (
            commandText = sql,
                transaction = transaction
        )

    let makeCommandWithParameters (sql: string)
                                  (parameters: Object)
                                  (transaction: IDbTransaction)
                                  : CommandDefinition =
        CommandDefinition (
            commandText = sql,
                parameters = parameters,
                    transaction = transaction
        )


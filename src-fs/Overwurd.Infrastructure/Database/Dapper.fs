module Overwurd.Infrastructure.Database.Dapper

open Dapper
open System
open System.Data
open System.Threading
open System.Threading.Tasks

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
    SqlMapper.AddTypeHandler (OptionHandler<string>())
    SqlMapper.AddTypeHandler (OptionHandler<DateTime>())
    ()

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

let executeAsync (command: CommandDefinition) (connection: IDbConnection): unit Task =
    task {
        let! _ = connection.ExecuteAsync(command)
        ()
    }

let queryAsync<'a> (command: CommandDefinition) (connection: IDbConnection): 'a list Task =
    task {
        let! result = connection.QueryAsync<'a>(command)
        return result |> List.ofSeq
    }

let findAsync<'a> (command: CommandDefinition) (connection: IDbConnection): 'a option Task =
    task {
        let! result = queryAsync<'a> command connection

        return
            match result with
            | [] -> None
            | [x] -> Some x
            | _ -> raise (InvalidOperationException "Found more than one element")
    }

let querySingleAsync<'a> (command: CommandDefinition) (connection: IDbConnection): 'a Task =
    task {
        let! result = queryAsync<'a> command connection

        return
            match result with
            | [] -> raise (InvalidOperationException "Sequence contains no elements")
            | [x] -> x
            | _ -> raise (InvalidOperationException "Sequence contains more than one element")
    }
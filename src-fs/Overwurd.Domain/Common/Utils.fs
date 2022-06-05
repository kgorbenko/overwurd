module internal Overwurd.Domain.Common.Utils

open System
open System.Text
open System.Threading.Tasks

let ensureUtc (dateTime: DateTime): DateTime =
    match dateTime.Kind with
    | DateTimeKind.Utc -> dateTime
    | _ -> raise (InvalidOperationException "Only UTC dates are allowed.")

let tryParseInt (value: string): int Option =
    match Int32.TryParse value with
    | true, value -> Some value
    | false, _ -> None
    
let toUpperCase (value: string): string =
    value.ToUpperInvariant()

let toByteArray (value: string): byte array =
    Encoding.UTF8.GetBytes value

module AsyncResult =

    let asynchronouslyBind (binder: 'T -> Task<Result<'U, 'TError>>)
                           (result: Result<'T, 'TError>)
                           : Task<Result<'U, 'TError>> =
        task {
            match result with
            | Ok r -> return! binder r
            | Error e -> return Error e
        }

    let asynchronouslyBindTask (binder: 'T -> Task<Result<'U, 'TError>>)
                               (resultTask: Task<Result<'T, 'TError>>)
                               : Task<Result<'U, 'TError>> =
        task {
            let! result = resultTask
            match result with
            | Ok r -> return! binder r
            | Error e -> return Error e
        }

    let synchronouslyBindTask (binder: 'T -> Result<'U, 'TError>)
                              (resultTask: Task<Result<'T, 'TError>>)
                              : Task<Result<'U, 'TError>> =
        task {
            let! result = resultTask
            match result with
            | Ok r -> return binder r
            | Error e -> return Error e
        }

module Overwurd.Domain.Common.AsyncResult

open System.Threading.Tasks

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
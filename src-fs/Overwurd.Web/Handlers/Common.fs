module Overwurd.Web.Handlers.Common

open Giraffe
open System.Threading.Tasks
open Microsoft.AspNetCore.Http

open Overwurd.Domain.Common.Validation
open Overwurd.Web.Common

let earlyReturn: HttpFunc = Some >> Task.FromResult

let skip: HttpFuncResult = Task.FromResult None

type ValidationResponse =
    { Errors: ValidationErrorMessage list }

let private bindModel<'a> (success: 'a -> HttpFunc -> HttpContext -> HttpFuncResult): HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! model = ctx.TryBindJsonAsync<'a>()
            
            return!
                match model with
                | Some bound -> success bound next ctx
                | None -> setStatusCode StatusCodes.Status400BadRequest earlyReturn ctx
        }

let private validateModel<'a> (validator: 'a -> ValidationResult) (success: 'a -> HttpFunc -> HttpContext -> HttpFuncResult) (model: 'a) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let validationResult = validator model
            
            return!
                match validationResult with
                | Success -> success model next ctx
                | Fail messages -> RequestErrors.badRequest (json { Errors = messages }) next ctx
        }

let bind<'a> (success: 'a -> HttpFunc -> HttpContext -> HttpFuncResult) : HttpHandler =
    bindModel<'a> success

let validate<'a> (validator : 'a -> ValidationResult) (success: 'a -> HttpFunc -> HttpContext -> HttpFuncResult) : HttpHandler =
    bindModel<'a> (validateModel<'a> validator success)
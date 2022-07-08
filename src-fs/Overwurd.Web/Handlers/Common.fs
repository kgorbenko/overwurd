module Overwurd.Web.Handlers.Common

open Giraffe
open System.Threading.Tasks

let finish: HttpFunc = Some >> Task.FromResult
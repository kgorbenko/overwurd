module Overwurd.Domain.Common.Validation

open System

type ValidationErrorMessage = string

type ValidationResult =
    | Ok
    | Error of ValidationErrorMessage

exception ValidationException of string

let (|NullOrWhiteSpace|_|) (str: string): unit option =
    if String.IsNullOrWhiteSpace(str)
        then Some ()
        else None

let (|LacksLength|_|) (minLength: int) (str: string): unit option =
    if str.Length < minLength
        then Some ()
        else None

let (|ExceedsMaxLength|_|) (maxLength: int) (str: string): unit option =
    if str.Length > maxLength
        then Some ()
        else None

let (|HasInvalidCharacters|_|) (validCharacters: char Set) (str: string): char list option =
    str
    |> List.ofSeq
    |> List.filter (validCharacters.Contains >> not)
    |> function
        | [] -> None
        | invalidCharacters -> Some invalidCharacters
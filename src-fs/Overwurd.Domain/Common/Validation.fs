module Overwurd.Domain.Common.Validation

open System

type ValidationErrorMessage = string

type ValidationResult =
    | Success
    | Fail of ValidationErrorMessage list

exception ValidationException of string list

let isNullOrWhiteSpace (str: string): bool =
    String.IsNullOrWhiteSpace(str)

let lacksLength (minLength: int) (str: string): bool =
    str.Length < minLength

let exceedsMaxLength (maxLength: int) (str: string): bool =
    str.Length > maxLength

let getInvalidCharacters (validCharacters: char Set) (str: string): char list =
    str
    |> List.ofSeq
    |> List.filter (validCharacters.Contains >> not)

let hasInvalidCharacters (validCharacters: char Set) (str: string): bool =
    getInvalidCharacters validCharacters str
    |> List.isEmpty
    |> not

let bothUpperAndLowerCharactersPresent (str: string): bool =
    let hasUpperCharacters = str |> Seq.exists Char.IsUpper
    let hasLowerCharacters = str |> Seq.exists Char.IsLower
    
    hasUpperCharacters && hasLowerCharacters
    
let getValidationErrors (ruleMessages: (('a -> bool) * ValidationErrorMessage) list)
                        (value: 'a): ValidationErrorMessage list =
    seq {
        for ruleMessage in ruleMessages do
            let rule, message = ruleMessage
            if rule value then
                yield message
    } |> List.ofSeq

let validate ruleMessages value =
    getValidationErrors ruleMessages value
    |> function
        | [] -> Success
        | messages -> Fail messages
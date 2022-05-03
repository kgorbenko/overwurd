namespace Overwurd.Domain.Common

module Validation =

    type ValidationErrorMessage = string

    type ValidationResult =
        | Ok
        | Error of ValidationErrorMessage

    exception ValidationException

    let (|WhiteSpace|_|) (str: string): unit option =
        if System.String.IsNullOrWhiteSpace(str)
            then Some ()
            else None

    let (|ExceedsMaxLength|_|) (maxLength: int) (str: string): unit option =
        if str.Length > maxLength
            then Some ()
            else None
namespace Overwurd.Domain

module User =

    type UserId =
        UserId of int

    type Login =
        private Login of string

    type PasswordHash =
        private PasswordHash of string

    module UserId =

        let unwrap (userId: UserId): int =
            match userId with
            | UserId value -> value

    module Login =

        open Overwurd.Domain.Common.Validation

        let allowedCharacters = Set "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._"

        let validate (login: string): ValidationResult =
            let minLength = 8
            let maxLength = 30

            match login with
            | NullOrWhiteSpace -> Error "Login cannot be empty."
            | LacksLength minLength -> Error $"Login cannot be shorter than {minLength} characters."
            | ExceedsMaxLength maxLength -> Error $"Login cannot be longer than {maxLength} characters."
            | HasInvalidCharacters allowedCharacters _ -> Error "Valid characters are lowercase and uppercase Latin letters, digits, '-', '.' and '_'."
            | _ -> Ok

        let create (login: string): Login =
            login
            |> validate
            |> function
                | Ok -> Login login
                | Error message -> raise (ValidationException message)

        let unwrap (login: Login): string =
            match login with
            | Login value -> value

    module PasswordHash =

        open Overwurd.Domain.Common.Validation

        let validate (passwordHash: string): ValidationResult =
            match passwordHash with
            | NullOrWhiteSpace -> Error "Password hash cannot be empty."
            | _ -> Ok

        let create (passwordHash: string): PasswordHash =
            passwordHash
            |> validate
            |> function
                | Ok -> PasswordHash passwordHash
                | Error message -> raise (ValidationException message)

        let unwrap (passwordHash: PasswordHash): string =
            match passwordHash with
            | PasswordHash value -> value

    open System
    open Overwurd.Domain.Common.Consistency

    type User =
        { Id: UserId
          CreatedAt: CreationDate
          Login: Login
          PasswordHash: PasswordHash }

    type UserCreationParametersForPersistence =
        { CreatedAt: DateTime
          Login: string
          NormalizedLogin: string
          PasswordHash: string }
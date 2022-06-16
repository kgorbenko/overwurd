﻿namespace Overwurd.Domain

open System
open System.Security.Cryptography
open System.Threading.Tasks
open Konscious.Security.Cryptography

open Overwurd.Domain
open Overwurd.Domain.Common.Utils

type UserId =
    UserId of int

type Login =
    private Login of string

type NormalizedLogin =
    private NormalizedLogin of string

type Password =
    private Password of string

type PasswordHash = string

type PasswordSalt = string

type PasswordHashAndSalt =
    { Hash: PasswordHash
      Salt: PasswordSalt }

type User =
    { Id: UserId
      CreatedAt: UtcDateTime
      Login: Login
      PasswordHash: PasswordHash
      PasswordSalt: PasswordSalt }

type UserCreationParameters =
    { CreatedAt: UtcDateTime
      Login: Login
      Password: Password }
    
type UserCreationParametersForPersistence =
    { CreatedAt: UtcDateTime
      Login: Login
      NormalizedLogin: NormalizedLogin
      PasswordHash: PasswordHash
      PasswordSalt: PasswordSalt }

type FindUserByIdAsync =
    UserId -> User option Task

type FindUserByNormalizedLoginAsync =
    NormalizedLogin -> User option Task

type CreateUserAsync =
    UserCreationParametersForPersistence -> UserId Task

type FindUserPasswordHashAndSalt =
    UserId -> PasswordHashAndSalt option Task
    
type UserPersister =
    { FindUserByIdAsync: FindUserByIdAsync
      FindUserByNormalizedLoginAsync: FindUserByNormalizedLoginAsync
      FindUserPasswordHashAndSalt: FindUserPasswordHashAndSalt
      CreateUserAsync: CreateUserAsync }

type UserActionsDependencies =
    { UserPersister: UserPersister }

type UserCreationResultError =
    | LoginIsOccupied

module User =

    module UserId =

        let unwrap (userId: UserId): int =
            match userId with
            | UserId value -> value

        let tryParse (userId: string): UserId option =
            userId
            |> tryParseInt
            |> Option.map UserId

    module Login =

        open Overwurd.Domain.Common.Validation

        let allowedCharacters = Set "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._"

        let validate (login: string): ValidationResult =
            let minLength = 8
            let maxLength = 30

            let rules =
                [ isNullOrWhiteSpace, "Login cannot be empty."
                  lacksLength minLength, $"Login cannot be shorter than {minLength} characters."
                  exceedsMaxLength maxLength, $"Login cannot be longer than {maxLength} characters."
                  hasInvalidCharacters allowedCharacters, "Valid characters are lowercase and uppercase Latin letters, digits, '-', '.' and '_'." ]
            
            validate rules login

        let create (login: string): Login =
            login
            |> validate
            |> function
                | Ok -> Login login
                | Error messages -> raise (ValidationException messages)
            
        let internal createBypassingValidation (login: string) =
            Login login

        let unwrap (login: Login): string =
            match login with
            | Login value -> value
    
    module NormalizedLogin =
        
        let create (login: Login): NormalizedLogin =
            login
            |> Login.unwrap
            |> toUpperCase
            |> NormalizedLogin
            
        let unwrap (login: NormalizedLogin): string =
            match login with
            | NormalizedLogin value -> value

    module Password =
        
        open Overwurd.Domain.Common.Validation
        
        let validate (password: string): ValidationResult =
            let minLength = 8
            
            let rules =
                [ isNullOrWhiteSpace, "Password cannot be empty.";
                  lacksLength minLength, $"Password should be at least {minLength} characters length.";
                  bothUpperAndLowerCharactersPresent, "Password should contain both uppercase and lowercase characters." ]

            validate rules password
        
        let create (password: string): Password =
            password
            |> validate
            |> function
                | Ok -> Password password
                | Error messages -> raise (ValidationException messages)
        
        let internal createBypassingValidation (password: string): Password =
            Password password
        
        let unwrap (password: Password): string =
            match password with
            | Password value -> value

    module private PasswordHash =

        let private saltLength = 16
        let private hashLength = 64
        
        let private hashAsync (password: string) (saltBytes: byte array) (hashLength: int): string Task =
            task {
                let passwordBytes = password |> toByteArray
                
                use argon2 = new Argon2id(passwordBytes)
                argon2.Salt <- saltBytes
                argon2.DegreeOfParallelism <- 1
                argon2.Iterations <- 2
                argon2.MemorySize <- 2024
                
                let! hashBytes = argon2.GetBytesAsync hashLength
                return Convert.ToBase64String hashBytes
            }
        
        let generateAsync (password: Password): PasswordHashAndSalt Task =
            task {
                let generateSalt saltLength =
                    RandomNumberGenerator.GetBytes(saltLength)
                
                let saltBytes = generateSalt saltLength
                let! hash = hashAsync (Password.unwrap password) saltBytes hashLength
                
                let salt = Convert.ToBase64String saltBytes
                
                return
                    { Hash = hash
                      Salt = salt }
            }
        
        let verifyAsync (password: string) (currentHash: PasswordHashAndSalt): bool Task =
            task {
                let saltBytes = currentHash.Salt |> toByteArray
                let! hash = hashAsync password saltBytes hashLength
                
                return currentHash.Hash = hash
            }
    
    let createUserAsync (dependencies: UserActionsDependencies)
                        (parameters: UserCreationParameters)
                        : Result<UserId, UserCreationResultError> Task =
        task {
            let normalizedLogin = parameters.Login |> NormalizedLogin.create
            let! existingUser = dependencies.UserPersister.FindUserByNormalizedLoginAsync normalizedLogin
            
            match existingUser with
            | Some _ ->
                return Error LoginIsOccupied
            | None ->
                let! hashAndSalt = PasswordHash.generateAsync parameters.Password
                
                let parametersForPersistence =
                    { CreatedAt = parameters.CreatedAt
                      Login = parameters.Login
                      NormalizedLogin = normalizedLogin
                      PasswordHash = hashAndSalt.Hash
                      PasswordSalt = hashAndSalt.Salt }
                
                let! userId = dependencies.UserPersister.CreateUserAsync parametersForPersistence
                
                return Ok userId
        }
    
    let verifyPasswordAsync (dependencies: UserActionsDependencies)
                            (user: User)
                            (password: string)
                            : bool Task =
        task {
            let findUserPasswordHashAndSalt =
                dependencies
                    .UserPersister
                    .FindUserPasswordHashAndSalt
            let! passwordHashAndSaltOption = findUserPasswordHashAndSalt user.Id
            
            match passwordHashAndSaltOption with
            | None -> return! raise (InvalidOperationException "")
            | Some hashAndSalt -> return! (PasswordHash.verifyAsync password hashAndSalt)
        }
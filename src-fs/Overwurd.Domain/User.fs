namespace Overwurd.Domain

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

type private PasswordHashAndSalt =
    { Hash: PasswordHash
      Salt: PasswordSalt }

type User =
    { Id: UserId
      CreatedAt: CreationDate
      Login: Login
      PasswordHash: PasswordHash
      PasswordSalt: PasswordSalt }

type UserCreationParameters =
    { CreatedAt: CreationDate
      Login: Login
      Password: Password }
    
type UserCreationParametersForPersistence =
    { CreatedAt: CreationDate
      Login: Login
      NormalizedLogin: NormalizedLogin
      PasswordHash: PasswordHash
      PasswordSalt: PasswordSalt }

type FindByNormalizedLoginAsync =
    NormalizedLogin -> User option Task

type CreateUserAsync =
    UserCreationParametersForPersistence -> UserId Task

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
            
            match password with
            | NullOrWhiteSpace -> Error "Password cannot be empty."
            | LacksLength minLength -> Error $"Password should be at least {minLength} characters length."
            | UpperOrLowerCharactersAreMissing -> Error "Password should contain both uppercase and lowercase characters."
            | _ -> Ok
        
        let create (password: string): Password =
            password
            |> validate
            |> function
                | Ok -> Password password
                | Error message -> raise (ValidationException message)
        
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
        
        let verifyAsync (password: Password) (currentHash: PasswordHashAndSalt): bool Task =
            task {
                let saltBytes = currentHash.Salt |> toByteArray
                let! hash = hashAsync (Password.unwrap password) saltBytes hashLength
                
                return currentHash.Hash = hash
            }
    
    let createUserAsync (findByNormalizedLoginAsync: FindByNormalizedLoginAsync)
                        (createUserAsync: CreateUserAsync)
                        (parameters: UserCreationParameters)
                        : Result<UserId, UserCreationResultError> Task =
        task {
            let normalizedLogin = parameters.Login |> NormalizedLogin.create
            let! existingUser = findByNormalizedLoginAsync normalizedLogin
            
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
                
                let! userId = createUserAsync parametersForPersistence
                
                return Ok userId
        }
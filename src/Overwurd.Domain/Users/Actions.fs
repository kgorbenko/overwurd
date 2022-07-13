module internal Overwurd.Domain.Users.Actions

open System
open System.Threading.Tasks

open Overwurd.Domain.Users
open Overwurd.Domain.Users.Entities
open Overwurd.Domain.Common.Persistence

type UserActionsDependencies =
    { UserStorage: UserStorage }

type UserCreationResultError =
    | LoginIsOccupied

let createUserAsync (dependencies: UserActionsDependencies)
                    (parameters: UserCreationParameters)
                    (session: DbSession)
                    : Result<UserId, UserCreationResultError> Task =
    task {
        let normalizedLogin = parameters.Login |> NormalizedLogin.create
        let! existingUser =
            let findUserByNormalizedLoginAsync =
                dependencies
                    .UserStorage
                    .FindUserByNormalizedLoginAsync
            findUserByNormalizedLoginAsync normalizedLogin session

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

            let! userId =
                let createUserAsync =
                    dependencies
                        .UserStorage
                        .CreateUserAsync
                createUserAsync parametersForPersistence session

            return Ok userId
    }

let verifyPasswordAsync (dependencies: UserActionsDependencies)
                        (user: User)
                        (password: string)
                        (session: DbSession)
                        : bool Task =
    task {
        let! passwordHashAndSaltOption =
            let findUserPasswordHashAndSalt =
                dependencies
                    .UserStorage
                    .FindUserPasswordHashAndSalt
            findUserPasswordHashAndSalt user.Id session

        match passwordHashAndSaltOption with
        | None -> return! raise (InvalidOperationException "")
        | Some hashAndSalt -> return! (PasswordHash.verifyAsync password hashAndSalt)
    }
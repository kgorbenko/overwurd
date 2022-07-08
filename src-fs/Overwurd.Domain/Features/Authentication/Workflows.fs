module Overwurd.Domain.Features.Authentication.Workflows

open System
open System.Data
open System.Security.Claims
open System.Threading
open System.Threading.Tasks

open Overwurd.Domain
open Overwurd.Domain.Jwt
open Overwurd.Domain.Jwt.Actions
open Overwurd.Domain.Users
open Overwurd.Domain.Common
open Overwurd.Domain.Users.Actions
open Overwurd.Domain.Users.Entities
open Overwurd.Domain.Common.Validation
open Overwurd.Domain.Common.Persistence
open Overwurd.Domain.Features.Authentication

type AuthDependencies =
    { GenerateGuid: GenerateGuid
      JwtConfiguration: JwtConfiguration
      UserStorage: UserStorage
      JwtStorage: JwtStorage }

let private makeUserActionsDependencies
    (authDependencies: AuthDependencies)
    : UserActionsDependencies =
        { UserStorage = authDependencies.UserStorage }

let private makeJwtDependencies
    (authDependencies: AuthDependencies)
    : Dependencies =
        { GenerateGuid = authDependencies.GenerateGuid
          JwtConfiguration = authDependencies.JwtConfiguration
          JwtStorage = authDependencies.JwtStorage }

let private validate (loginRaw: string)
                     (passwordRaw: string)
                     : Result<Login * Password, SignUpError> =
    let loginValidationResult = Login.validate loginRaw
    let passwordValidationResult = Password.validate passwordRaw

    match loginValidationResult, passwordValidationResult with
    | Success, Success ->
        (Login.create loginRaw, Password.create passwordRaw)
        |> Ok
    | result1, result2 ->
        [ result1; result2 ]
        |> List.choose (fun x ->
            match x with
            | Success -> None
            | Fail messages -> Some messages)
        |> List.concat
        |> ValidationError
        |> Error

let private createUserAsync (dependencies: AuthDependencies)
                            (now: UtcDateTime)
                            (session: DbSession)
                            (credentials: Login * Password)
                            : Result<UserId, SignUpError> Task =
    task {
        let login, password = credentials


            
        let! creationResult =
            let userActionsDependencies = makeUserActionsDependencies dependencies
            let creationParameters: UserCreationParameters =
                { CreatedAt = now
                  Login = login
                  Password = password }
            createUserAsync userActionsDependencies creationParameters session

        match creationResult with
        | Ok userId -> return Ok userId
        | Error UserCreationResultError.LoginIsOccupied ->
            return Error SignUpError.LoginIsOccupied
    }

let private getUserById (dependencies: AuthDependencies)
                        (session: DbSession)
                        (userId: UserId)
                        : Result<User, SignUpError> Task =
    task {
        let! userOption =
            let findUserByIdAsync =
                dependencies
                    .UserStorage
                    .FindUserByIdAsync
            findUserByIdAsync userId session

        match userOption with
        | Some user -> return Ok user
        | None ->
            let message = $"Could not find User by Id (#{UserId.unwrap userId}) after creation."
            return
                message
                |> InvalidOperationException
                |> raise
    }

let private generateTokensAsync (dependencies: AuthDependencies)
                                (now: UtcDateTime)
                                (session: DbSession)
                                (user: User)
                                : Result<SuccessfulAuthenticationData, 'error> Task =
    task {
        let claims =
            [ Claim(dependencies.JwtConfiguration.ClaimsOptions.UserIdClaimType, (UserId.unwrap user.Id).ToString())
              Claim(dependencies.JwtConfiguration.ClaimsOptions.UserNameClaimType, Login.unwrap user.Login) ]
        
        let! tokensPair =
            let jwtDependencies = makeJwtDependencies dependencies
            generateTokensPairAsync jwtDependencies user.Id claims now session
        
        return
            { User = user
              Tokens = tokensPair.Tokens
              AccessTokenExpiresAt = tokensPair.AccessTokenExpiresAt
              RefreshTokenExpiresAt = tokensPair.RefreshTokenExpiresAt }
            |> Ok
    }

let private getUserByTokensAsync (dependencies: AuthDependencies)
                                 (session: DbSession)
                                 (data: JwtGenerationResult)
                                 : Result<SuccessfulAuthenticationData, RefreshError> Task =
    task {
        let! userOption =
            let findUserByIdAsync =
                dependencies
                    .UserStorage
                    .FindUserByIdAsync
            findUserByIdAsync data.UserId session

        match userOption with
        | Some user ->
            return
                { User = user
                  Tokens = data.Tokens
                  AccessTokenExpiresAt = data.AccessTokenExpiresAt
                  RefreshTokenExpiresAt = data.RefreshTokenExpiresAt }
                |> Ok
        | None ->
            let message = $"Could not find User by Id (#{UserId.unwrap data.UserId}) after tokens refresh."
            return
                message
                |> InvalidOperationException
                |> raise
    }

let private findUserByLogin (dependencies: AuthDependencies)
                            (credentials: Credentials)
                            (session: DbSession)
                            : Result<User, SignInError> Task =
    task {
        let normalizedLogin =
            credentials.Login
            |> Login.createBypassingValidation
            |> NormalizedLogin.create

        let! userOption =
            let findUserByNormalizedLoginAsync =
                dependencies
                    .UserStorage
                    .FindUserByNormalizedLoginAsync
            findUserByNormalizedLoginAsync normalizedLogin session

        match userOption with
        | None ->
            return Error UserDoesNotExist
        | Some user ->
            return Ok user
    }

let private verifyPasswordAsync (dependencies: AuthDependencies)
                                (credentials: Credentials)
                                (session: DbSession)
                                (user: User)
                                : Result<User, SignInError> Task =
    task {
        let! isPasswordValid =
            let verifyDependencies = makeUserActionsDependencies dependencies
            verifyPasswordAsync verifyDependencies user credentials.Password session

        return if isPasswordValid
            then Ok user
            else Error InvalidPassword
    }

let private signUpAsyncInternal (dependencies: AuthDependencies)
                                (now: UtcDateTime)
                                (loginRaw: string)
                                (passwordRaw: string)
                                (session: DbSession)
                                : Result<SuccessfulAuthenticationData, SignUpError> Task =
    task {
        return!
            validate loginRaw passwordRaw
            |> AsyncResult.asynchronouslyBind (createUserAsync dependencies now session)
            |> AsyncResult.asynchronouslyBindTask (getUserById dependencies session)
            |> AsyncResult.asynchronouslyBindTask (generateTokensAsync dependencies now session)
    }

let private refreshAsyncInternal (dependencies: AuthDependencies)
                                 (tokenValuesPair: JwtTokensPair)
                                 (now: UtcDateTime)
                                 (session: DbSession)
                                 : Result<SuccessfulAuthenticationData, RefreshError> Task =
    task {
        let refreshDependencies =
            { GenerateGuid = dependencies.GenerateGuid
              JwtConfiguration = dependencies.JwtConfiguration
              JwtStorage = dependencies.JwtStorage }

        return!
            refreshAccessTokenAsync refreshDependencies tokenValuesPair now session
            |> AsyncResult.asynchronouslyBindTask (getUserByTokensAsync dependencies session)
    }

let private signInAsyncInternal (dependencies: AuthDependencies)
                                (credentials: Credentials)
                                (now: UtcDateTime)
                                (session: DbSession)
                                : Result<SuccessfulAuthenticationData, SignInError> Task =
    task {
        return!
            findUserByLogin dependencies credentials session
            |> AsyncResult.asynchronouslyBindTask (verifyPasswordAsync dependencies credentials session)
            |> AsyncResult.asynchronouslyBindTask (generateTokensAsync dependencies now session)
    }

let signUpAsync (dependencies: AuthDependencies)
                (now: UtcDateTime)
                (loginRaw: string)
                (passwordRaw: string)
                (connection: IDbConnection)
                (cancellationToken: CancellationToken)
                : Result<SuccessfulAuthenticationData, SignUpError> Task =
    task {
        return! signUpAsyncInternal dependencies now loginRaw passwordRaw |> inTransactionAsync connection cancellationToken
    }

let refreshAsync (dependencies: AuthDependencies)
                 (tokenValuesPair: JwtTokensPair)
                 (now: UtcDateTime)
                 (connection: IDbConnection)
                 (cancellationToken: CancellationToken)
                 : Result<SuccessfulAuthenticationData, RefreshError> Task =
    task {
        return! refreshAsyncInternal dependencies tokenValuesPair now |> inTransactionAsync connection cancellationToken
    }

let signInAsync (dependencies: AuthDependencies)
                (credentials: Credentials)
                (now: UtcDateTime)
                (connection: IDbConnection)
                (cancellationToken: CancellationToken)
                : Result<SuccessfulAuthenticationData, SignInError> Task =
    task {
        return! signInAsyncInternal dependencies credentials now |> inTransactionAsync connection cancellationToken
    }
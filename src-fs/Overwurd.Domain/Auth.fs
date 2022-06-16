namespace Overwurd.Domain

open System
open System.Security.Claims
open System.Threading.Tasks

open Overwurd.Domain
open Overwurd.Domain.Common.Validation

type AuthDependencies =
    { GenerateGuid: GenerateGuid
      JwtConfiguration: JwtConfiguration
      UserPersister: UserPersister
      RefreshTokensPersister: JwtRefreshTokensPersister }

type Credentials =
    { Login: string
      Password: string }

type TokensData =
    { User: User
      Tokens: JwtTokensPair
      AccessTokenExpiresAt: UtcDateTime
      RefreshTokenExpiresAt: UtcDateTime }

type SignUpError =
    | ValidationError of ValidationErrorMessage list
    | LoginIsOccupied

type SignInError =
    | UserDoesNotExist
    | InvalidPassword

module Auth =

    open Overwurd.Domain.Jwt
    open Overwurd.Domain.User
    open Overwurd.Domain.Common.Utils

    let private makeUserActionsDependencies
        (authDependencies: AuthDependencies)
        : UserActionsDependencies =
            { UserPersister = authDependencies.UserPersister }

    let private makeJwtDependencies
        (authDependencies: AuthDependencies)
        : JwtDependencies =
            { GenerateGuid = authDependencies.GenerateGuid
              JwtConfiguration = authDependencies.JwtConfiguration
              RefreshTokensPersister = authDependencies.RefreshTokensPersister}

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
                createUserAsync userActionsDependencies creationParameters

            match creationResult with
            | Ok userId -> return Ok userId
            | Error UserCreationResultError.LoginIsOccupied ->
                return Error LoginIsOccupied
        }

    let private getUserById (dependencies: AuthDependencies)
                            (userId: UserId)
                            : Result<User, SignUpError> Task =
        task {
            let! userOption =
                let findUserByIdAsync =
                    dependencies
                        .UserPersister
                        .FindUserByIdAsync
                findUserByIdAsync userId

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
                                    (user: User)
                                    : Result<TokensData, 'error> Task =
        task {
            let claims =
                [ Claim(dependencies.JwtConfiguration.ClaimsOptions.UserIdClaimType, (UserId.unwrap user.Id).ToString())
                  Claim(dependencies.JwtConfiguration.ClaimsOptions.UserNameClaimType, Login.unwrap user.Login) ]
            
            let! tokensPair =
                let jwtDependencies = makeJwtDependencies dependencies
                generateTokensPairAsync jwtDependencies user.Id claims now
            
            return
                { User = user
                  Tokens = tokensPair.Tokens
                  AccessTokenExpiresAt = tokensPair.AccessTokenExpiresAt
                  RefreshTokenExpiresAt = tokensPair.RefreshTokenExpiresAt }
                |> Ok
        }

    let private getUserByTokensAsync (dependencies: AuthDependencies)
                                     (data: RefreshedTokens)
                                     : Result<TokensData, RefreshAccessTokenError> Task =
        task {
            let! userOption =
                let findUserByIdAsync =
                    dependencies
                        .UserPersister
                        .FindUserByIdAsync
                findUserByIdAsync data.UserId

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
                                : Result<User, SignInError> Task =
        task {
            let normalizedLogin =
                credentials.Login
                |> Login.createBypassingValidation
                |> NormalizedLogin.create

            let! userOption =
                let findUserByNormalizedLoginAsync =
                    dependencies
                        .UserPersister
                        .FindUserByNormalizedLoginAsync
                findUserByNormalizedLoginAsync normalizedLogin

            match userOption with
            | None ->
                return Error UserDoesNotExist
            | Some user ->
                return Ok user
        }

    let private verifyPasswordAsync (dependencies: AuthDependencies)
                                    (credentials: Credentials)
                                    (user: User)
                                    : Result<User, SignInError> Task =
        task {
            let! isPasswordValid =
                let verifyDependencies = makeUserActionsDependencies dependencies
                verifyPasswordAsync verifyDependencies user credentials.Password

            return if isPasswordValid
                then Ok user
                else Error InvalidPassword
        }

    let signUpAsync (dependencies: AuthDependencies)
                    (now: UtcDateTime)
                    (loginRaw: string)
                    (passwordRaw: string)
                    : Result<TokensData, SignUpError> Task =
        task {
            return!
                validate loginRaw passwordRaw
                |> AsyncResult.asynchronouslyBind (createUserAsync dependencies now)
                |> AsyncResult.asynchronouslyBindTask (getUserById dependencies)
                |> AsyncResult.asynchronouslyBindTask (generateTokensAsync dependencies now)
        }

    let refreshAsync (dependencies: AuthDependencies)
                     (tokenValuesPair: JwtTokensPair)
                     (now: UtcDateTime)
                     : Result<TokensData, RefreshAccessTokenError> Task =
        task {
            let refreshDependencies =
                { GenerateGuid = dependencies.GenerateGuid
                  JwtConfiguration = dependencies.JwtConfiguration
                  RefreshTokensPersister = dependencies.RefreshTokensPersister }

            return!
                refreshAccessTokenAsync refreshDependencies tokenValuesPair now
                |> AsyncResult.asynchronouslyBindTask (getUserByTokensAsync dependencies)
        }
    
    let signInAsync (dependencies: AuthDependencies)
                    (credentials: Credentials)
                    (now: UtcDateTime)
                    : Result<TokensData, SignInError> Task =
        task {
            return!
                findUserByLogin dependencies credentials
                |> AsyncResult.asynchronouslyBindTask (verifyPasswordAsync dependencies credentials)
                |> AsyncResult.asynchronouslyBindTask (generateTokensAsync dependencies now)
        }
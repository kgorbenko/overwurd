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

    let private validate (loginRaw: string)
                         (passwordRaw: string)
                         : Result<Login * Password, SignUpError> =
        let loginValidationResult = Login.validate loginRaw
        let passwordValidationResult = Password.validate passwordRaw
        
        match loginValidationResult, passwordValidationResult with
        | Ok, Ok ->
            Result.Ok (Login.create loginRaw, Password.create passwordRaw)
        | result1, result2 ->
            [ result1; result2 ]
            |> List.choose (fun x ->
                match x with
                | Ok -> None
                | Error messages -> Some messages)
            |> List.concat
            |> ValidationError
            |> Result.Error
    
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
            | Result.Ok userId ->
                return Result.Ok userId
            | Result.Error UserCreationResultError.LoginIsOccupied ->
                return Result.Error LoginIsOccupied
        }
    
    let private getUserById (dependencies: AuthDependencies)
                            (userId: UserId)
                            : Result<User, SignUpError> Task =
        task {
            let! user = dependencies.UserPersister.FindUserByIdAsync userId
            
            match user with
            | Some user ->
                return Result.Ok user
            | None ->
                return raise (InvalidOperationException $"Could not find user by Id (#{UserId.unwrap userId}) after creation")
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
                let dependencies: JwtDependencies =
                    { GenerateGuid = dependencies.GenerateGuid
                      JwtConfiguration = dependencies.JwtConfiguration
                      RefreshTokensPersister = dependencies.RefreshTokensPersister}
                generateTokensPairAsync dependencies user.Id claims now
            
            return Result.Ok
                { User = user
                  Tokens = tokensPair.Tokens
                  AccessTokenExpiresAt = tokensPair.AccessTokenExpiresAt
                  RefreshTokenExpiresAt = tokensPair.RefreshTokenExpiresAt }
        }

    let private getUserByTokensAsync (dependencies: AuthDependencies)
                                     (data: RefreshedTokens)
                                     : Result<TokensData, RefreshAccessTokenError> Task =
        task {
            let! userOption = dependencies.UserPersister.FindUserByIdAsync data.UserId

            match userOption with
            | Some user ->
                return
                    Result.Ok
                        { User = user
                          Tokens = data.Tokens
                          AccessTokenExpiresAt = data.AccessTokenExpiresAt
                          RefreshTokenExpiresAt = data.RefreshTokenExpiresAt }
            | None ->
                return raise (InvalidOperationException $"Could not find user by Id (#{UserId.unwrap data.UserId}) after tokens refresh")
        }
    
    let private findUserByLogin (dependencies: AuthDependencies)
                                (credentials: Credentials)
                                : Result<User, SignInError> Task =
        task {
            let normalizedLogin =
                credentials.Login
                |> Login.createBypassingValidation
                |> NormalizedLogin.create

            let! userOption = dependencies.UserPersister.FindUserByNormalizedLoginAsync normalizedLogin
            
            match userOption with
            | None ->
                return Result.Error UserDoesNotExist
            | Some user ->
                return Result.Ok user
        }
    
    let private verifyPasswordAsync (dependencies: AuthDependencies)
                                    (credentials: Credentials)
                                    (user: User)
                                    : Result<User, SignInError> Task =
        task {
            let verifyDependencies = makeUserActionsDependencies dependencies
            let! isPasswordValid = verifyPasswordAsync verifyDependencies user credentials.Password
            
            return if isPasswordValid
                then Result.Ok user
                else Result.Error InvalidPassword
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
namespace Overwurd.Domain

open System
open System.Security.Claims
open System.Threading.Tasks
open Overwurd.Domain
open Overwurd.Domain.Common.Validation
open Overwurd.Domain.Common.Utils

type SignUpResult =
    | Success
    | ValidationError of ValidationErrorMessage list
    | LoginIsOccupied

module Auth =

    open Overwurd.Domain.User
    
    let private validate (loginRaw: string)
                         (passwordRaw: string)
                         : Result<Login * Password, SignUpResult> =
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
    
    let private createUserAsync (findByNormalizedLoginAsync: FindByNormalizedLoginAsync)
                                (createUserAsync: CreateUserAsync)
                                (now: UtcDateTime)
                                (credentials: Login * Password)
                                : Result<UserId, SignUpResult> Task =
        task {
            let login, password = credentials
            let creationParameters: UserCreationParameters =
                { CreatedAt = CreationDate.create now
                  Login = login
                  Password = password }
            
            let! creationResult = User.createUserAsync findByNormalizedLoginAsync createUserAsync creationParameters
            
            match creationResult with
            | Result.Ok userId ->
                return Result.Ok userId
            | Result.Error UserCreationResultError.LoginIsOccupied ->
                return Result.Error LoginIsOccupied
        }
    
    let private getUserById (getUserByIdAsync: FindUserByIdAsync)
                            (userId: UserId)
                            : Result<User, SignUpResult> Task =
        task {
            let! user = getUserByIdAsync userId
            
            match user with
            | Some user ->
                return Result.Ok user
            | None ->
                return raise (InvalidOperationException $"Could not find user by Id (#{UserId.unwrap userId}) after creation")
        }
    
    let private generateTokensAsync (generateGuid: GenerateGuid)
                                    (getUserRefreshTokensAsync: GetUserRefreshTokensAsync)
                                    (removeRefreshTokensAsync: RemoveRefreshTokensAsync)
                                    (createRefreshTokenAsync: CreateRefreshTokenAsync)
                                    (claimsIdentityOptions: ClaimsIdentityOptions)
                                    (configuration: JwtConfiguration)
                                    (now: UtcDateTime)
                                    (user: User)
                                    : Result<JwtTokensPair, SignUpResult> Task =
        task {
            let claims =
                [ Claim(claimsIdentityOptions.UserIdClaimType, (UserId.unwrap user.Id).ToString())
                  Claim(claimsIdentityOptions.UserNameClaimType, Login.unwrap user.Login) ]
            
            let! tokensPair =
                Jwt.generateTokensAsync generateGuid
                                        getUserRefreshTokensAsync
                                        removeRefreshTokensAsync
                                        createRefreshTokenAsync
                                        configuration
                                        user.Id
                                        claims
                                        now
            
            return Result.Ok tokensPair
        }
    
    let signUpAsync (findByNormalizedLoginAsync: FindByNormalizedLoginAsync)
                    (createUserInternalAsync: CreateUserAsync)
                    (getUserByIdAsync: FindUserByIdAsync)
                    (generateGuid: GenerateGuid)
                    (getUserRefreshTokensAsync: GetUserRefreshTokensAsync)
                    (removeRefreshTokensAsync: RemoveRefreshTokensAsync)
                    (createRefreshTokenAsync: CreateRefreshTokenAsync)
                    (configuration: JwtConfiguration)
                    (claimsIdentityOptions: ClaimsIdentityOptions)
                    (now: UtcDateTime)
                    (loginRaw: string)
                    (passwordRaw: string)
                    : Result<JwtTokensPair, SignUpResult> Task =
        task {
            return!
                validate loginRaw passwordRaw
                |> AsyncResult.asynchronouslyBind (createUserAsync findByNormalizedLoginAsync createUserInternalAsync now)
                |> AsyncResult.asynchronouslyBindTask (getUserById getUserByIdAsync)
                |> AsyncResult.asynchronouslyBindTask (generateTokensAsync generateGuid getUserRefreshTokensAsync removeRefreshTokensAsync createRefreshTokenAsync claimsIdentityOptions configuration now)
        }
    
    let signInAsync () =v 
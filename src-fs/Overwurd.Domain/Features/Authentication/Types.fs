namespace Overwurd.Domain.Features.Authentication

open Overwurd.Domain
open Overwurd.Domain.Jwt
open Overwurd.Domain.Users
open Overwurd.Domain.Common.Validation

type RawCredentials =
    { Login: string
      Password: string }

type Credentials =
    { Login: Login
      Password: PasswordValue }

type SuccessfulAuthenticationData =
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
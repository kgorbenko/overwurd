namespace Overwurd.Domain.Users

open System.Threading.Tasks

open Overwurd.Domain
open Overwurd.Domain.Common.Persistence

type UserCreationParameters =
    { CreatedAt: UtcDateTime
      Login: Login
      Password: PasswordValue }

type UserCreationParametersForPersistence =
    { CreatedAt: UtcDateTime
      Login: Login
      NormalizedLogin: NormalizedLogin
      PasswordHash: PasswordHash
      PasswordSalt: PasswordSalt }

type FindUserByIdAsync =
    UserId -> DbSession -> User option Task

type FindUserByNormalizedLoginAsync =
    NormalizedLogin -> DbSession -> User option Task

type CreateUserAsync =
    UserCreationParametersForPersistence -> DbSession -> UserId Task

type FindUserPasswordHashAndSalt =
    UserId -> DbSession -> Password option Task

type UserStorage =
    { FindUserByIdAsync: FindUserByIdAsync
      FindUserByNormalizedLoginAsync: FindUserByNormalizedLoginAsync
      FindUserPasswordHashAndSalt: FindUserPasswordHashAndSalt
      CreateUserAsync: CreateUserAsync }
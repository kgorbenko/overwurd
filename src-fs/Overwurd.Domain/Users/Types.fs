namespace Overwurd.Domain.Users

open Overwurd.Domain

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
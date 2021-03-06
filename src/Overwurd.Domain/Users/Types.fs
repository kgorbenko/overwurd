namespace Overwurd.Domain.Users

open Overwurd.Domain.Common

type UserId =
    UserId of int

type Login =
    private Login of string

type NormalizedLogin =
    private NormalizedLogin of string

type PasswordValue =
    private PasswordValue of string

type PasswordHash = string

type PasswordSalt = string

type Password =
    { Hash: PasswordHash
      Salt: PasswordSalt }

type User =
    { Id: UserId
      CreatedAt: UtcDateTime
      Login: Login
      Password: Password }
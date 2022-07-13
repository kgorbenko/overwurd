module Overwurd.DataAccess.UserStorage

open System

open Overwurd.Domain.Common
open Overwurd.Domain.Users
open Overwurd.Domain.Users.Entities
open Overwurd.DataAccess.Database.Dapper
open Overwurd.Domain.Common.Persistence

type internal UserPersistentModel =
    { Id: int
      CreatedAt: DateTime
      Login: string
      NormalizedLogin: string
      PasswordHash: string
      PasswordSalt: string }

type internal PasswordPersistentModel =
    { Hash: string
      Salt: string }

let private toDomainUser (model: UserPersistentModel): User =
    { Id = UserId model.Id
      CreatedAt = UtcDateTime.create model.CreatedAt
      Login = Login.create model.Login
      Password =
          { Hash = model.PasswordHash
            Salt = model.PasswordSalt } }

let private toDomainHashAndSalt (model: PasswordPersistentModel): Password =
    { Hash = model.Hash
      Salt = model.Salt }

let findUserByIdAsync: FindUserByIdAsync =
    fun (userId: UserId)
        (session: DbSession) ->
        task {
            let sql = """
select
    "Id",
    "CreatedAt",
    "Login",
    "NormalizedLogin",
    "PasswordHash",
    "PasswordSalt"
  from "overwurd"."Users"
 where "Id" = @Id
"""

            let parameters = {| Id = UserId.unwrap userId |}
            let command = makeSqlCommandWithParameters sql parameters session
            let! result = session.Connection |> findAsync<UserPersistentModel> command

            return result
                |> Option.map toDomainUser
        }

let findUserByNormalizedLoginAsync: FindUserByNormalizedLoginAsync =
    fun (normalizedLogin: NormalizedLogin)
        (session: DbSession) ->
        task {
            let sql = """
select
    "Id",
    "CreatedAt",
    "Login",
    "NormalizedLogin",
    "PasswordHash",
    "PasswordSalt"
  from "overwurd"."Users"
 where "NormalizedLogin" = @NormalizedLogin
"""

            let parameters = {| NormalizedLogin = NormalizedLogin.unwrap normalizedLogin |}
            let command = makeSqlCommandWithParameters sql parameters session
            let! result = session.Connection |> findAsync<UserPersistentModel> command

            return result
                |> Option.map toDomainUser
        }

let createUserAsync: CreateUserAsync =
    fun (parameters: UserCreationParametersForPersistence)
        (session: DbSession) ->
        task {
            let sql = """
insert into "overwurd"."Users" (
    "CreatedAt",
    "Login",
    "NormalizedLogin",
    "PasswordHash",
    "PasswordSalt"
) values (
    @CreatedAt,
    @Login,
    @NormalizedLogin,
    @PasswordHash,
    @PasswordSalt
) returning "Id"
"""
            let parameters =
                {| CreatedAt = UtcDateTime.unwrap parameters.CreatedAt
                   Login = Login.unwrap parameters.Login
                   NormalizedLogin = NormalizedLogin.unwrap parameters.NormalizedLogin
                   PasswordHash = parameters.PasswordHash
                   PasswordSalt = parameters.PasswordSalt |}

            let command = makeSqlCommandWithParameters sql parameters session
            let! userId = session.Connection |> querySingleAsync<int> command
            return UserId userId
        }

let findUserPasswordHashAndSalt: FindUserPasswordHashAndSalt =
    fun (userId: UserId)
        (session: DbSession) ->
        task {
            let sql = """
select
    "PasswordHash" as Hash,
    "PasswordSalt" as Salt
  from "overwurd"."Users"
 where "Id" = @Id
"""
            
            let parameters = {| Id = UserId.unwrap userId |}
            let command = makeSqlCommandWithParameters sql parameters session
            let! result = session.Connection |> findAsync<PasswordPersistentModel> command
            
            return result
                |> Option.map toDomainHashAndSalt
        }

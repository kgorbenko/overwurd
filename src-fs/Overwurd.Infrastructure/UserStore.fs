namespace Overwurd.Infrastructure

open System
open System.Threading
open System.Threading.Tasks
open Overwurd.Domain
open Overwurd.Domain.User
open Overwurd.Infrastructure.Database
open Overwurd.Infrastructure.Database.Dapper

type internal UserPersistentModel =
    { Id: int
      CreatedAt: DateTime
      Login: string
      NormalizedLogin: string
      PasswordHash: string
      PasswordSalt: string }

module UserStore =

    let private toDomain (model: UserPersistentModel): User =
        { Id = UserId model.Id
          CreatedAt = CreationDate.create model.CreatedAt
          Login = Login.create model.Login
          PasswordHash = model.PasswordHash
          PasswordSalt = model.PasswordSalt }

    let getUserByIdAsync (userId: UserId)
                         (cancellationToken: CancellationToken)
                         (session: Session)
                         : User option Task =
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
            let command = makeSqlCommandWithParameters sql parameters session.Transaction cancellationToken
            let! result = session.Connection |> findAsync<UserPersistentModel> command

            return result
                |> Option.map toDomain
        }

    let getUserByNormalizedLoginAsync (normalizedLogin: string)
                                      (cancellationToken: CancellationToken)
                                      (session: Session)
                                      : User option Task =
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

            let parameters = {| NormalizedLogin = normalizedLogin |}
            let command = makeSqlCommandWithParameters sql parameters session.Transaction cancellationToken
            let! result = session.Connection |> findAsync<UserPersistentModel> command

            return result
                |> Option.map toDomain
        }

    let createUserAsync (parameters: UserCreationParametersForPersistence)
                        (cancellationToken: CancellationToken)
                        (session: Session)
                        : int Task =
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
                {| CreatedAt = CreationDate.unwrap parameters.CreatedAt
                   Login = Login.unwrap parameters.Login
                   NormalizedLogin = NormalizedLogin.unwrap parameters.NormalizedLogin
                   PasswordHash = parameters.PasswordHash
                   PasswordSalt = parameters.PasswordSalt |}

            let command = makeSqlCommandWithParameters sql parameters session.Transaction cancellationToken
            return! session.Connection |> querySingleAsync<int> command
        }

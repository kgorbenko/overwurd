namespace Overwurd.Infrastructure

open System
open System.Threading
open System.Threading.Tasks
open Overwurd.Domain
open Overwurd.Infrastructure.Database
open Overwurd.Infrastructure.Database.Dapper

type internal UserPersistentModel =
    { Id: int
      CreatedAt: DateTime
      Login: string
      NormalizedLogin: string
      Password: string }

module UserStore =

    let private toDomain (model: UserPersistentModel): User =
        { Id = UserId model.Id
          CreatedAt = CreationDate.create model.CreatedAt
          Login = User.Login.create model.Login
          PasswordHash = User.PasswordHash.create model.Password }

    let getUserByIdAsync (userId: int)
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
    "Password"
from "overwurd"."Users"
where "Id" = @Id
"""

            let parameters = {| Id = userId |}
            let command = makeSqlCommandWithParameters sql parameters session.Transaction cancellationToken
            let! result = session.Connection |> findAsync<UserPersistentModel> command

            return result
                |> Option.map toDomain
        }

    let getUserByLoginAsync (login: string)
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
    "Password"
from "overwurd"."Users"
where "Login" = @Login
"""

            let parameters = {| Login = login |}
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
    "Password"
) values (
    @CreatedAt,
    @Login,
    @NormalizedLogin,
    @Password
) returning "Id"
"""
            let parameters =
                {| CreatedAt = parameters.CreatedAt
                   Login = parameters.Login
                   NormalizedLogin = parameters.NormalizedLogin
                   Password = parameters.PasswordHash |}


            let command = makeSqlCommandWithParameters sql parameters session.Transaction cancellationToken
            return! session.Connection |> querySingleAsync<int> command
        }

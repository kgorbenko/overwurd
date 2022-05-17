module Overwurd.Infrastructure.Tests.Common.Database

open Dapper
open System.Threading.Tasks

open Overwurd.Domain
open Overwurd.Infrastructure.Database

let clearAsync (session: Session): unit Task =
    task {
        let sql = """
truncate "overwurd"."Users" restart identity cascade;
"""

        let command = CommandDefinition(commandText = sql, transaction = session.Transaction)
        let! _ = session.Connection.ExecuteAsync command

        ()
    }

let createUserAsync (parameters: UserCreationParametersForPersistence)
                    (session: Session)
                    : UserId Task =
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
    @PasswordHash
) returning "Id"
"""

        let command = CommandDefinition(commandText = sql, parameters = parameters, transaction = session.Transaction)
        let! id = session.Connection.QuerySingleAsync<int> command

        return (UserId id)
    }

let createCourseAsync (creationParameters: CourseCreationParametersForPersistence)
                      (userId: int)
                      (session: Session)
                      : CourseId Task =
    task {
        let sql = """
insert into "overwurd"."Courses" (
    "CreatedAt",
    "UserId",
    "Name",
    "Description"
) values (
    @CreatedAt,
    @UserId,
    @Name,
    @Description
) returning "Id"
"""

        let parameters =
            {| CreatedAt = creationParameters.CreatedAt
               UserId = userId
               Name = creationParameters.Name
               Description = creationParameters.Description |}

        let command = CommandDefinition(commandText = sql, parameters = parameters, transaction = session.Transaction)
        let! id = session.Connection.QuerySingleAsync<int> command

        return (CourseId id)
    }

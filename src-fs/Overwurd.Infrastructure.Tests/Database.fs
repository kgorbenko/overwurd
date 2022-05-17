namespace Overwurd.Infrastructure.Tests

open System.Threading
open Dapper
open Overwurd.Domain.Course
open Overwurd.Domain.User

module Database =

    open System.Threading.Tasks
    open Microsoft.Extensions.Configuration
    open NUnit.Framework
    open Overwurd.Infrastructure.Database
    open Overwurd.Infrastructure.Database.Utils

    let private getConnectionString () =
        let configuration =
            ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddJsonFile("appsettings.tests.json")
                .AddEnvironmentVariables()
                .Build()

        configuration.GetConnectionString("Default")

    let private clearAsync (session: Session) =
        task {
            let sql = """
truncate "overwurd"."Users" restart identity cascade;
"""

            let command = CommandDefinition(commandText = sql, transaction = session.Transaction)
            let! _ = session.Connection.ExecuteAsync command

            ()
        }

    let withConnectionAsync (queryAsync: Session -> 'a Task): 'a Task =
        task {
            let connectionString = getConnectionString ()
            return! withConnectionAsync queryAsync connectionString
        }

    let prepareDatabaseAsync (session: Session): unit Task =
        task {
            do! clearAsync session
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

namespace Overwurd.Infrastructure

open System
open System.Linq.Expressions
open Dapper
open Dapper.FluentMap
open Dapper.FluentMap.Conventions
open Dapper.FluentMap.Mapping

module Course =

    open System.Threading
    open System.Threading.Tasks

    open Overwurd.Infrastructure.Common
    open Overwurd.Domain.Course

    type CoursePersistentModel = {
        Id: int
        UserId: int
        CreatedAt: DateTimeOffset
        Name: string
        Description: string option
    }

    let toDomain (model: CoursePersistentModel): Course =
        { Id = CourseId model.Id
          UserId = model.UserId
          CreatedAt = model.CreatedAt
          Name = Name.create model.Name
          Description = Description.create model.Description }

    let getAllCoursesAsync (cancellationToken: CancellationToken)
                           (session: Session)
                           : Course list Task =
        task {
            let sql = """
SELECT "Id",
       "UserId",
       "CreatedAt",
       "Name",
       "Description"
  FROM "overwurd"."Courses"
"""

            let command = CommandDefinition (
                commandText = sql,
                    transaction = session.Transaction,
                        cancellationToken = cancellationToken
            )

            let! result = session.Connection.QueryAsync<CoursePersistentModel>(command)

            return result
                |> Seq.map toDomain
                |> List.ofSeq
        }

    let getCourseByNameAsync (session: Session)
                             (cancellationToken: CancellationToken)
                             (courseName: Name.CourseName)
                             : Course option Task =
        task {
            let sql = """
SELECT Id,
       CreatedAt,
       Name,
       Description
  from overwurd.Courses
 where Name = @Name
"""

            let parameters = {| Name = courseName |}
            let command = CommandDefinition (
                commandText = sql,
                    parameters = parameters,
                        transaction = session.Transaction,
                            cancellationToken = cancellationToken
            )

            let! result = session.Connection.QuerySingleAsync<CoursePersistentModel option>(command)
            return result |> Option.map toDomain
        }
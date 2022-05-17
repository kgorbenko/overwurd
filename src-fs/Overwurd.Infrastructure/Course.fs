namespace Overwurd.Infrastructure

open System
open Dapper
open Overwurd.Domain.Common.Consistency
open Overwurd.Domain.User

module Course =

    open System.Threading
    open System.Threading.Tasks

    open Overwurd.Infrastructure.Database
    open Overwurd.Domain.Course

    type CoursePersistentModel = {
        Id: int
        UserId: int
        CreatedAt: DateTime
        Name: string
        Description: string option
    }

    let toDomain (model: CoursePersistentModel): Course =
        { Id = CourseId model.Id
          UserId = UserId model.UserId
          CreatedAt = CreationDate.create model.CreatedAt
          Name = CourseName.create model.Name
          Description = CourseDescription.create model.Description }

    let getUserCoursesAsync (userId: int)
                            (cancellationToken: CancellationToken)
                            (session: Session)
                            : Course list Task =
        task {
            let sql = """
select "Id",
       "UserId",
       "CreatedAt",
       "Name",
       "Description"
  from "overwurd"."Courses"
 where "UserId" = @UserId
 order by "Id" desc
"""

            let parameters = {| UserId = userId |}
            let command = Utils.makeSqlCommandWithParameters sql parameters session.Transaction cancellationToken
            let! result = session.Connection.QueryAsync<CoursePersistentModel>(command)

            return result
                |> Seq.map toDomain
                |> List.ofSeq
        }
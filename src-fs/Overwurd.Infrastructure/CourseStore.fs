namespace Overwurd.Infrastructure

open System
open System.Threading
open System.Threading.Tasks

open Overwurd.Domain
open Overwurd.Domain.Course
open Overwurd.Infrastructure.Database
open Overwurd.Infrastructure.Database.Dapper

type internal CoursePersistentModel = {
    Id: int
    UserId: int
    CreatedAt: DateTime
    Name: string
    Description: string option
}

module CourseStore =

    let private toDomain (model: CoursePersistentModel): Course =
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
            let command = makeSqlCommandWithParameters sql parameters session.Transaction cancellationToken
            let! result = session.Connection |> queryAsync<CoursePersistentModel> command

            return result
                |> Seq.map toDomain
                |> List.ofSeq
        }
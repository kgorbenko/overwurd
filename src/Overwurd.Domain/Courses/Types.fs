namespace Overwurd.Domain.Courses

open Overwurd.Domain.Common
open Overwurd.Domain.Users

type CourseId =
    CourseId of int

type CourseName =
    internal CourseName of string

type CourseDescription =
    internal CourseDescription of string
    
type Course =
    { Id: CourseId
      UserId: UserId
      CreatedAt: UtcDateTime
      Name: CourseName
      Description: CourseDescription option }
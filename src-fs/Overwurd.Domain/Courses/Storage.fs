namespace Overwurd.Domain.Courses

open Overwurd.Domain

type CourseCreationParametersForPersistence =
    { CreatedAt: UtcDateTime
      Name: CourseName
      Description: CourseDescription option }
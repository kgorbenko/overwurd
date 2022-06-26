namespace Overwurd.Domain.Courses.Entities

open Overwurd.Domain.Courses

module CourseId =

    let unwrap (courseId: CourseId): int =
        match courseId with
        | CourseId value -> value

module CourseName =

    open Overwurd.Domain.Common.Validation

    let validate (name: string): ValidationResult =
        let maxLength = 60
        
        let rules =
            [ isNullOrWhiteSpace, "Name cannot be empty."
              exceedsMaxLength maxLength, $"Name cannot be longer than {maxLength} characters." ]
        
        validate rules name

    let create (name: string): CourseName =
        validate name
        |> function
            | Success -> CourseName name
            | Fail message -> raise (ValidationException message)

    let unwrap (name: CourseName): string =
        match name with
        | CourseName value -> value

module CourseDescription =

    open Overwurd.Domain.Common.Validation

    let validate (description: string option): ValidationResult =
        let maxLength = 255

        let rules =
            [ isNullOrWhiteSpace, "Description cannot be empty."
              exceedsMaxLength maxLength, $"Description cannot be longer than {maxLength} characters." ]

        match description with
        | None -> Success
        | Some d -> validate rules d

    let create (description: string option): CourseDescription option =
        description
        |> validate
        |> function
            | Success ->
                match description with
                | Some d -> CourseDescription d |> Some
                | None -> None
            | Fail message -> raise (ValidationException message)

    let unwrap (description: CourseDescription): string =
        match description with
        | CourseDescription value -> value
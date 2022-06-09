namespace Overwurd.Domain

open Overwurd.Domain

type CourseId =
    CourseId of int

type CourseName =
    private CourseName of string

type CourseDescription =
    private CourseDescription of string
    
type Course =
    { Id: CourseId
      UserId: UserId
      CreatedAt: CreationDate
      Name: CourseName
      Description: CourseDescription option }

type CourseCreationParametersForPersistence =
    { CreatedAt: CreationDate
      Name: CourseName
      Description: CourseDescription option }

module Course =

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
                | Ok -> CourseName name
                | Error message -> raise (ValidationException message)

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
            | None -> Ok
            | Some d -> validate rules d

        let create (description: string option): CourseDescription option =
            description
            |> validate
            |> function
                | Ok ->
                    match description with
                    | Some d -> CourseDescription d |> Some
                    | None -> None
                | Error message -> raise (ValidationException message)

        let unwrap (description: CourseDescription): string =
            match description with
            | CourseDescription value -> value
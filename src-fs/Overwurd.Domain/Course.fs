namespace Overwurd.Domain

open System.Threading.Tasks

module Course =

    module Name =

        open Overwurd.Domain.Common.Validation

        type CourseName =
            private CourseName of string

        let validate (name: string): ValidationResult =
            let maxLength = 60

            match name with
            | WhiteSpace -> Error "Name cannot be empty."
            | ExceedsMaxLength maxLength -> Error $"Name cannot be longer than {maxLength} characters."
            | _ -> Ok

        let create (name: string): CourseName =
            validate name
            |> function
                | Ok -> CourseName name
                | _ -> raise ValidationException

    module Description =

        open Overwurd.Domain.Common.Validation

        type CourseDescription =
            private CourseDescription of string

        let validate (description: string option): ValidationResult =
            let maxLength = 255

            match description with
            | None -> Ok
            | Some d ->
                match d with
                | WhiteSpace -> Error "Description cannot be empty."
                | ExceedsMaxLength maxLength -> Error $"Description cannot be longer than {maxLength} characters."
                | _ -> Ok

        let create (description: string option): CourseDescription option =
            description
            |> validate
            |> function
                | Ok ->
                    match description with
                    | Some d -> CourseDescription d |> Some
                    | None -> None
                | _ -> raise ValidationException

    open System
    open Name
    open Description

    type CourseId = CourseId of int

    type Course = {
        Id: CourseId
        UserId: int
        CreatedAt: DateTimeOffset
        Name: CourseName
        Description: CourseDescription option
    }

    type CourseCreationParameters = {
        CreatedAt: DateTimeOffset
        Name: CourseName
        Description: CourseDescription
    }

    type GetAllCoursesAsync = unit -> Course list Task
    type GetCourseByNameAsync = CourseName -> Course option Task
    type StoreCourseAsync = CourseCreationParameters -> unit Task

    type CourseUpdateParameters = {
        CreatedAt: DateTimeOffset
        Name: CourseName
        Description: CourseDescription
    }

    type GetCourseByIdAsync = CourseId -> Course option Task
    type UpdateCourseAsync = CourseId -> CourseUpdateParameters -> unit Task
    type RemoveCourseAsync = CourseId -> unit Task

    type StoreCourseResult =
        | Success
        | NameIsOccupied

    let createAsync (getCourseByNameAsync: GetCourseByNameAsync)
                    (storeCourseAsync: StoreCourseAsync)
                    (parameters: CourseCreationParameters)
                    : StoreCourseResult Task =
        task {
            let! currentCourse = getCourseByNameAsync parameters.Name

            match currentCourse with
            | Some _ ->
                return NameIsOccupied
            | None ->
                do! storeCourseAsync parameters
                return Success
        }

    type UpdateCourseResult =
        | Success
        | CourseNotFound
        | NameIsOccupied

    let updateAsync (getCourseByIdAsync: GetCourseByIdAsync)
                    (getCourseByNameAsync: GetCourseByNameAsync)
                    (updateCourseAsync: UpdateCourseAsync)
                    (id: CourseId)
                    (parameters: CourseUpdateParameters)
                    : UpdateCourseResult Task =
        task {
            let! course = getCourseByIdAsync id

            match course with
            | None ->
                return CourseNotFound
            | Some _ ->
                let! courseByNewName = getCourseByNameAsync parameters.Name

                match courseByNewName with
                | Some _ ->
                    return NameIsOccupied
                | None ->
                    do! updateCourseAsync id parameters
                    return Success
        }

    type RemoveCourseResult =
        | Success
        | CourseNotFound

    let removeAsync (getCourseByIdAsync: GetCourseByIdAsync)
                    (removeCourseAsync: RemoveCourseAsync)
                    (id: CourseId)
                    : RemoveCourseResult Task =
        task {
            let! course = getCourseByIdAsync id

            match course with
            | None ->
                return CourseNotFound
            | Some _ ->
                do! removeCourseAsync id
                return Success
        }

    let getAllCoursesAsync (getAllCoursesAsync: GetAllCoursesAsync)
                           : Course list Task =
        task {
            return! getAllCoursesAsync ()
        }
namespace Overwurd.Domain

module Course =

    type CourseId =
        CourseId of int

    type CourseName =
        private CourseName of string

    type CourseDescription =
        private CourseDescription of string

    module CourseId =

        let unwrap (courseId: CourseId): int =
            match courseId with
            | CourseId value -> value

    module CourseName =

        open Overwurd.Domain.Common.Validation

        let validate (name: string): ValidationResult =
            let maxLength = 60

            match name with
            | NullOrWhiteSpace -> Error "Name cannot be empty."
            | ExceedsMaxLength maxLength -> Error $"Name cannot be longer than {maxLength} characters."
            | _ -> Ok

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

            match description with
            | None -> Ok
            | Some d ->
                match d with
                | NullOrWhiteSpace -> Error "Description cannot be empty."
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
                | Error message -> raise (ValidationException message)

        let unwrap (description: CourseDescription): string =
            match description with
            | CourseDescription value -> value

    open System
    open System.Threading.Tasks
    open Overwurd.Domain.User
    open Overwurd.Domain.Common.Consistency

    type Course = {
        Id: CourseId
        UserId: UserId
        CreatedAt: CreationDate
        Name: CourseName
        Description: CourseDescription option
    }

    type CourseCreationParameters = {
        CreatedAt: CreationDate
        Name: CourseName
        Description: CourseDescription option
    }

    type CourseCreationParametersForPersistence = {
        CreatedAt: DateTime
        Name: string
        Description: string option
    }

    type GetUserCoursesAsync = UserId -> Course list Task
    type GetCourseByNameAsync = CourseName -> Course option Task
    type StoreCourseAsync = CourseCreationParameters -> unit Task

    type CourseUpdateParameters = {
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

    let getUserCoursesAsync (getUserCoursesAsync: GetUserCoursesAsync)
                            (userId: UserId)
                           : Course list Task =
        task {
            return! getUserCoursesAsync userId
        }
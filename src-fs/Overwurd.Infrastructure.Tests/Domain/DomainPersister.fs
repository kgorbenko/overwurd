module Overwurd.Infrastructure.Tests.Domain.DomainPersister

open System.Threading.Tasks

open Overwurd.Domain
open Overwurd.Domain.Course
open Overwurd.Domain.User
open Overwurd.Domain.Jwt
open Overwurd.Infrastructure.Tests.Common
open Overwurd.Infrastructure.Tests.Domain
open Overwurd.Infrastructure.Database

let private ensureTransient =
    function
    | Some id -> failwith $"Entity should be transient, but had an id: {id}"
    | _ -> ()

let private persistCourseAsync (course: CourseSnapshot)
                               (userId: UserId)
                               (session: Session)
                               : unit Task =
    task {
        ensureTransient course.Id

        let courseCreationParameters =
            { CreatedAt = CreationDate.unwrap course.CreatedAt
              Name = CourseName.unwrap course.Name
              Description = course.Description |> Option.map CourseDescription.unwrap }

        let! courseId = Database.createCourseAsync courseCreationParameters (UserId.unwrap userId) session
        course.Id <- Some courseId
    }

let private persistRefreshTokenAsync (token: JwtRefreshTokenSnapshot)
                                     (userId: UserId)
                                     (session: Session)
                                     : unit Task =
    task {
        ensureTransient token.Id

        let tokenCreationParameters: JwtRefreshTokenCreationParametersForPersistence =
            { AccessTokenId = JwtAccessTokenId.unwrap token.AccessTokenId
              Value = token.Value
              UserId = UserId.unwrap userId
              CreatedAt = CreationDate.unwrap token.CreatedAt
              RefreshedAt = token.RefreshedAt |> Option.map RefreshDate.unwrap
              ExpiresAt = ExpiryDate.unwrap token.ExpiresAt
              IsRevoked = token.IsRevoked }

        let! tokenId = Database.createRefreshTokenAsync tokenCreationParameters session
        token.Id <- Some tokenId
    }

let private persistUserAsync (user: UserSnapshot)
                             (session: Session)
                             : unit Task =
    task {
        ensureTransient user.Id

        let userCreationParameters =
            { CreatedAt = CreationDate.unwrap user.CreatedAt
              Login = Login.unwrap user.Login
              NormalizedLogin = (Login.unwrap user.Login).ToLowerInvariant()
              PasswordHash = PasswordHash.unwrap user.PasswordHash }

        let! userId = Database.createUserAsync userCreationParameters session
        user.Id <- Some userId

        for course in user.Courses do
            do! persistCourseAsync course userId session

        for token in user.JwtRefreshTokens do
            do! persistRefreshTokenAsync token userId session
    }

let persistSnapshotAsync (snapshot: DomainSnapshot)
                         (session: Session)
                         : unit Task =
    task {
        for user in snapshot.Users do
            do! persistUserAsync user session
    }

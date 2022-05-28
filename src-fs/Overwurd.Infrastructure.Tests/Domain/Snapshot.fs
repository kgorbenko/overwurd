namespace Overwurd.Infrastructure.Tests.Domain

open System

open Overwurd.Domain
open Overwurd.Domain.User
open Overwurd.Domain.Course

type DomainSnapshot =
    { mutable Users: UserSnapshot list }
and UserSnapshot =
    { mutable Id: UserId option
      CreatedAt: CreationDate
      Login: Login
      PasswordHash: PasswordHash
      Courses: CourseSnapshot list
      JwtRefreshTokens: JwtRefreshTokenSnapshot list }
and CourseSnapshot =
    { mutable Id: CourseId option
      CreatedAt: CreationDate
      Name: CourseName
      Description: CourseDescription option }
and JwtRefreshTokenSnapshot =
    { mutable Id: JwtRefreshTokenId option
      AccessTokenId: JwtAccessTokenId
      Value: Guid
      CreatedAt: CreationDate
      RefreshedAt: RefreshDate option
      ExpiresAt: ExpiryDate
      IsRevoked: bool }

module DomainSnapshot =

    let create (): DomainSnapshot =
        { Users = [] }

    let appendUser (user: UserSnapshot) (snapshot: DomainSnapshot) : DomainSnapshot =
        { snapshot with Users = snapshot.Users @ [user] }

    let appendUsers (users: UserSnapshot list) (snapshot: DomainSnapshot): DomainSnapshot =
        { snapshot with Users = snapshot.Users @ users }

module Building =

    let makeUserWithPasswordAndCourses (login: string) (passwordHash: string) (courses: CourseSnapshot list) (createdAt: DateTime): UserSnapshot =
        { Id = None
          CreatedAt = CreationDate.create createdAt
          Login = Login.create login
          PasswordHash = PasswordHash.create passwordHash
          Courses = courses
          JwtRefreshTokens = [] }

    let makeUserWithPassword (login: string) (passwordHash: string) (createdAt: DateTime): UserSnapshot =
        { Id = None
          CreatedAt = CreationDate.create createdAt
          Login = Login.create login
          PasswordHash = PasswordHash.create passwordHash
          Courses = []
          JwtRefreshTokens = [] }

    let makeUserWithCoursesAndPredefinedPassword (login: string) (courses: CourseSnapshot list) (createdAt: DateTime): UserSnapshot =
        { Id = None
          CreatedAt = CreationDate.create createdAt
          Login = Login.create login
          PasswordHash = PasswordHash.create "123456"
          Courses = courses
          JwtRefreshTokens = [] }

    let makeUserWithRefreshTokens (login: string) (tokens: JwtRefreshTokenSnapshot list) (createdAt: DateTime): UserSnapshot =
        { Id = None
          CreatedAt = CreationDate.create createdAt
          Login = Login.create login
          PasswordHash = PasswordHash.create "123456"
          Courses = []
          JwtRefreshTokens = tokens }

    let makeUser (login: string) (createdAt: DateTime): UserSnapshot =
        { Id = None
          CreatedAt = CreationDate.create createdAt
          Login = Login.create login
          PasswordHash = PasswordHash.create "123456"
          Courses = []
          JwtRefreshTokens = [] }

    let makeCourseWithDescription (name: string) (description: string option) (createdAt: DateTime): CourseSnapshot =
        { Id = None
          CreatedAt = CreationDate.create createdAt
          Name = CourseName.create name
          Description = CourseDescription.create description }

    let makeCourse (name: string) (createdAt: DateTime): CourseSnapshot =
        { Id = None
          CreatedAt = CreationDate.create createdAt
          Name = CourseName.create name
          Description = None }

    let makeRefreshToken (createdAt: DateTime): JwtRefreshTokenSnapshot =
        { Id = None
          AccessTokenId = JwtAccessTokenId (Guid.NewGuid())
          Value = Guid.NewGuid()
          CreatedAt = CreationDate.create createdAt
          RefreshedAt = None
          ExpiresAt = ExpiryDate.create (createdAt.AddMinutes 5)
          IsRevoked = false }
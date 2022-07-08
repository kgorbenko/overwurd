namespace Overwurd.Infrastructure.Tests.Domain

open System

open Overwurd.Domain
open Overwurd.Domain.Users
open Overwurd.Domain.Users.Entities
open Overwurd.Domain.Courses
open Overwurd.Domain.Courses.Entities
open Overwurd.Domain.Jwt

type DomainSnapshot =
    { mutable Users: UserSnapshot list }
and UserSnapshot =
    { mutable Id: UserId option
      CreatedAt: UtcDateTime
      Login: Login
      NormalizedLogin: NormalizedLogin
      PasswordHash: PasswordHash
      PasswordSalt: PasswordSalt
      Courses: CourseSnapshot list
      JwtRefreshTokens: JwtRefreshTokenSnapshot list }
and CourseSnapshot =
    { mutable Id: CourseId option
      CreatedAt: UtcDateTime
      Name: CourseName
      Description: CourseDescription option }
and JwtRefreshTokenSnapshot =
    { mutable Id: JwtRefreshTokenId option
      AccessTokenId: JwtAccessTokenId
      Value: Guid
      CreatedAt: UtcDateTime
      RefreshedAt: UtcDateTime option
      ExpiresAt: UtcDateTime
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
        let login = Login.create login
        
        { Id = None
          CreatedAt = UtcDateTime.create createdAt
          Login = login
          NormalizedLogin = NormalizedLogin.create login
          PasswordHash = passwordHash
          PasswordSalt = "W7IHpCImV90XHULk"
          Courses = courses
          JwtRefreshTokens = [] }

    let makeUserWithPassword (login: string) (passwordHash: string) (createdAt: DateTime): UserSnapshot =
        let login = Login.create login
        
        { Id = None
          CreatedAt = UtcDateTime.create createdAt
          Login = login
          NormalizedLogin = NormalizedLogin.create login
          PasswordHash = passwordHash
          PasswordSalt = "W7IHpCImV90XHULk"
          Courses = []
          JwtRefreshTokens = [] }

    let makeUserWithCoursesAndPredefinedPassword (login: string) (courses: CourseSnapshot list) (createdAt: DateTime): UserSnapshot =
        let login = Login.create login
        
        { Id = None
          CreatedAt = UtcDateTime.create createdAt
          Login = login
          NormalizedLogin = NormalizedLogin.create login
          PasswordHash = "117136275353e02cc95d6bb3b38d824a"
          PasswordSalt = "W7IHpCImV90XHULk"
          Courses = courses
          JwtRefreshTokens = [] }

    let makeUserWithRefreshTokens (login: string) (tokens: JwtRefreshTokenSnapshot list) (createdAt: DateTime): UserSnapshot =
        let login = Login.create login
        
        { Id = None
          CreatedAt = UtcDateTime.create createdAt
          Login = login
          NormalizedLogin = NormalizedLogin.create login
          PasswordHash = "117136275353e02cc95d6bb3b38d824a"
          PasswordSalt = "W7IHpCImV90XHULk"
          Courses = []
          JwtRefreshTokens = tokens }

    let makeUser (login: string) (createdAt: DateTime): UserSnapshot =
        let login = Login.create login
        
        { Id = None
          CreatedAt = UtcDateTime.create createdAt
          Login = login
          NormalizedLogin = NormalizedLogin.create login
          PasswordHash = "117136275353e02cc95d6bb3b38d824a"
          PasswordSalt = "W7IHpCImV90XHULk"
          Courses = []
          JwtRefreshTokens = [] }

    let makeCourseWithDescription (name: string) (description: string option) (createdAt: DateTime): CourseSnapshot =
        { Id = None
          CreatedAt = UtcDateTime.create createdAt
          Name = CourseName.create name
          Description = CourseDescription.create description }

    let makeCourse (name: string) (createdAt: DateTime): CourseSnapshot =
        { Id = None
          CreatedAt = UtcDateTime.create createdAt
          Name = CourseName.create name
          Description = None }

    let makeRefreshToken (createdAt: DateTime): JwtRefreshTokenSnapshot =
        { Id = None
          AccessTokenId = JwtAccessTokenId (Guid.NewGuid())
          Value = Guid.NewGuid()
          CreatedAt = UtcDateTime.create createdAt
          RefreshedAt = None
          ExpiresAt = UtcDateTime.create (createdAt.AddMinutes 5)
          IsRevoked = false }
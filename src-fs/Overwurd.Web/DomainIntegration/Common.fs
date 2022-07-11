module Overwurd.Web.DomainIntegration.Common

open System

open Microsoft.AspNetCore.Http

open Overwurd.Domain.Jwt
open Overwurd.Domain.Jwt.Actions
open Overwurd.Domain.Users
open Overwurd.Web.Common
open Overwurd.Web.Common.Configuration
open Overwurd.Infrastructure.UserStorage
open Overwurd.Infrastructure.JwtStorage
open Overwurd.Domain.Features.Authentication.Workflows

let generateGuid: GenerateGuid =
    fun () -> Guid.NewGuid()

let getJwtConfiguration (ctx: HttpContext): JwtConfiguration =
    { TokensConfiguration = ctx.GetJwtTokensConfiguration()
      ClaimsOptions = getClaimsOptions ()
      TokenValidationParameters = ctx.GetJwtTokenValidationParameters() }

let userStorage: UserStorage =
    { FindUserByIdAsync = findUserByIdAsync
      FindUserByNormalizedLoginAsync = findUserByNormalizedLoginAsync
      FindUserPasswordHashAndSalt = findUserPasswordHashAndSalt
      CreateUserAsync = createUserAsync }

let jwtStorage: JwtStorage =
    { GetUserRefreshTokensAsync = getUserRefreshTokensAsync
      GetRefreshTokenByUserAndAccessTokenAsync = getRefreshTokenByUserAndAccessTokenAsync
      CreateRefreshTokenAsync = createRefreshTokenAsync
      UpdateRefreshTokenAsync = updateRefreshTokenAsync
      RemoveRefreshTokensAsync = removeRefreshTokensAsync }

let getAuthDependencies (ctx: HttpContext): AuthDependencies =
    { GenerateGuid = generateGuid
      JwtConfiguration = getJwtConfiguration ctx
      UserStorage = userStorage
      JwtStorage = jwtStorage }
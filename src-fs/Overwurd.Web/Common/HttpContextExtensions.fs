namespace Overwurd.Web.Common

open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Tokens
open System.Runtime.CompilerServices

open Overwurd.Domain.Jwt
open Overwurd.Domain.Users
open Overwurd.Domain.Users.Entities
open Overwurd.Web.Common.Configuration

[<Extension>]
type HttpContextExtensions() =

    [<Extension>]
    static member GetConfiguration(ctx: HttpContext): IConfiguration =
        ctx.GetService<IConfiguration>()

    [<Extension>]
    static member GetJwtTokensConfiguration(ctx: HttpContext): JwtTokensConfiguration =
        ctx.GetConfiguration() |> getTokensConfiguration

    [<Extension>]
    static member GetJwtTokenValidationParameters(ctx: HttpContext): TokenValidationParameters =
        ctx.GetConfiguration() |> getValidationParameters

    [<Extension>]
    static member GetUserId(ctx: HttpContext): UserId =
        let claimsOptions = getClaimsOptions ()
        ctx.User.Claims
        |> List.ofSeq
        |> List.tryFind (fun claim -> claim.Type = claimsOptions.UserIdClaimType)
        |> function
            | Some claim -> UserId.parse claim.Value
            | None -> failwith $"Provided by user token does not contain '{claimsOptions.UserIdClaimType}'"

    [<Extension>]
    static member TryBindJsonAsync<'a>(ctx: HttpContext): 'a option Task =
        task {
            let serializer = ctx.GetJsonSerializer()
            try
                let! deserialized = serializer.DeserializeAsync<'a> ctx.Request.Body
                return Some deserialized
            with
            | :? System.Text.Json.JsonException ->
                return None
        }
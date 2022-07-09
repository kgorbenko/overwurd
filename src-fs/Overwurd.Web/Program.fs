module Overwurd.Web.App

open System
open System.Text.Json
open System.Text.Json.Serialization
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Authentication.JwtBearer

open Overwurd.Infrastructure.Database
open Overwurd.Web.Common.Configuration
open Overwurd.Web.WebApp

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500

let configureApp (app : IApplicationBuilder) =
    do Dapper.registerTypeHandlers ()
    app.UseGiraffeErrorHandler(errorHandler)
        .UseAuthentication()
        .UseHsts()
        .UseStaticFiles()
        .UseGiraffe(webApp)

let getValidationParameters (services: IServiceCollection) =
    let provider = services.BuildServiceProvider()
    let configuration = provider.GetService<IConfiguration>()
    getValidationParameters configuration

let getJsonOptions () =
    let jsonOptions = JsonSerializerOptions()
    jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    jsonOptions.Converters.Add(JsonFSharpConverter())
    jsonOptions

let configureServices (services : IServiceCollection) =
    services.AddSingleton(getJsonOptions ()) |> ignore
    services.AddSingleton<Json.ISerializer, SystemTextJson.Serializer>() |> ignore

    services
        .AddAuthentication(fun options ->
            options.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(fun options ->
            options.SaveToken <- true
            options.TokenValidationParameters <- getValidationParameters services)
        |> ignore

    services.AddGiraffe()
        |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0
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
open Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;

open Overwurd.DataAccess.Database
open Overwurd.Web.Common.Configuration
open Overwurd.Web.WebApp

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500

let configureApp (app : WebApplication) (env: IWebHostEnvironment) =
    app.UseGiraffeErrorHandler(errorHandler)
        .UseAuthentication()
        .UseHsts()
        .UseStaticFiles()
        .UseGiraffe(webApp)

    app.UseSpaStaticFiles()
    app.UseSpa(
        fun spa ->
            spa.Options.SourcePath <- "ClientApp"
        
            if env.IsDevelopment() then
                spa.UseReactDevelopmentServer(npmScript = "start")
        )

let configureBuilder (builder : WebApplicationBuilder) =
    let getValidationParameters (services: IServiceCollection) =
        let provider = services.BuildServiceProvider()
        let configuration = provider.GetService<IConfiguration>()
        getValidationParameters configuration

    let getJsonOptions () =
        let jsonOptions = JsonSerializerOptions()
        jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        jsonOptions.Converters.Add(JsonFSharpConverter())
        jsonOptions

    let port = Environment.GetEnvironmentVariable("PORT")
    builder.WebHost.UseUrls($"http://*:{port}") |> ignore

    builder.Services.AddSingleton(getJsonOptions ()) |> ignore
    builder.Services.AddSingleton<Json.ISerializer, SystemTextJson.Serializer>() |> ignore

    builder.Services.AddSpaStaticFiles(
        fun options ->
            options.RootPath <- "ClientApp/build")

    builder.Services
        .AddAuthentication(
            fun options ->
                options.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
                options.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(
            fun options ->
                options.SaveToken <- true
                options.TokenValidationParameters <- getValidationParameters builder.Services)
        |> ignore

    builder.Services.AddGiraffe()
        |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    do Dapper.registerTypeHandlers ()
    
    let builder = WebApplication.CreateBuilder(args)
    configureBuilder builder
    configureLogging builder.Logging

    let app = builder.Build()
    configureApp app builder.Environment
    app.Run()
    
    0
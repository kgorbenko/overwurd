module Overwurd.Web.App

open System
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection

open Overwurd.Infrastructure.Database
open Overwurd.Web.WebApp

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500

let configureApp (app : IApplicationBuilder) =
    do Dapper.registerTypeHandlers ()
    app.UseGiraffeErrorHandler(errorHandler)
        .UseHsts()
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

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
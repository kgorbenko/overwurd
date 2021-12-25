using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Overwurd.Model;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;

namespace Overwurd.Web
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment webHostEnvironment;

        public Startup([NotNull] IConfiguration configuration,
                       [NotNull] IWebHostEnvironment webHostEnvironment)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSpaStaticFiles(staticFilesOptions => { staticFilesOptions.RootPath = "ClientApp/build"; });

            var connectionString = webHostEnvironment.IsProduction()
                ? GetConnectionStringFromEnvironment()
                : configuration.GetConnectionString("OverwurdDatabase");

            services.AddDbContext<OverwurdDbContext>(options => options.UseNpgsql(connectionString));
            services.AddTransient<IOverwurdRepository<Vocabulary>, OverwurdRepository<Vocabulary, OverwurdDbContext>>();
            services.AddTransient<IReadOnlyOverwurdRepository<Vocabulary>, ReadOnlyOverwurdRepository<Vocabulary, OverwurdDbContext>>();
        }

        private static string GetConnectionStringFromEnvironment()
        {
            const string environmentVariableName = "DATABASE_URL";
            var databaseUriValue = Environment.GetEnvironmentVariable(environmentVariableName)
                                   ?? throw new ArgumentException($"Environment variable '{environmentVariableName}' should be set in 'Production' environment.");
            var databaseUri = new Uri(databaseUriValue);

            var database = databaseUri.LocalPath.TrimStart(trimChar: '/');
            var userInfo = databaseUri.UserInfo.Split(separator: ':', StringSplitOptions.RemoveEmptyEntries);

            return $"Host={databaseUri.Host};" +
                   $"Port={databaseUri.Port};" +
                   $"Database={database};" +
                   $"Username={userInfo[0]};" +
                   $"Password={userInfo[1]};" +
                   "SSL Mode=Require;" +
                   "Trust Server Certificate=True;";
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            } else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
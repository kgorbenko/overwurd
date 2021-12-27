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

        public Startup([NotNull] IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSpaStaticFiles(staticFilesOptions => { staticFilesOptions.RootPath = "ClientApp/build"; });

            var connectionString = configuration.GetConnectionString("Default");

            services.AddDbContext<OverwurdDbContext>(options => options.UseNpgsql(connectionString));
            services.AddTransient<IOverwurdRepository<Vocabulary>, OverwurdRepository<Vocabulary, OverwurdDbContext>>();
            services.AddTransient<IReadOnlyOverwurdRepository<Vocabulary>, ReadOnlyOverwurdRepository<Vocabulary, OverwurdDbContext>>();
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
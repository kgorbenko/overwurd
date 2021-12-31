using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Overwurd.Model;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;
using Overwurd.Web.Options;
using Overwurd.Web.Services.Auth;

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

            var jwtConfigurationJson = configuration.GetSection("Jwt").Get<JwtConfigurationJson>();
            var jwtConfiguration = new JwtConfiguration(
                SecurityAlgorithmSignature: SecurityAlgorithms.HmacSha256Signature,
                SigningKey: jwtConfigurationJson.SigningKey,
                Issuer: jwtConfigurationJson.Issuer,
                Audience: jwtConfigurationJson.Audience,
                AccessTokenExpirationInMinutes: jwtConfigurationJson.AccessTokenExpirationInMinutes,
                RefreshTokenExpirationInDays: jwtConfigurationJson.RefreshTokenExpirationInDays
            );

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtConfiguration.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(AuthHelper.GetBytesFromSigningKey(jwtConfiguration.SigningKey)),
                ValidateAudience = true,
                ValidAudience = jwtConfiguration.Audience,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                ValidAlgorithms = new[] { jwtConfiguration.SecurityAlgorithmSignature }
            };

            services.AddSingleton(jwtConfiguration);
            services.AddSingleton(tokenValidationParameters);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(x =>
                    {
                        x.SaveToken = true;
                        x.TokenValidationParameters = tokenValidationParameters;
                    });

            var connectionString = configuration.GetConnectionString("Default");

            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
            services.AddTransient<IJwtRefreshTokenProvider, JwtRefreshTokenProvider>();
            services.AddTransient<IJwtAuthService, JwtAuthService>();
            services.AddTransient<IOverwurdRepository<Vocabulary>, OverwurdRepository<Vocabulary, ApplicationDbContext>>();
            services.AddTransient<IReadOnlyOverwurdRepository<Vocabulary>, ReadOnlyOverwurdRepository<Vocabulary, ApplicationDbContext>>();
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
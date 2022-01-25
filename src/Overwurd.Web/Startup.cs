using System;
using System.Reflection;
using FluentValidation.AspNetCore;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Overwurd.Model;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;
using Overwurd.Model.Services;
using Overwurd.Web.Options;
using Overwurd.Web.Services;
using Overwurd.Web.Services.Auth;
using Overwurd.Web.Services.Auth.Stores;

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
            services.AddControllersWithViews()
                    .AddFluentValidation(x => x.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()));
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
                ValidAlgorithms = new[] { jwtConfiguration.SecurityAlgorithmSignature }
            };

            services.AddSingleton(jwtConfiguration);
            services.AddSingleton(tokenValidationParameters);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.SaveToken = true;
                x.TokenValidationParameters = tokenValidationParameters;
            });

            var connectionString = configuration.GetConnectionString("Default");

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseNpgsql(
                    connectionString,
                    builder => builder.MigrationsHistoryTable(
                        ApplicationDbContext.MigrationsHistoryTableName,
                        ApplicationDbContext.SchemaName))
            );

            services.AddIdentityCore<User>();
            services.AddSingleton<ClaimsIdentityOptions>();
            services.AddTransient<IUserStore<User>, UserPasswordStore>();
            services.AddTransient<IUserPasswordStore<User>, UserPasswordStore>();
            services.AddTransient<IGuidProvider, GuidProvider>();
            services.AddTransient<IJwtRefreshTokenProvider, JwtRefreshTokenProvider>();
            services.AddTransient<IJwtAuthService, JwtAuthService>();
            services.AddTransient<IRepository<Vocabulary>, Repository<Vocabulary>>();
            services.AddTransient<IReadOnlyRepository<Vocabulary>, ReadOnlyRepository<Vocabulary>>();
            services.AddTransient<IRepository<User>, Repository<User>>();
            services.AddTransient<IReadOnlyRepository<User>, ReadOnlyRepository<User>>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHsts();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

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
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Overwurd.Model;
using Overwurd.Web.Services.Auth;

namespace Overwurd.Web
{
    public class ApplicationDbContext : ModelDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<JwtRefreshToken> JwtRefreshTokens { get; [UsedImplicitly] set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
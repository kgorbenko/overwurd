using Microsoft.EntityFrameworkCore;
using Overwurd.Model;
using Overwurd.Web.Services.Auth;

namespace Overwurd.Web
{
    public class ApplicationDbContext : ModelDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(new DbContextOptionsBuilder(options)) { }

        public DbSet<JwtRefreshToken> JwtRefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Overwurd.Model.Models;

namespace Overwurd.Model
{
    public class ApplicationDbContext : DbContext
    {
        public const string MigrationsHistoryTableName = "__MigrationsHistory";

        public const string SchemaName = "overwurd";

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; [UsedImplicitly] set; }

        public DbSet<JwtRefreshToken> JwtRefreshTokens { get; [UsedImplicitly] set; }

        public DbSet<Vocabulary> Vocabularies { get; [UsedImplicitly] set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
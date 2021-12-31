using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Overwurd.Model.Models;

namespace Overwurd.Model
{
    public class ModelDbContext : DbContext
    {
        public ModelDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; [UsedImplicitly] set; }

        public DbSet<Vocabulary> Vocabularies { get; [UsedImplicitly] set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModelDbContext).Assembly);
        }
    }
}
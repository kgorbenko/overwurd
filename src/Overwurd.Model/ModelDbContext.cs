using Microsoft.EntityFrameworkCore;
using Overwurd.Model.Models;

namespace Overwurd.Model
{
    public class ModelDbContext : DbContext
    {
        public ModelDbContext(DbContextOptionsBuilder builder) : base(builder.Options) { }

        public DbSet<User> Users { get; }

        public DbSet<Vocabulary> Vocabularies { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModelDbContext).Assembly);
        }
    }
}
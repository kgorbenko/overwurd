using Microsoft.EntityFrameworkCore;
using Overwurd.Model.Models;

namespace Overwurd.Model
{
    public class OverwurdDbContext : DbContext
    {
        public OverwurdDbContext(DbContextOptions<OverwurdDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Vocabulary> Vocabularies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OverwurdDbContext).Assembly);
        }
    }
}
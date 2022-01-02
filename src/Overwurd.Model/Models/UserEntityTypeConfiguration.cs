using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Overwurd.Model.Models
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CreatedAt);
            builder.Property(x => x.FirstName);
            builder.Property(x => x.SecondName);

            builder.Property(x => x.Login);
            builder.HasIndex(x => x.Login).IsUnique();
            builder.Property(x => x.NormalizedLogin);
            builder.HasIndex(x => x.NormalizedLogin).IsUnique();
            builder.Property(x => x.Password);

            builder.HasMany(x => x.Roles)
                   .WithMany("Users");
        }
    }
}
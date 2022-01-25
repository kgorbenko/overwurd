using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Overwurd.Model.Models;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.FirstName);
        builder.Property(x => x.LastName);

        builder.Property(x => x.UserName);
        builder.HasIndex(x => x.UserName).IsUnique();
        builder.Property(x => x.NormalizedUserName);
        builder.HasIndex(x => x.NormalizedUserName).IsUnique();
        builder.Property(x => x.Password);

        builder.HasMany(x => x.Roles)
               .WithMany("Users");
    }
}
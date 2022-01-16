using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Overwurd.Model.Models
{
    public class JwtRefreshTokenEntityTypeConfiguration : IEntityTypeConfiguration<JwtRefreshToken>
    {
        public void Configure(EntityTypeBuilder<JwtRefreshToken> builder)
        {
            builder.HasKey(x => x.UserId);
            builder.Property(x => x.AccessTokenId).IsRequired();
            builder.Property(x => x.TokenString).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.ExpiresAt).IsRequired();
            builder.Property(x => x.IsRevoked).IsRequired();

            builder.HasOne<User>()
                   .WithOne()
                   .HasForeignKey<JwtRefreshToken>(x => x.UserId);
        }
    }
}
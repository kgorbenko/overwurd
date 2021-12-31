using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Overwurd.Web.Services.Auth
{
    public class JwtRefreshTokenEntityTypeConfiguration : IEntityTypeConfiguration<JwtRefreshToken>
    {
        public void Configure(EntityTypeBuilder<JwtRefreshToken> builder)
        {
            builder.HasKey(x => x.UserId);
            builder.Property(x => x.AccessTokenId);
            builder.Property(x => x.TokenString);
            builder.Property(x => x.CreatedAt);
            builder.Property(x => x.ExpiresAt);
            builder.Property(x => x.IsRevoked);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Overwurd.Model.Models
{
    public class VocabularyEntityTypeConfiguration : IEntityTypeConfiguration<Vocabulary>
    {
        public void Configure(EntityTypeBuilder<Vocabulary> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name)
                   .HasField("name")
                   .IsRequired();
            builder.HasIndex(x => x.Name)
                   .IsUnique();
            builder.Property(x => x.CreatedAt);
        }
    }
}
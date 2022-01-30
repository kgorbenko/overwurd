using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Overwurd.Model.Models;

public class VocabularyEntityTypeConfiguration : IEntityTypeConfiguration<Vocabulary>
{
    public void Configure(EntityTypeBuilder<Vocabulary> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
               .HasField("name")
               .IsRequired();
        builder.Property(x => x.Description)
               .HasField("description")
               .IsRequired();
        builder.Property(x => x.CreatedAt);

        builder.HasIndex(x => new { x.Name, x.CourseId })
               .IsUnique();

        builder.HasOne(x => x.Course)
               .WithMany(x => x.Vocabularies)
               .HasForeignKey(nameof(Vocabulary.CourseId))
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
    }
}
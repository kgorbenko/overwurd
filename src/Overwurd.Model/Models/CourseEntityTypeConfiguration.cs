using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Overwurd.Model.Models;

public class CourseEntityTypeConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
               .HasField("name")
               .IsRequired();
        builder.Property(x => x.Description)
               .HasField("description")
               .IsRequired();
        builder.Property(x => x.CreatedAt);

        builder.HasIndex(x => new { x.Name, x.UserId })
               .IsUnique();

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(nameof(Course.UserId))
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Vocabularies)
               .WithOne(x => x.Course)
               .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Courses");
    }
}
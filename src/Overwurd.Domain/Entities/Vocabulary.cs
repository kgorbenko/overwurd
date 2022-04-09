using System.Diagnostics.CodeAnalysis;

namespace Overwurd.Domain.Entities;

public class Vocabulary : IEntityWithId, IEntityWithCreationTime, IEquatable<Vocabulary>
{
    public uint Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public uint CourseId { get; }

    public EntityName Name { get; private set; }

    public EntityDescription? Description { get; private set; }

    [MemberNotNull(nameof(Name))]
    public void SetName(EntityName name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void SetDescription(EntityDescription? description)
    {
        Description = description;
    }

    public static Vocabulary CreateForCourse(Course course, uint id, EntityName name, EntityDescription? description, DateTimeOffset createdAt)
    {
        var newVocabulary = new Vocabulary(id, name, description, course.Id, createdAt);
        course.AddVocabulary(newVocabulary);

        return newVocabulary;
    }

    private Vocabulary(uint id, EntityName name, EntityDescription? description, uint courseId, DateTimeOffset createdAt)
    {
        Id = id;
        SetName(name);
        SetDescription(description);
        CourseId = courseId;
        CreatedAt = createdAt;
    }

    public override bool Equals(object? obj) => Equals(obj as Vocabulary);

    public bool Equals(Vocabulary? other) => EntityWithIdComparer.Instance.Equals(this, other);

    public override int GetHashCode() => EntityWithIdComparer.Instance.GetHashCode(this);
}
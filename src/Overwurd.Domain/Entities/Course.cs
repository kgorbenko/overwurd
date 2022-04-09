namespace Overwurd.Domain.Entities;

public class Course : IEntityWithId, IEntityWithCreationTime, IEquatable<Course>
{
    private readonly ICollection<Vocabulary> vocabularies;

    public uint Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public uint UserId { get; }

    public EntityName Name { get; private set; }

    public EntityDescription? Description { get; private set; }

    public IImmutableList<Vocabulary> Vocabularies => vocabularies.ToImmutableList();

    [MemberNotNull(nameof(Name))]
    public void SetName(EntityName name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void SetDescription(EntityDescription? description)
    {
        Description = description;
    }

    public void AddVocabulary(Vocabulary vocabulary)
    {
        if (vocabulary is null)
            throw new ArgumentNullException(nameof(vocabulary));

        if (vocabulary.CourseId != Id)
            throw new InvalidOperationException($"Cannot add {nameof(Vocabulary)} #{vocabulary.Id} to {nameof(Course)} #{Id}. " +
                                                $"{nameof(Vocabulary)} #{vocabulary.Id} is already attached to {nameof(Course)} #{Id}");

        if (vocabularies.Contains(vocabulary))
            throw new InvalidOperationException($"Cannot add {nameof(Vocabulary)} #{vocabulary.Id} to {nameof(Course)} #{Id}. " +
                                                $"{nameof(Vocabulary)} #{vocabulary.Id} is already attached to this {nameof(Course)}");

        vocabularies.Add(vocabulary);
    }

    public void RemoveVocabulary(Vocabulary vocabulary)
    {
        if (vocabulary is null)
            throw new ArgumentNullException(nameof(vocabulary));

        if (vocabulary.CourseId != Id || !vocabularies.Contains(vocabulary))
            throw new InvalidOperationException($"Cannot remove {nameof(Vocabulary)} #{vocabulary.Id} from {nameof(Course)} #{Id}. " +
                                                $"{nameof(Vocabulary)} #{vocabulary.Id} does not belong to {nameof(Course)} #{Id}");

        vocabularies.Remove(vocabulary);
    }

    public static Course CreateForUser(User user, uint id, EntityName name, EntityDescription? description, DateTimeOffset createdAt)
    {
        var newCourse = new Course(id, user.Id, name, description, createdAt);
        user.AddCourse(newCourse);

        return newCourse;
    }

    private Course(uint id, uint userId, EntityName name, EntityDescription? description, DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        SetName(name);
        SetDescription(description);
        vocabularies = new List<Vocabulary>();
        CreatedAt = createdAt;
    }

    public override bool Equals(object? obj) => Equals(obj as Course);

    public bool Equals(Course? other) => EntityWithIdComparer.Instance.Equals(this, other);

    public override int GetHashCode() => EntityWithIdComparer.Instance.GetHashCode(this);
}
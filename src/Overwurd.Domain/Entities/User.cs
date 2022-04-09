using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Overwurd.Domain.Entities;

public class User : IEntityWithId, IEntityWithCreationTime, IEquatable<User>
{
    private readonly ICollection<Course> courses;

    public uint Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public IImmutableList<Course> Courses => courses.ToImmutableList();

    public UserLogin Login { get; }

    public UserPasswordHash PasswordHash { get; private set; }

    [MemberNotNull(nameof(PasswordHash))]
    public void SetPassword(UserPasswordHash passwordHash)
    {
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
    }

    public void AddCourse(Course course)
    {
        if (course is null)
            throw new ArgumentNullException(nameof(course));

        if (course.UserId != Id)
            throw new InvalidOperationException($"Cannot add {nameof(Course)} #{course.Id} to {nameof(User)} #{Id}. " +
                                                $"{nameof(Course)} #{course.Id} is already attached to {nameof(User)} #{course.UserId}");

        if (courses.Contains(course))
            throw new InvalidOperationException($"Cannot add {nameof(Course)} #{course.Id} to {nameof(User)} #{Id}. " +
                                                $"{nameof(Course)} #{course.Id} is already attached to this {nameof(User)}");

        courses.Add(course);
    }

    public void RemoveCourse(Course course)
    {
        if (course is null)
            throw new ArgumentNullException(nameof(course));

        if (course.UserId != Id || !courses.Contains(course))
            throw new InvalidOperationException($"Cannot remove {nameof(Course)} #{course.Id} from {nameof(User)} #{Id}. " +
                                                $"{nameof(Course)} #{course.Id} does not belong to {nameof(User)} #{Id}");

        courses.Remove(course);
    }

    public User(uint id, UserLogin login, UserPasswordHash passwordHash, DateTimeOffset createdAt)
    {
        Id = id;
        Login = login ?? throw new ArgumentNullException(nameof(login));
        SetPassword(passwordHash);
        CreatedAt = createdAt;
        courses = new List<Course>();
    }

    public override bool Equals(object? obj) => Equals((User?) obj);

    public bool Equals(User? other) => EntityWithIdComparer.Instance.Equals(this, other);

    public override int GetHashCode() => EntityWithIdComparer.Instance.GetHashCode(this);
}
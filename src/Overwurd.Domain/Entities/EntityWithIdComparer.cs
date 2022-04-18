namespace Overwurd.Domain.Entities;

public class EntityWithIdComparer : IEqualityComparer<IEntityWithId>
{
    public static readonly IEqualityComparer<IEntityWithId> Instance = new EntityWithIdComparer();

    public bool Equals(IEntityWithId? x, IEntityWithId? y)
    {
        if (x is null || y is null)
            return false;

        if (ReferenceEquals(x, y))
            return true;

        if (x.GetType() != y.GetType())
            return false;

        return x.Id == y.Id;
    }

    public int GetHashCode(IEntityWithId obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        return HashCode.Combine(obj.Id);
    }
}
namespace Overwurd.Domain.Entities;

public interface IEntityWithCreationTime
{
    public DateTimeOffset CreatedAt { get; }
}
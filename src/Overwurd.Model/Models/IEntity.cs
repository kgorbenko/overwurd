using System;

namespace Overwurd.Model.Models;

public interface IEntity : IEntityWithNumericId
{
    DateTimeOffset CreatedAt { get; }
}
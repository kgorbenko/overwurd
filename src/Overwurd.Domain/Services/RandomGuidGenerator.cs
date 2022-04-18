namespace Overwurd.Domain.Services;

public class RandomGuidGenerator : IRandomGuidGenerator
{
    public Guid Generate()
    {
        return Guid.NewGuid();
    }
}
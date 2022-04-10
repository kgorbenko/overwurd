namespace Overwurd.Domain.Services.Authentication;

public static class ParseUserIdHelper
{
    public static int Parse(string stringUserId)
    {
        var canParse = int.TryParse(stringUserId, out var parsedId);

        if (!canParse)
        {
            throw new InvalidOperationException($"Provided string User Id '{stringUserId}' cannot be parsed into int.");
        }

        return parsedId;
    }
}
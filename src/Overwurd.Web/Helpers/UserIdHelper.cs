using System;

namespace Overwurd.Web.Helpers;

public static class UserIdHelper
{
    public static int ParseUserId(this string stringUserId)
    {
        var canParse = int.TryParse(stringUserId, out var parsedId);

        if (!canParse)
        {
            throw new InvalidOperationException($"Provided string User Id '{stringUserId}' cannot be parsed into int.");
        }

        return parsedId;
    }
}
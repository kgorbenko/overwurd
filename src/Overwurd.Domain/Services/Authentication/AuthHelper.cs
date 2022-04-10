using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Overwurd.Domain.Services.Authentication;

public static class AuthHelper
{
    public static byte[] GetBytesFromSigningKey(string key) =>
        Encoding.ASCII.GetBytes(key);

    public static int GetUserIdFromAccessToken(string tokenString, string userIdClaimType)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(tokenString);
        var userIdString = token.Claims.Single(x => x.Type == userIdClaimType).Value;

        return ParseUserIdHelper.Parse(userIdString);
    }

    public static bool TryGetUserIdFromAccessToken(string tokenString, string userIdClaimType, out int id)
    {
        try
        {
            id = GetUserIdFromAccessToken(tokenString, userIdClaimType);
            return true;
        } catch
        {
            id = 0;
            return false;
        }
    }

    public static ImmutableArray<Claim> GetUserClaims(User user, IClaimsIdentityOptions claimsIdentityOptions)
    {
        return new Claim[]
        {
            new(claimsIdentityOptions.UserIdClaimType, user.Id.ToString()),
            new(claimsIdentityOptions.UserNameClaimType, user.Login.Value)
        }.ToImmutableArray();
    }
}
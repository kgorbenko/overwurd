using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Overwurd.Model.Models;

namespace Overwurd.Web.Services.Auth
{
    public static class AuthHelper
    {
        public static byte[] GetBytesFromSigningKey(string key) =>
            Encoding.ASCII.GetBytes(key);

        public static long GetUserIdFromAccessToken(string tokenString, string userIdClaimType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(tokenString);
            var userIdString = token.Claims.Single(x => x.Type == userIdClaimType).Value;

            return long.Parse(userIdString);
        }

        public static bool TryGetUserIdFromAccessToken(string tokenString, string userIdClaimType, out long id)
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

        public static ImmutableArray<Claim> GetUserClaims(User user, ClaimsIdentityOptions claimsIdentityOptions)
        {
            var identityClaims = new Claim[]
            {
                new(claimsIdentityOptions.UserIdClaimType, user.Id.ToString()),
                new(claimsIdentityOptions.UserNameClaimType, user.Login)
            };

            var roleClaims = user.Roles
                                 .Select(x => new Claim(claimsIdentityOptions.RoleClaimType, x.Name));

            return identityClaims.Concat(roleClaims).ToImmutableArray();
        }
    }
}
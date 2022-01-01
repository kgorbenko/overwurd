using System;
using System.Collections.Generic;
using Overwurd.Model.Models;

namespace Overwurd.Model.Tests.EqualityComparers
{
    public class JwtRefreshTokenComparerForTests : IEqualityComparer<JwtRefreshToken>
    {
        public bool Equals(JwtRefreshToken x, JwtRefreshToken y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.AccessTokenId == y.AccessTokenId &&
                   x.UserId == y.UserId &&
                   x.TokenString == y.TokenString &&
                   x.ExpiresAt.Equals(y.ExpiresAt) &&
                   x.CreatedAt.Equals(y.CreatedAt) &&
                   x.IsRevoked == y.IsRevoked;
        }

        public int GetHashCode(JwtRefreshToken obj)
        {
            return HashCode.Combine(obj.AccessTokenId, obj.UserId, obj.TokenString, obj.ExpiresAt, obj.CreatedAt, obj.IsRevoked);
        }
    }
}
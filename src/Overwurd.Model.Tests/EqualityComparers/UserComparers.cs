using System.Collections.Generic;
using Nito.Comparers;
using Overwurd.Model.Models;

namespace Overwurd.Model.Tests.EqualityComparers
{
    public static class UserComparers
    {
        public static readonly IEqualityComparer<User> UserRelationshipAgnosticComparer =
            EqualityComparerBuilder.For<User>()
                                   .EquateBy(x => x.Id)
                                   .ThenEquateBy(x => x.UserName)
                                   .ThenEquateBy(x => x.NormalizedUserName)
                                   .ThenEquateBy(x => x.Password)
                                   .ThenEquateBy(x => x.FirstName)
                                   .ThenEquateBy(x => x.LastName);
    }
}
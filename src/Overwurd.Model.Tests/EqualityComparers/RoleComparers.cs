using System.Collections.Generic;
using Nito.Comparers;
using Overwurd.Model.Models;

namespace Overwurd.Model.Tests.EqualityComparers;

public static class RoleComparers
{
    public static readonly IEqualityComparer<Role> RoleComparer =
        EqualityComparerBuilder.For<Role>()
                               .EquateBy(x => x.Name)
                               .ThenEquateBy(x => x.RoleType);
}
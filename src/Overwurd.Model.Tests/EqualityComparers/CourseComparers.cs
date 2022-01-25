using System.Collections.Generic;
using Nito.Comparers;
using Overwurd.Model.Models;

namespace Overwurd.Model.Tests.EqualityComparers;

public static class CourseComparers
{
    public static readonly IEqualityComparer<Course> CourseRelationshipAgnosticComparer =
        EqualityComparerBuilder.For<Course>()
                               .EquateBy(x => x.Id)
                               .ThenEquateBy(x => x.Name)
                               .ThenEquateBy(x => x.Description)
                               .ThenEquateBy(x => x.CreatedAt);
}
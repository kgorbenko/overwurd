using System.Collections.Generic;
using Nito.Comparers;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;

namespace Overwurd.Model.Tests.EqualityComparers;

public static class CourseComparers
{
    public static readonly IEqualityComparer<Course> CourseRelationshipAgnosticComparer =
        EqualityComparerBuilder.For<Course>()
                               .EquateBy(x => x.Id)
                               .ThenEquateBy(x => x.Name)
                               .ThenEquateBy(x => x.UserId)
                               .ThenEquateBy(x => x.Description)
                               .ThenEquateBy(x => x.CreatedAt);

    public static readonly IEqualityComparer<PaginationResult<Course>> PaginationResultComparer =
        EqualityComparerBuilder.For<PaginationResult<Course>>()
                               .EquateBy(x => x.Results, CourseRelationshipAgnosticComparer.EquateSequence())
                               .ThenEquateBy(x => x.TotalCount);
}
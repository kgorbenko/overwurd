using System.Collections.Generic;
using Nito.Comparers;
using Overwurd.Domain.Entities;

namespace Overwurd.Domain.Tests.Comparers;

public static class CourseComparer
{
    public static readonly IEqualityComparer<Course> Instance =
        EqualityComparerBuilder
            .For<Course>()
            .EquateBy(x => x.Id)
            .ThenEquateBy(x => x.CreatedAt)
            .ThenEquateBy(x => x.UserId)
            .ThenEquateBy(x => x.Name)
            .ThenEquateBy(x => x.Description)
            .ThenEquateBy(x => x.Vocabularies, VocabularyComparer.Instance.EquateSequence());
}
using System.Collections.Generic;
using Nito.Comparers;
using Overwurd.Domain.Entities;

namespace Overwurd.Domain.Tests.Comparers;

public static class VocabularyComparer
{
    public static readonly IEqualityComparer<Vocabulary> Instance =
        EqualityComparerBuilder
            .For<Vocabulary>()
            .EquateBy(x => x.Id)
            .ThenEquateBy(x => x.CreatedAt)
            .ThenEquateBy(x => x.CourseId)
            .ThenEquateBy(x => x.Name)
            .ThenEquateBy(x => x.Description);
}
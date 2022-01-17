using System.Collections.Generic;
using Nito.Comparers;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;

namespace Overwurd.Model.Tests.EqualityComparers
{
    public static class VocabularyComparers
    {
        public static readonly IEqualityComparer<Vocabulary> VocabularyComparer =
            EqualityComparerBuilder.For<Vocabulary>()
                                   .EquateBy(x => x.Id)
                                   .ThenEquateBy(x => x.Name)
                                   .ThenEquateBy(x => x.CreatedAt);

        public static readonly IEqualityComparer<PaginationResult<Vocabulary>> PaginationResultComparer =
            EqualityComparerBuilder.For<PaginationResult<Vocabulary>>()
                                   .EquateBy(x => x.Results, _ => VocabularyComparer.EquateSequence())
                                   .ThenEquateBy(x => x.TotalCount);
    }
}
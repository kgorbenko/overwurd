using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;

namespace Overwurd.Model.Tests.EqualityComparers {
    public class VocabulariesPaginationResultComparerForTests : IEqualityComparer<OverwurdPaginationResult<Vocabulary>> {
        private readonly IEqualityComparer<Vocabulary> vocabulariesComparer;

        public VocabulariesPaginationResultComparerForTests([NotNull] IEqualityComparer<Vocabulary> vocabulariesComparer) {
            this.vocabulariesComparer = vocabulariesComparer ?? throw new ArgumentNullException(nameof(vocabulariesComparer));
        }

        public bool Equals(OverwurdPaginationResult<Vocabulary> x, OverwurdPaginationResult<Vocabulary> y) {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, objB: null))
            {
                return false;
            }

            if (ReferenceEquals(y, objB: null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Results.SequenceEqual(y.Results, vocabulariesComparer) && x.TotalCount == y.TotalCount;
        }

        public int GetHashCode(OverwurdPaginationResult<Vocabulary> obj) {
            return HashCode.Combine(obj.Results, obj.TotalCount);
        }
    }
}
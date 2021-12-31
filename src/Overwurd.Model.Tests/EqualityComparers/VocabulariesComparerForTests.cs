using System;
using System.Collections.Generic;
using Overwurd.Model.Models;

namespace Overwurd.Model.Tests.EqualityComparers
{
    public class VocabulariesComparerForTests : IEqualityComparer<Vocabulary>
    {
        public bool Equals(Vocabulary x, Vocabulary y)
        {
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

            return x.Id == y.Id
                && x.Name.Equals(y.Name, StringComparison.Ordinal)
                && x.CreatedAt.Equals(y.CreatedAt);
        }

        public int GetHashCode(Vocabulary obj)
        {
            return HashCode.Combine(obj.Id, obj.Name, obj.CreatedAt);
        }
    }
}
using System.Collections.Immutable;

namespace Overwurd.Model.Repositories
{
    public record PaginationResult<T>(IImmutableList<T> Results, int TotalCount);
}
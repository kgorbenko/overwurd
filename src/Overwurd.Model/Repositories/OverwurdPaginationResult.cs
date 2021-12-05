using System.Collections.Immutable;

namespace Overwurd.Model.Repositories
{
    public record OverwurdPaginationResult<T>(IImmutableList<T> Results, int TotalCount);
}
using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Overwurd.Model.Models;

namespace Overwurd.Model.Repositories {
    public interface IReadOnlyOverwurdRepository<T> where T : IEntityWithNumericId {
        Task<T> FindByIdAsync(long id, CancellationToken cancellationToken = default);

        Task<OverwurdPaginationResult<T>> PaginateAsync(Expression<Func<T, bool>> predicate, int page, int pageSize, CancellationToken cancellationToken = default);

        Task<IImmutableList<T>> FindByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<IImmutableList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
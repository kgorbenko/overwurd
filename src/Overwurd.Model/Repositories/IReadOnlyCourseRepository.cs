using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Overwurd.Model.Models;

namespace Overwurd.Model.Repositories;

public interface IReadOnlyCourseRepository : IReadOnlyRepository<Course>
{
    Task<IImmutableList<Course>> GetUserCoursesAsync(int userId, CancellationToken cancellationToken);

    Task<PaginationResult<Course>> PaginateUserCoursesAsync(int userId, int page, int pageSize, CancellationToken cancellationToken);
}
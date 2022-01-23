using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Overwurd.Model.Models;

namespace Overwurd.Model.Repositories
{
    public interface IReadOnlyCourseRepository : IReadOnlyRepository<Course>
    {
        Task<IImmutableList<Course>> GetUserCoursesAsync(int userId, CancellationToken cancellationToken);
    }
}
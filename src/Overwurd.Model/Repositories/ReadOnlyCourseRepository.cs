using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Overwurd.Model.Models;

namespace Overwurd.Model.Repositories;

public class ReadOnlyCourseRepository : ReadOnlyRepository<Course>, IReadOnlyCourseRepository
{
    public ReadOnlyCourseRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<IImmutableList<Course>> GetUserCoursesAsync(int userId, CancellationToken cancellationToken)
    {
        return (await DbContext.Courses
                               .Where(x => x.User.Id == userId)
                               .OrderBy(x => x.CreatedAt)
                               .AsNoTracking()
                               .ToArrayAsync(cancellationToken: cancellationToken))
            .ToImmutableArray();
    }
}
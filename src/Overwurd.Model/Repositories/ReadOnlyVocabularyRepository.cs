using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Overwurd.Model.Models;

namespace Overwurd.Model.Repositories;

public class ReadOnlyVocabularyRepository : ReadOnlyRepository<Vocabulary>, IReadOnlyVocabularyRepository
{
    public ReadOnlyVocabularyRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<IImmutableList<Vocabulary>> GetCourseVocabulariesAsync(int courseId, CancellationToken cancellationToken)
    {
        return (await DbContext.Vocabularies
                               .Where(x => x.Course.Id == courseId)
                               .OrderBy(x => x.CreatedAt)
                               .AsNoTracking()
                               .ToArrayAsync(cancellationToken: cancellationToken))
            .ToImmutableList();
    }
}
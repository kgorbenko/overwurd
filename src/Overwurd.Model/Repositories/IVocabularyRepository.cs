using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Overwurd.Model.Models;

namespace Overwurd.Model.Repositories;

public interface IVocabularyRepository : IRepository<Vocabulary>
{
    Task<IImmutableList<Vocabulary>> GetCourseVocabulariesAsync(int courseId, CancellationToken cancellationToken);
}
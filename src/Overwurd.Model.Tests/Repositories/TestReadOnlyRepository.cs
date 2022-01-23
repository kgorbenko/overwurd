using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;
using static Overwurd.Model.Tests.EqualityComparers.VocabularyComparers;

namespace Overwurd.Model.Tests.Repositories {
    public class TestReadOnlyRepository : BaseModelDatabaseDependentTestFixture
    {
        [Test]
        public async Task TestFindByIdAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.AddRangeAsync(vocabulary1, vocabulary2);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new ReadOnlyRepository<Vocabulary, ApplicationDbContext>(context);

                var actual = await repository.FindByIdAsync(vocabulary1.Id);

                Assert.That(actual, Is.EqualTo(vocabulary1).Using(VocabularyComparer));
                Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
            }
        }

        [Test]
        public async Task TestFindBy()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.AddRangeAsync(vocabulary1, vocabulary2);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new ReadOnlyRepository<Vocabulary, ApplicationDbContext>(context);

                var actual = (await repository.FindByAsync(x => x.Name == "Vocabulary 2")).Single();

                Assert.That(actual, Is.EqualTo(vocabulary2).Using(VocabularyComparer));
                Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
            }
        }

        [Test]
        public async Task TestGetAllAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3", "Description 3");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new ReadOnlyRepository<Vocabulary, ApplicationDbContext>(context);

                var actual = (await repository.GetAllAsync()).ToArray();

                Assert.That(actual, Is.EqualTo(new[] { vocabulary1, vocabulary2, vocabulary3 }).Using(VocabularyComparer));
                Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
            }
        }

        [Test]
        public async Task TestPaginateAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3", "Description 3");

            var vocabulary4 = new Vocabulary("Vocabulary 4", "Description 4");
            var vocabulary5 = new Vocabulary("Vocabulary 5", "Description 5");
            var vocabulary6 = new Vocabulary("Vocabulary 6", "Description 6");

            var vocabulary7 = new Vocabulary("Vocabulary 7", "Description 7");
            var vocabulary8 = new Vocabulary("Vocabulary 8", "Description 8");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3, vocabulary4, vocabulary5, vocabulary6, vocabulary7, vocabulary8);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new ReadOnlyRepository<Vocabulary, ApplicationDbContext>(context);

                var firstPageActual = await repository.PaginateAsync(x => true, page: 1, pageSize: 3);
                var firstPageExpected = new PaginationResult<Vocabulary>(new[] { vocabulary1, vocabulary2, vocabulary3 }.ToImmutableArray(), 8);
                Assert.That(firstPageActual, Is.EqualTo(firstPageExpected).Using(PaginationResultComparer));

                var secondPageActual = await repository.PaginateAsync(x => true, page: 2, pageSize: 3);
                var secondPageExpected = new PaginationResult<Vocabulary>(new[] { vocabulary4, vocabulary5, vocabulary6 }.ToImmutableArray(), 8);
                Assert.That(secondPageActual, Is.EqualTo(secondPageExpected).Using(PaginationResultComparer));

                var thirdPageActual = await repository.PaginateAsync(x => true, page: 3, pageSize: 3);
                var thirdPageExpected = new PaginationResult<Vocabulary>(new[] { vocabulary7, vocabulary8 }.ToImmutableArray(), 8);
                Assert.That(thirdPageActual, Is.EqualTo(thirdPageExpected).Using(PaginationResultComparer));

                var fourthPageActual = await repository.PaginateAsync(x => true, page: 4, pageSize: 3);
                var fourthPageExpected = new PaginationResult<Vocabulary>(ImmutableArray<Vocabulary>.Empty, 8);
                Assert.That(fourthPageActual, Is.EqualTo(fourthPageExpected).Using(PaginationResultComparer));

                Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
            }
        }
    }
}
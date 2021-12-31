using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;
using Overwurd.Model.Tests.EqualityComparers;

namespace Overwurd.Model.Tests.Repositories {
    public class TestReadOnlyOverwurdRepository : OverwurdBaseDatabaseDependentTestFixture {
        private static readonly IEqualityComparer<Vocabulary> vocabulariesComparer = new VocabulariesComparerForTests();

        private static readonly IEqualityComparer<OverwurdPaginationResult<Vocabulary>> vocabulariesPaginationResultComparer =
            new VocabulariesPaginationResultComparerForTests(vocabulariesComparer);

        [Test]
        public async Task TestFindByIdAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");

            await using (var context = new ModelDbContext(ContextOptionsBuilder))
            {
                await context.AddRangeAsync(vocabulary1, vocabulary2);
                await context.SaveChangesAsync();
            }

            await using (var context = new ModelDbContext(ContextOptionsBuilder))
            {
                var repository = new ReadOnlyOverwurdRepository<Vocabulary, ModelDbContext>(context);

                var actual = await repository.FindByIdAsync(vocabulary1.Id);

                Assert.That(actual, Is.EqualTo(vocabulary1).Using(vocabulariesComparer));
                Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
            }
        }

        [Test]
        public async Task TestFindBy()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");

            await using (var context = new ModelDbContext(ContextOptionsBuilder))
            {
                await context.AddRangeAsync(vocabulary1, vocabulary2);
                await context.SaveChangesAsync();
            }

            await using (var context = new ModelDbContext(ContextOptionsBuilder))
            {
                var repository = new ReadOnlyOverwurdRepository<Vocabulary, ModelDbContext>(context);

                var actual = (await repository.FindByAsync(x => x.Name == "Vocabulary 2")).Single();

                Assert.That(actual, Is.EqualTo(vocabulary2).Using(vocabulariesComparer));
                Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
            }
        }

        [Test]
        public async Task TestGetAllAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3");

            await using (var context = new ModelDbContext(ContextOptionsBuilder))
            {
                await context.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3);
                await context.SaveChangesAsync();
            }

            await using (var context = new ModelDbContext(ContextOptionsBuilder))
            {
                var repository = new ReadOnlyOverwurdRepository<Vocabulary, ModelDbContext>(context);

                var actual = (await repository.GetAllAsync()).ToArray();

                Assert.That(actual, Is.EqualTo(new[] { vocabulary1, vocabulary2, vocabulary3 }).Using(vocabulariesComparer));
                Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
            }
        }

        [Test]
        public async Task TestPaginateAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3");

            var vocabulary4 = new Vocabulary("Vocabulary 4");
            var vocabulary5 = new Vocabulary("Vocabulary 5");
            var vocabulary6 = new Vocabulary("Vocabulary 6");

            var vocabulary7 = new Vocabulary("Vocabulary 7");
            var vocabulary8 = new Vocabulary("Vocabulary 8");

            await using (var context = new ModelDbContext(ContextOptionsBuilder))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3, vocabulary4, vocabulary5, vocabulary6, vocabulary7, vocabulary8);
                await context.SaveChangesAsync();
            }

            await using (var context = new ModelDbContext(ContextOptionsBuilder))
            {
                var repository = new ReadOnlyOverwurdRepository<Vocabulary, ModelDbContext>(context);

                var firstPageActual = await repository.PaginateAsync(x => true, page: 1, pageSize: 3);
                var firstPageExpected = new OverwurdPaginationResult<Vocabulary>(new[] { vocabulary1, vocabulary2, vocabulary3 }.ToImmutableArray(), 8);
                Assert.That(firstPageActual, Is.EqualTo(firstPageExpected).Using(vocabulariesPaginationResultComparer));

                var secondPageActual = await repository.PaginateAsync(x => true, page: 2, pageSize: 3);
                var secondPageExpected = new OverwurdPaginationResult<Vocabulary>(new[] { vocabulary4, vocabulary5, vocabulary6 }.ToImmutableArray(), 8);
                Assert.That(secondPageActual, Is.EqualTo(secondPageExpected).Using(vocabulariesPaginationResultComparer));

                var thirdPageActual = await repository.PaginateAsync(x => true, page: 3, pageSize: 3);
                var thirdPageExpected = new OverwurdPaginationResult<Vocabulary>(new[] { vocabulary7, vocabulary8 }.ToImmutableArray(), 8);
                Assert.That(secondPageActual, Is.EqualTo(secondPageExpected).Using(vocabulariesPaginationResultComparer));

                var fourthPageActual = await repository.PaginateAsync(x => true, page: 4, pageSize: 3);
                var fourthPageExpected = new OverwurdPaginationResult<Vocabulary>(ImmutableArray<Vocabulary>.Empty, 8);

                Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
            }
        }
    }
}
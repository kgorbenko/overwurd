using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;
using Overwurd.Model.Tests.EqualityComparers;

namespace Overwurd.Model.Tests.Repositories
{
    public class TestOverwurdRepository : BaseModelDatabaseDependentTestFixture
    {
        private readonly IEqualityComparer<Vocabulary> vocabulariesComparer = new VocabulariesComparerForTests();

        [Test]
        public async Task TestAddAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);

                await repository.AddAsync(vocabulary1);
                await repository.AddAsync(vocabulary2);
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var expected = new[] { vocabulary1, vocabulary2 };
                var actual = await context.Vocabularies.ToArrayAsync();

                Assert.That(actual, Is.EqualTo(expected).Using(vocabulariesComparer));
            }
        }

        [Test]
        public async Task TestAddRangeAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);

                await repository.AddRangeAsync(new[] { vocabulary1, vocabulary2, vocabulary3 }.ToImmutableArray());
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var actual = await context.Vocabularies.ToArrayAsync();

                Assert.That(actual, Is.EqualTo(new[] { vocabulary1, vocabulary2, vocabulary3 }).Using(vocabulariesComparer));
            }
        }

        [Test]
        public async Task TestGetAllAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);
                var actual = await repository.GetAllAsync();

                Assert.That(actual, Is.EqualTo(new[] { vocabulary1, vocabulary2, vocabulary3 }).Using(vocabulariesComparer));
            }
        }

        [Test]
        public async Task TestFindByIdAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);

                Assert.That(await repository.FindByIdAsync(vocabulary1.Id), Is.EqualTo(vocabulary1).Using(vocabulariesComparer));
                Assert.That(await repository.FindByIdAsync(vocabulary2.Id), Is.EqualTo(vocabulary2).Using(vocabulariesComparer));
                Assert.That(await repository.FindByIdAsync(vocabulary3.Id), Is.EqualTo(vocabulary3).Using(vocabulariesComparer));
            }

        }

        [Test]
        public async Task TestFindByAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);

                Assert.That(await repository.FindByAsync(x => x.Id == vocabulary1.Id), Is.EqualTo(new[] { vocabulary1 }).Using(vocabulariesComparer));
                Assert.That(await repository.FindByAsync(x => x.Name == "Vocabulary 2"), Is.EqualTo(new[] { vocabulary2 }).Using(vocabulariesComparer));
            }
        }

        [Test]
        public async Task TestUpdateAsyncAddedEntities()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);

                vocabulary1.Name = "Some New Test Name";
                vocabulary2.Name = "Some Another Test Name";

                await repository.UpdateAsync(vocabulary1);
                await repository.UpdateAsync(vocabulary2);
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var vocabulary1Updated = await context.Vocabularies.FindAsync(vocabulary1.Id);
                Assert.That(vocabulary1Updated, Is.EqualTo(vocabulary1).Using(vocabulariesComparer));

                var vocabulary2Updated = await context.Vocabularies.FindAsync(vocabulary2.Id);
                Assert.That(vocabulary2Updated, Is.EqualTo(vocabulary2).Using(vocabulariesComparer));
            }
        }

        [Test]
        public async Task TestUpdateAsyncAfterGetting()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2);
                await context.SaveChangesAsync();
            }

            Vocabulary vocabulary1Found, vocabulary2Found;

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);

                vocabulary1Found = await repository.FindByIdAsync(vocabulary1.Id);
                vocabulary2Found = await repository.FindByIdAsync(vocabulary2.Id);

                vocabulary1Found.Name = "Some New Test Name";
                vocabulary2Found.Name = "Some Another Test Name";

                await repository.UpdateAsync(vocabulary1Found);
                await repository.UpdateAsync(vocabulary2Found);
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var vocabulary1Updated = await context.Vocabularies.FindAsync(vocabulary1.Id);
                Assert.That(vocabulary1Updated, Is.EqualTo(vocabulary1Found).Using(vocabulariesComparer));

                var vocabulary2Updated = await context.Vocabularies.FindAsync(vocabulary2.Id);
                Assert.That(vocabulary2Updated, Is.EqualTo(vocabulary2Found).Using(vocabulariesComparer));
            }
        }

        [Test]
        public async Task TestRemoveAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);

                await repository.RemoveAsync(vocabulary1);
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                Assert.That(await context.Vocabularies.ToArrayAsync(), Is.EqualTo(new[] { vocabulary2, vocabulary3 }).Using(vocabulariesComparer));
            }
        }

        [Test]
        public async Task RemoveNonExistingEntityThrowsException()
        {
            await using var context = new ApplicationDbContext(ContextOptions);
            var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);

            var vocabulary1 = new Vocabulary("Vocabulary 1");
            vocabulary1.GetType().GetProperty("Id")!.SetValue(vocabulary1, value: 1);

            Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await repository.RemoveAsync(vocabulary1));
        }

        [Test]
        public async Task TestRemoveRangeAsync()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3");
            var vocabulary4 = new Vocabulary("Vocabulary 4");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3, vocabulary4);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);

                await repository.RemoveRangeAsync(new[] { vocabulary1, vocabulary3 }.ToImmutableArray());
                Assert.That(await context.Vocabularies.ToArrayAsync(), Is.EqualTo(new[] { vocabulary2, vocabulary4 }).Using(vocabulariesComparer));
            }
        }

        [Test]
        public async Task TestPaginate()
        {
            var vocabulary1 = new Vocabulary("Vocabulary 1");
            var vocabulary2 = new Vocabulary("Vocabulary 2");
            var vocabulary3 = new Vocabulary("Vocabulary 3");

            var vocabulary4 = new Vocabulary("Vocabulary 4");
            var vocabulary5 = new Vocabulary("Vocabulary 5");
            var vocabulary6 = new Vocabulary("Vocabulary 6");

            var vocabulary7 = new Vocabulary("Vocabulary 7");
            var vocabulary8 = new Vocabulary("Vocabulary 8");

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3, vocabulary4, vocabulary5, vocabulary6, vocabulary7, vocabulary8);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new OverwurdRepository<Vocabulary, ApplicationDbContext>(context);

                var firstPage = await repository.PaginateByAsync(x => true, page: 1, pageSize: 3);
                Assert.That(firstPage.TotalCount, Is.EqualTo(8));
                Assert.That(firstPage.Results, Is.EqualTo(new[] { vocabulary1, vocabulary2, vocabulary3 }).Using(vocabulariesComparer));

                var secondPage = await repository.PaginateByAsync(x => true, page: 2, pageSize: 3);
                Assert.That(secondPage.TotalCount, Is.EqualTo(8));
                Assert.That(secondPage.Results, Is.EqualTo(new[] { vocabulary4, vocabulary5, vocabulary6 }).Using(vocabulariesComparer));

                var thirdPage = await repository.PaginateByAsync(x => true, page: 3, pageSize: 3);
                Assert.That(thirdPage.TotalCount, Is.EqualTo(8));
                Assert.That(thirdPage.Results, Is.EqualTo(new[] { vocabulary7, vocabulary8 }).Using(vocabulariesComparer));

                var fourthPage = await repository.PaginateByAsync(x => true, page: 4, pageSize: 3);
                Assert.That(fourthPage.TotalCount, Is.EqualTo(8));
                Assert.That(fourthPage.Results, Is.Empty);
            }
        }
    }
}
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;
using static Overwurd.Model.Tests.EqualityComparers.VocabularyComparers;

namespace Overwurd.Model.Tests.Repositories
{
    public class TestRepository : BaseModelDatabaseDependentTestFixture
    {
        private async Task<Course> CreateAndStoreCourseAsync()
        {
            await using var context = new ApplicationDbContext(ContextOptions);

            var user = new User { UserName = "Test User" };
            await context.Users.AddAsync(user);

            var course = new Course(name: "Course test name", description: "Course test description")
            {
                User = user
            };
            await context.Courses.AddAsync(course);

            return course;
        }

        private static Vocabulary CreateVocabulary(string name, string description, Course course)
        {
            var vocabulary = new Vocabulary(name: name, description: description)
            {
                Course = course
            };

            return vocabulary;
        }

        [Test]
        public async Task TestAddAsync()
        {
            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            var vocabulary2 = CreateVocabulary("Vocabulary 2", "Description 2", course);

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new Repository<Vocabulary>(context);

                await repository.AddAsync(vocabulary1);
                await repository.AddAsync(vocabulary2);
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var expected = new[] { vocabulary1, vocabulary2 };
                var actual = await context.Vocabularies.ToArrayAsync();

                Assert.That(actual, Is.EqualTo(expected).Using(VocabularyRelationshipAgnosticComparer));
            }
        }

        [Test]
        public async Task TestAddRangeAsync()
        {
            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            var vocabulary2 = CreateVocabulary("Vocabulary 2", "Description 2", course);
            var vocabulary3 = CreateVocabulary("Vocabulary 3", "Description 3", course);

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new Repository<Vocabulary>(context);

                await repository.AddRangeAsync(new[] { vocabulary1, vocabulary2, vocabulary3 }.ToImmutableArray());
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var actual = await context.Vocabularies.ToArrayAsync();

                Assert.That(actual, Is.EqualTo(new[] { vocabulary1, vocabulary2, vocabulary3 }).Using(VocabularyRelationshipAgnosticComparer));
            }
        }

        [Test]
        public async Task TestGetAllAsync()
        {
            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            var vocabulary2 = CreateVocabulary("Vocabulary 2", "Description 2", course);
            var vocabulary3 = CreateVocabulary("Vocabulary 3", "Description 3", course);

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new Repository<Vocabulary>(context);
                var actual = await repository.GetAllAsync();

                Assert.That(actual, Is.EqualTo(new[] { vocabulary1, vocabulary2, vocabulary3 }).Using(VocabularyRelationshipAgnosticComparer));
            }
        }

        [Test]
        public async Task TestFindByIdAsync()
        {
            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            var vocabulary2 = CreateVocabulary("Vocabulary 2", "Description 2", course);
            var vocabulary3 = CreateVocabulary("Vocabulary 3", "Description 3", course);

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new Repository<Vocabulary>(context);

                Assert.That(await repository.FindByIdAsync(vocabulary1.Id), Is.EqualTo(vocabulary1).Using(VocabularyRelationshipAgnosticComparer));
                Assert.That(await repository.FindByIdAsync(vocabulary2.Id), Is.EqualTo(vocabulary2).Using(VocabularyRelationshipAgnosticComparer));
                Assert.That(await repository.FindByIdAsync(vocabulary3.Id), Is.EqualTo(vocabulary3).Using(VocabularyRelationshipAgnosticComparer));
            }

        }

        [Test]
        public async Task TestFindByAsync()
        {
            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            var vocabulary2 = CreateVocabulary("Vocabulary 2", "Description 2", course);
            var vocabulary3 = CreateVocabulary("Vocabulary 3", "Description 3", course);

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new Repository<Vocabulary>(context);

                Assert.That(await repository.FindByAsync(x => x.Id == vocabulary1.Id), Is.EqualTo(new[] { vocabulary1 }).Using(VocabularyRelationshipAgnosticComparer));
                Assert.That(await repository.FindByAsync(x => x.Name == "Vocabulary 2"), Is.EqualTo(new[] { vocabulary2 }).Using(VocabularyRelationshipAgnosticComparer));
            }
        }

        [Test]
        public async Task TestUpdateAsyncAddedEntities()
        {
            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            var vocabulary2 = CreateVocabulary("Vocabulary 2", "Description 2", course);

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new Repository<Vocabulary>(context);

                vocabulary1.Name = "Some New Test Name";
                vocabulary2.Name = "Some Another Test Name";

                await repository.UpdateAsync(vocabulary1);
                await repository.UpdateAsync(vocabulary2);
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var vocabulary1Updated = await context.Vocabularies.FindAsync(vocabulary1.Id);
                Assert.That(vocabulary1Updated, Is.EqualTo(vocabulary1).Using(VocabularyRelationshipAgnosticComparer));

                var vocabulary2Updated = await context.Vocabularies.FindAsync(vocabulary2.Id);
                Assert.That(vocabulary2Updated, Is.EqualTo(vocabulary2).Using(VocabularyRelationshipAgnosticComparer));
            }
        }

        [Test]
        public async Task TestUpdateAsyncAfterGetting()
        {
            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            var vocabulary2 = CreateVocabulary("Vocabulary 2", "Description 2", course);

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2);
                await context.SaveChangesAsync();
            }

            Vocabulary vocabulary1Found, vocabulary2Found;

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new Repository<Vocabulary>(context);

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
                Assert.That(vocabulary1Updated, Is.EqualTo(vocabulary1Found).Using(VocabularyRelationshipAgnosticComparer));

                var vocabulary2Updated = await context.Vocabularies.FindAsync(vocabulary2.Id);
                Assert.That(vocabulary2Updated, Is.EqualTo(vocabulary2Found).Using(VocabularyRelationshipAgnosticComparer));
            }
        }

        [Test]
        public async Task TestRemoveAsync()
        {
            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            var vocabulary2 = CreateVocabulary("Vocabulary 2", "Description 2", course);
            var vocabulary3 = CreateVocabulary("Vocabulary 3", "Description 3", course);

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new Repository<Vocabulary>(context);

                await repository.RemoveAsync(vocabulary1);
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                Assert.That(await context.Vocabularies.ToArrayAsync(), Is.EqualTo(new[] { vocabulary2, vocabulary3 }).Using(VocabularyRelationshipAgnosticComparer));
            }
        }

        [Test]
        public async Task RemoveNonExistingEntityThrowsException()
        {
            await using var context = new ApplicationDbContext(ContextOptions);
            var repository = new Repository<Vocabulary>(context);

            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            vocabulary1.GetType().GetProperty("Id")!.SetValue(vocabulary1, value: 1);

            Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await repository.RemoveAsync(vocabulary1));
        }

        [Test]
        public async Task TestRemoveRangeAsync()
        {
            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            var vocabulary2 = CreateVocabulary("Vocabulary 2", "Description 2", course);
            var vocabulary3 = CreateVocabulary("Vocabulary 3", "Description 3", course);
            var vocabulary4 = CreateVocabulary("Vocabulary 4", "Description 4", course);

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3, vocabulary4);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new Repository<Vocabulary>(context);

                await repository.RemoveRangeAsync(new[] { vocabulary1, vocabulary3 }.ToImmutableArray());
                Assert.That(await context.Vocabularies.ToArrayAsync(), Is.EqualTo(new[] { vocabulary2, vocabulary4 }).Using(VocabularyRelationshipAgnosticComparer));
            }
        }

        [Test]
        public async Task TestPaginate()
        {
            var course = await CreateAndStoreCourseAsync();
            var vocabulary1 = CreateVocabulary("Vocabulary 1", "Description 1", course);
            var vocabulary2 = CreateVocabulary("Vocabulary 2", "Description 2", course);
            var vocabulary3 = CreateVocabulary("Vocabulary 3", "Description 3", course);

            var vocabulary4 = CreateVocabulary("Vocabulary 4", "Description 4", course);
            var vocabulary5 = CreateVocabulary("Vocabulary 5", "Description 5", course);
            var vocabulary6 = CreateVocabulary("Vocabulary 6", "Description 6", course);

            var vocabulary7 = CreateVocabulary("Vocabulary 7", "Description 7", course);
            var vocabulary8 = CreateVocabulary("Vocabulary 8", "Description 8", course);

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Vocabularies.AddRangeAsync(vocabulary1, vocabulary2, vocabulary3, vocabulary4, vocabulary5, vocabulary6, vocabulary7, vocabulary8);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var repository = new Repository<Vocabulary>(context);

                var firstPage = await repository.PaginateByAsync(x => true, page: 1, pageSize: 3);
                Assert.That(firstPage.TotalCount, Is.EqualTo(8));
                Assert.That(firstPage.Results, Is.EqualTo(new[] { vocabulary1, vocabulary2, vocabulary3 }).Using(VocabularyRelationshipAgnosticComparer));

                var secondPage = await repository.PaginateByAsync(x => true, page: 2, pageSize: 3);
                Assert.That(secondPage.TotalCount, Is.EqualTo(8));
                Assert.That(secondPage.Results, Is.EqualTo(new[] { vocabulary4, vocabulary5, vocabulary6 }).Using(VocabularyRelationshipAgnosticComparer));

                var thirdPage = await repository.PaginateByAsync(x => true, page: 3, pageSize: 3);
                Assert.That(thirdPage.TotalCount, Is.EqualTo(8));
                Assert.That(thirdPage.Results, Is.EqualTo(new[] { vocabulary7, vocabulary8 }).Using(VocabularyRelationshipAgnosticComparer));

                var fourthPage = await repository.PaginateByAsync(x => true, page: 4, pageSize: 3);
                Assert.That(fourthPage.TotalCount, Is.EqualTo(8));
                Assert.That(fourthPage.Results, Is.Empty);
            }
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;
using static Overwurd.Model.Tests.EqualityComparers.VocabularyComparers;

namespace Overwurd.Model.Tests.Repositories;

public class TestVocabularyRepository : BaseModelDatabaseDependentTestFixture
{
    private async Task StoreVocabulariesAsync(params Vocabulary[] vocabularies)
    {
        await using var context = new ApplicationDbContext(ContextOptions);
        await context.Vocabularies.AddRangeAsync(vocabularies);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task TestGetCourseVocabulariesAsyncNoVocabularies()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course(name: "Course", description: "Description") { User = user };

        await using (var context = new ApplicationDbContext(ContextOptions))
        {
            await context.Courses.AddAsync(course);
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(ContextOptions))
        {
            var repository = new VocabularyRepository(context);

            var result = await repository.GetCourseVocabulariesAsync(course.Id, CancellationToken.None);

            Assert.That(result, Is.Empty);
        }
    }

    [Test]
    public async Task TestGetUserCoursesAsyncSimple()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course(name: "Course", description: "Description") { User = user };
        var vocabulary1 = new Vocabulary(name: "Vocabulary 1", description: "Description 1") { Course = course };
        var vocabulary2 = new Vocabulary(name: "Vocabulary 2", description: "Description 2") { Course = course };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new VocabularyRepository(context);

        var result = await repository.GetCourseVocabulariesAsync(course.Id, CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { vocabulary1, vocabulary2 }).Using(VocabularyRelationshipAgnosticComparer));
    }

    [Test]
    public async Task TestGetCourseVocabulariesAsyncDoesNotTakeVocabulariesFromAnotherCourse()
    {
        var user1 = new User { UserName = "Test User 1" };
        var course1 = new Course(name: "Course 1", description: "Description 1") { User = user1 };
        var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1") { Course = course1 };

        var user2 = new User { UserName = "Test User 2" };
        var course2 = new Course(name: "Course 2", description: "Description 2") { User = user2 };
        var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2") { Course = course2 };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new VocabularyRepository(context);

        var result = await repository.GetCourseVocabulariesAsync(course1.Id, CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { vocabulary1 }).Using(VocabularyRelationshipAgnosticComparer));
    }

    [Test]
    public async Task TestGetUserCoursesAsyncOrdersByCourseName()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course(name: "Course", description: "Description") { User = user };

        var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1") { Course = course };
        await Task.Delay(TimeSpan.FromSeconds(1));
        var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2") { Course = course };
        await Task.Delay(TimeSpan.FromSeconds(1));
        var vocabulary3 = new Vocabulary("Vocabulary 3", "Description 3") { Course = course };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2, vocabulary3);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new VocabularyRepository(context);

        var result = await repository.GetCourseVocabulariesAsync(course.Id, CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { vocabulary1, vocabulary2, vocabulary3 }).Using(VocabularyRelationshipAgnosticComparer));
    }

    [Test]
    public async Task TestCountCourseVocabulariesAsync()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course(name: "Course", description: "Description") { User = user };
        var vocabulary1 = new Vocabulary(name: "Vocabulary 1", description: "Description 1") { Course = course };
        var vocabulary2 = new Vocabulary(name: "Vocabulary 2", description: "Description 2") { Course = course };
        var vocabulary3 = new Vocabulary(name: "Vocabulary 3", description: "Description 3") { Course = course };
        var vocabulary4 = new Vocabulary(name: "Vocabulary 4", description: "Description 4") { Course = course };
        var vocabulary5 = new Vocabulary(name: "Vocabulary 5", description: "Description 5") { Course = course };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2, vocabulary3, vocabulary4, vocabulary5);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new VocabularyRepository(context);

        var result = await repository.CountCourseVocabulariesAsync(course.Id, CancellationToken.None);

        Assert.That(result, Is.EqualTo(5));
    }
}
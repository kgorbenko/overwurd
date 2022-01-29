using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;
using static Overwurd.Model.Tests.EqualityComparers.VocabularyComparers;

namespace Overwurd.Model.Tests.Repositories;

public class TestReadOnlyRepository : BaseModelDatabaseDependentTestFixture
{
    private async Task StoreVocabulariesAsync(params Vocabulary[] vocabularies)
    {
        await using var context = new ApplicationDbContext(ContextOptions);
        await context.Vocabularies.AddRangeAsync(vocabularies);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task TestFindByIdAsync()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course("Course", "Description") { User = user };
        var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1") { Course = course };
        var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2") { Course = course };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyRepository<Vocabulary>(context);

        var actual = await repository.FindByIdAsync(vocabulary1.Id);

        Assert.That(actual, Is.EqualTo(vocabulary1).Using(VocabularyRelationshipAgnosticComparer));
        Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
    }

    [Test]
    public async Task TestFindBy()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course("Course", "Description") { User = user };
        var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1") { Course = course };
        var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2") { Course = course };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyRepository<Vocabulary>(context);

        var actual = (await repository.FindByAsync(x => x.Name == "Vocabulary 2")).Single();

        Assert.That(actual, Is.EqualTo(vocabulary2).Using(VocabularyRelationshipAgnosticComparer));
        Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
    }

    [Test]
    public async Task TestGetAllAsync()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course("Course", "Description") { User = user };
        var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1") { Course = course };
        var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2") { Course = course };
        var vocabulary3 = new Vocabulary("Vocabulary 3", "Description 3") { Course = course };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2, vocabulary3);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyRepository<Vocabulary>(context);

        var actual = (await repository.GetAllAsync()).ToArray();

        Assert.That(actual, Is.EqualTo(new[] { vocabulary1, vocabulary2, vocabulary3 }).Using(VocabularyRelationshipAgnosticComparer));
        Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
    }

    [Test]
    public async Task TestPaginateAsync()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course("Course", "Description") { User = user };
        var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1") { Course = course };
        var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2") { Course = course };
        var vocabulary3 = new Vocabulary("Vocabulary 3", "Description 3") { Course = course };

        var vocabulary4 = new Vocabulary("Vocabulary 4", "Description 4") { Course = course };
        var vocabulary5 = new Vocabulary("Vocabulary 5", "Description 5") { Course = course };
        var vocabulary6 = new Vocabulary("Vocabulary 6", "Description 6") { Course = course };

        var vocabulary7 = new Vocabulary("Vocabulary 7", "Description 7") { Course = course };
        var vocabulary8 = new Vocabulary("Vocabulary 8", "Description 8") { Course = course };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2, vocabulary3, vocabulary4, vocabulary5, vocabulary6, vocabulary7, vocabulary8);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyRepository<Vocabulary>(context);

        var firstPageActual = await repository.PaginateByAsync(x => true, page: 1, pageSize: 3);
        var firstPageExpected = new PaginationResult<Vocabulary>(new[] { vocabulary8, vocabulary7, vocabulary6 }.ToImmutableArray(), 8);
        Assert.That(firstPageActual, Is.EqualTo(firstPageExpected).Using(PaginationResultComparer));

        var secondPageActual = await repository.PaginateByAsync(x => true, page: 2, pageSize: 3);
        var secondPageExpected = new PaginationResult<Vocabulary>(new[] { vocabulary5, vocabulary4, vocabulary3 }.ToImmutableArray(), 8);
        Assert.That(secondPageActual, Is.EqualTo(secondPageExpected).Using(PaginationResultComparer));

        var thirdPageActual = await repository.PaginateByAsync(x => true, page: 3, pageSize: 3);
        var thirdPageExpected = new PaginationResult<Vocabulary>(new[] { vocabulary2, vocabulary1 }.ToImmutableArray(), 8);
        Assert.That(thirdPageActual, Is.EqualTo(thirdPageExpected).Using(PaginationResultComparer));

        var fourthPageActual = await repository.PaginateByAsync(x => true, page: 4, pageSize: 3);
        var fourthPageExpected = new PaginationResult<Vocabulary>(ImmutableArray<Vocabulary>.Empty, 8);
        Assert.That(fourthPageActual, Is.EqualTo(fourthPageExpected).Using(PaginationResultComparer));

        Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
    }

    [Test]
    public async Task TestPaginateWithFilter()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course("Course", "Description") { User = user };
        var vocabulary1 = new Vocabulary("Vocabulary 1", "Description") { Course = course };
        var vocabulary2 = new Vocabulary("Vocabulary 2", "Another Description") { Course = course };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyRepository<Vocabulary>(context);

        var firstPageActual = await repository.PaginateByAsync(x => x.Description == "Description", page: 1, pageSize: 1);
        var firstPageExpected = new PaginationResult<Vocabulary>(new[] { vocabulary1 }.ToImmutableArray(), TotalCount: 1);
        Assert.That(firstPageActual, Is.EqualTo(firstPageExpected).Using(PaginationResultComparer));

        var thirdPageActual = await repository.PaginateByAsync(x => x.Description == "Description", page: 2, pageSize: 1);
        var thirdPageExpected = new PaginationResult<Vocabulary>(ImmutableArray<Vocabulary>.Empty, TotalCount: 1);
        Assert.That(thirdPageActual, Is.EqualTo(thirdPageExpected).Using(PaginationResultComparer));

        Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
    }

    [Test]
    public async Task TestCountByEmpty()
    {
        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyRepository<Vocabulary>(context);

        var actual = await repository.CountByAsync(x => true);

        Assert.That(actual, Is.EqualTo(0));
        Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
    }

    [Test]
    public async Task TestCountByFilter()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course("Course", "Description") { User = user };
        var vocabulary1 = new Vocabulary("Vocabulary 1", "Description") { Course = course };
        var vocabulary2 = new Vocabulary("Vocabulary 2", "Another Description") { Course = course };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyRepository<Vocabulary>(context);

        var actual = await repository.CountByAsync(x => x.Name == "Vocabulary 1");

        Assert.That(actual, Is.EqualTo(1));
        Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
    }

    [Test]
    public async Task TestCountByNumber()
    {
        var user = new User { UserName = "Test User" };
        var course = new Course("Course", "Description") { User = user };
        var vocabulary1 = new Vocabulary("Vocabulary 1", "Description 1") { Course = course };
        var vocabulary2 = new Vocabulary("Vocabulary 2", "Description 2") { Course = course };
        var vocabulary3 = new Vocabulary("Vocabulary 3", "Description 3") { Course = course };
        var vocabulary4 = new Vocabulary("Vocabulary 4", "Description 4") { Course = course };
        var vocabulary5 = new Vocabulary("Vocabulary 5", "Description 5") { Course = course };

        await StoreVocabulariesAsync(vocabulary1, vocabulary2, vocabulary3, vocabulary4, vocabulary5);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyRepository<Vocabulary>(context);

        var actual = await repository.CountByAsync(x => x.CourseId == course.Id);

        Assert.That(actual, Is.EqualTo(5));
        Assert.That(context.ChangeTracker.Entries<Vocabulary>(), Is.Empty);
    }
}
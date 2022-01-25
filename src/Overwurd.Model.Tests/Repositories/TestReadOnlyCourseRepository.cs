using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;
using static Overwurd.Model.Tests.EqualityComparers.CourseComparers;

namespace Overwurd.Model.Tests.Repositories;

public class TestReadOnlyCourseRepository : BaseModelDatabaseDependentTestFixture
{
    private async Task StoreCoursesAsync(params Course[] courses)
    {
        await using var context = new ApplicationDbContext(ContextOptions);
        await context.Courses.AddRangeAsync(courses);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task TestGetUserCoursesAsyncNoCourses()
    {
        var user = new User { UserName = "Test User" };

        await using (var context = new ApplicationDbContext(ContextOptions))
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(ContextOptions))
        {
            var repository = new ReadOnlyCourseRepository(context);

            var result = await repository.GetUserCoursesAsync(user.Id, CancellationToken.None);

            Assert.That(result, Is.Empty);
            Assert.That(context.ChangeTracker.Entries<Course>(), Is.Empty);
        }
    }

    [Test]
    public async Task TestGetUserCoursesAsyncSimple()
    {
        var user = new User { UserName = "Test User" };
        var course1 = new Course(name: "Course 1", description: "Description 1") { User = user };
        var course2 = new Course(name: "Course 2", description: "Description 2") { User = user };
        var course3 = new Course(name: "Course 3", description: "Description 3") { User = user };

        await StoreCoursesAsync(course1, course2, course3);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyCourseRepository(context);

        var result = await repository.GetUserCoursesAsync(user.Id, CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { course1, course2, course3 }).Using(CourseRelationshipAgnosticComparer));
        Assert.That(context.ChangeTracker.Entries<Course>(), Is.Empty);
    }

    [Test]
    public async Task TestGetUserCoursesAsyncDoesNotTakeCoursesFromAnotherUser()
    {
        var user1 = new User { UserName = "Test User 1" };
        var course1 = new Course(name: "Course 1", description: "Description 1") { User = user1 };

        var user2 = new User { UserName = "Test User 2" };
        var course2 = new Course(name: "Course 2", description: "Description 2") { User = user2 };

        await StoreCoursesAsync(course1, course2);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyCourseRepository(context);

        var result = await repository.GetUserCoursesAsync(user1.Id, CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { course1 }).Using(CourseRelationshipAgnosticComparer));
        Assert.That(context.ChangeTracker.Entries<Course>(), Is.Empty);
    }

    [Test]
    public async Task TestGetUserCoursesAsyncOrdersByCourseName()
    {
        var user = new User { UserName = "Test User" };

        var course1 = new Course(name: "CourseC", description: "Description 1") { User = user };
        await Task.Delay(TimeSpan.FromSeconds(1));
        var course2 = new Course(name: "CourseB", description: "Description 2") { User = user };
        await Task.Delay(TimeSpan.FromSeconds(1));
        var course3 = new Course(name: "CourseA", description: "Description 3") { User = user };

        await StoreCoursesAsync(course1, course2, course3);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new ReadOnlyCourseRepository(context);

        var result = await repository.GetUserCoursesAsync(user.Id, CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { course1, course2, course3 }).Using(CourseRelationshipAgnosticComparer));
        Assert.That(context.ChangeTracker.Entries<Course>(), Is.Empty);
    }
}
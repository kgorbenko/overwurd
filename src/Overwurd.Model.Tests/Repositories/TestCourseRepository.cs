using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;
using static Overwurd.Model.Tests.EqualityComparers.CourseComparers;

namespace Overwurd.Model.Tests.Repositories;

public class TestCourseRepository : BaseModelDatabaseDependentTestFixture
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
            var repository = new CourseRepository(context);

            var result = await repository.GetUserCoursesAsync(user.Id, CancellationToken.None);

            Assert.That(result, Is.Empty);
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
        var repository = new CourseRepository(context);

        var result = await repository.GetUserCoursesAsync(user.Id, CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { course1, course2, course3 }).Using(CourseRelationshipAgnosticComparer));
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
        var repository = new CourseRepository(context);

        var result = await repository.GetUserCoursesAsync(user1.Id, CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { course1 }).Using(CourseRelationshipAgnosticComparer));
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
        var repository = new CourseRepository(context);

        var result = await repository.GetUserCoursesAsync(user.Id, CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { course1, course2, course3 }).Using(CourseRelationshipAgnosticComparer));
    }

    [Test]
    public async Task TestPaginateUserCoursesAsync()
    {
        var user1 = new User { UserName = "Test User 1" };
        var course11 = new Course(name: "Course 11", description: "Description 11") { User = user1 };
        var course12 = new Course(name: "Course 12", description: "Description 12") { User = user1 };

        var user2 = new User { UserName = "Test User 2" };
        var course21 = new Course(name: "Course 21", description: "Description 21") { User = user2 };

        await StoreCoursesAsync(course11, course12, course21);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new CourseRepository(context);

        var actual = await repository.PaginateUserCoursesAsync(user1.Id, page: 1, pageSize: 2, CancellationToken.None);
        var expected = new PaginationResult<Course>(new[] { course12, course11 }.ToImmutableArray(), TotalCount: 2);
        Assert.That(actual, Is.EqualTo(expected).Using(PaginationResultComparer));
    }

    [Test]
    public async Task TestGetUserCourseByNameAsync()
    {
        var user = new User { UserName = "Test User" };
        var course1 = new Course("Course 1", "Description") { User = user };
        var course2 = new Course("Course 2", "Description") { User = user };

        var user2 = new User { UserName = "Test User 2" };
        var course3 = new Course("Course 2", "Description") { User = user2 };

        await StoreCoursesAsync(course1, course2, course3);

        await using var context = new ApplicationDbContext(ContextOptions);
        var repository = new CourseRepository(context);

        var nonExistentCourse = await repository.GetUserCourseByNameAsync(user.Id, "Non Existent Name", CancellationToken.None);
        Assert.That(nonExistentCourse, Is.Null);

        var single = await repository.GetUserCourseByNameAsync(user.Id, "Course 2", CancellationToken.None);
        Assert.That(single, Is.EqualTo(course2).Using(CourseRelationshipAgnosticComparer));
    }
}
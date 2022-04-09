namespace Overwurd.Domain.Tests.Entities;

public class TestUser
{
    [Test]
    public static void EqualityWithSameIds()
    {
        var user1 = UserTestHelper.CreateUser(id: 10);
        var user2 = UserTestHelper.CreateAnotherUser(id: 10);

        Assert.IsTrue(user1.Equals(user2));
        Assert.IsTrue(user1.Equals((object) user2));
    }

    [Test]
    public static void EqualityWithDifferentIds()
    {
        var user1 = UserTestHelper.CreateUser(id: 10);
        var user2 = UserTestHelper.CreateAnotherUser(id: 11);

        Assert.IsFalse(user1.Equals(user2));
        Assert.IsFalse(user1.Equals((object) user2));
    }

    [Test]
    public static void GetHashCodeSameIds()
    {
        var user1 = UserTestHelper.CreateUser(id: 123u);
        var user2 = UserTestHelper.CreateAnotherUser(id: 123u);

        Assert.AreEqual(user1.GetHashCode(), user2.GetHashCode());
    }

    [Test]
    public static void GetHashCodeDifferentIds()
    {
        var user1 = UserTestHelper.CreateUser(id: 123u);
        var user2 = UserTestHelper.CreateAnotherUser(id: 122u);

        Assert.AreNotEqual(user1.GetHashCode(), user2.GetHashCode());
    }

    [Test]
    public static void ConstructorLoginCannotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new User(
                id: 1u,
                login: null!,
                passwordHash: new UserPasswordHash("test-password-hash"),
                createdAt: DateTimeOffset.Now
            );
        });
    }

    [Test]
    public static void ConstructorPasswordHashCannotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new User(
                id: 1u,
                login: new UserLogin("test-user-login"),
                passwordHash: null!,
                createdAt: DateTimeOffset.Now
            );
        });
    }

    [Test]
    public static void PasswordHashCannotBeNull()
    {
        var user = UserTestHelper.CreateUser();

        Assert.Throws<ArgumentNullException>(
            () => user.SetPassword(null!)
        );
    }

    [Test]
    public static void TestConstructor()
    {
        var user = new User(
            id: 1u,
            login: new UserLogin("test-user-login"),
            passwordHash: new UserPasswordHash("test-password-hash"),
            createdAt: new DateTimeOffset(year: 2022, month: 01, day: 01, hour: 00, minute: 00, second: 00, TimeSpan.Zero)
        );

        Assert.AreEqual(user.Id, 1u);
        Assert.AreEqual(user.CreatedAt, new DateTimeOffset(year: 2022, month: 01, day: 01, hour: 00, minute: 00, second: 00, TimeSpan.Zero));
        Assert.AreEqual(user.Login, new UserLogin("test-user-login"));
        Assert.AreEqual(user.PasswordHash, new UserPasswordHash("test-password-hash"));
        Assert.IsEmpty(user.Courses);
    }

    [Test]
    public static void CannotAddNullCourse()
    {
        var user = UserTestHelper.CreateUser();

        Assert.Throws<ArgumentNullException>(
            () => user.AddCourse(null!)
        );
    }

    [Test]
    public static void CannotAddCourseFromAnotherUser()
    {
        var user1 = UserTestHelper.CreateUser();
        var user2 = UserTestHelper.CreateAnotherUser();
        var course = CourseTestHelper.CreateCourseForUser(user1);

        Assert.Throws<InvalidOperationException>(
            () => user2.AddCourse(course)
        );
    }

    [Test]
    public static void CannotAddCourseTwice()
    {
        var user = UserTestHelper.CreateUser();
        var course = CourseTestHelper.CreateCourseForUser(user);

        Assert.Throws<InvalidOperationException>(
            () => user.AddCourse(course)
        );
    }

    [Test]
    public static void AddCourse()
    {
        var user = UserTestHelper.CreateUser();

        var course = Course.CreateForUser(
            user: user,
            id: 1u,
            name: new EntityName("test-name"),
            description: new EntityDescription("test-description"),
            createdAt: DateTimeOffset.Now
        );

        Assert.That(user.Courses, Is.EqualTo(new[] { course }).Using(CourseComparer.Instance));
    }

    [Test]
    public static void CannotRemoveNullCourse()
    {
        var user = UserTestHelper.CreateUser();

        Assert.Throws<ArgumentNullException>(
            () => user.RemoveCourse(null!)
        );
    }

    [Test]
    public static void CannotRemoveCourseThatDoesNotBelong()
    {
        var user1 = UserTestHelper.CreateUser();
        var user2 = UserTestHelper.CreateAnotherUser();
        var course = CourseTestHelper.CreateCourseForUser(user1);

        Assert.Throws<InvalidOperationException>(
            () => user2.RemoveCourse(course)
        );
    }

    [Test]
    public static void RemoveCourse()
    {
        var user1 = UserTestHelper.CreateUser();
        var course = CourseTestHelper.CreateCourseForUser(user1);

        Assert.That(user1.Courses, Is.EqualTo(new[] { course }).Using(CourseComparer.Instance));

        user1.RemoveCourse(course);
        Assert.IsEmpty(user1.Courses);
    }
}
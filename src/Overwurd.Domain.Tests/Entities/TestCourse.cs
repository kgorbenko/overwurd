namespace Overwurd.Domain.Tests.Entities;

public static class TestCourse
{
    [Test]
    public static void EqualityWithSameIds()
    {
        var (user1, user2) = (UserTestHelper.CreateUser(), UserTestHelper.CreateAnotherUser());
        var course1 = CourseTestHelper.CreateCourseForUser(user1, id: 10u);
        var course2 = CourseTestHelper.CreateCourseForUser(user2, id: 10u);

        #pragma warning disable NUnit2010
        Assert.That(course1.Equals(course2), Is.True);
        Assert.That(course1.Equals((object) course2), Is.True);
        #pragma warning restore NUnit2010
    }

    [Test]
    public static void EqualityWithDifferentIds()
    {
        var (user1, user2) = (UserTestHelper.CreateUser(), UserTestHelper.CreateAnotherUser());
        var course1 = CourseTestHelper.CreateCourseForUser(user1, id: 10u);
        var course2 = CourseTestHelper.CreateCourseForUser(user2, id: 20u);

        #pragma warning disable NUnit2010
        Assert.That(course1.Equals(course2), Is.False);
        Assert.That(course1.Equals((object) course2), Is.False);
        #pragma warning restore NUnit2010
    }

    [Test]
    public static void GetHashCodeSameIds()
    {
        var (course1, _) = CourseTestHelper.CreateCourseWithUser(id: 123u);
        var (course2, _) = CourseTestHelper.CreateCourseWithUser(id: 123u);

        Assert.That(course2.GetHashCode(), Is.EqualTo(course1.GetHashCode()));
    }

    [Test]
    public static void GetHashCodeDifferentIds()
    {
        var (course1, _) = CourseTestHelper.CreateCourseWithUser(id: 100u);
        var (course2, _) = CourseTestHelper.CreateCourseWithUser(id: 200u);

        Assert.That(course2.GetHashCode(), Is.Not.EqualTo(course1.GetHashCode()));
    }

    [Test]
    public static void CreateForUserNameCannotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = Course.CreateForUser(
                user: UserTestHelper.CreateUser(),
                id: 1u,
                name: null!,
                description: new EntityDescription("description"),
                createdAt: DateTimeOffset.Now
            );
        });
    }

    [Test]
    public static void NameCannotBeNull()
    {
        var (course, _) = CourseTestHelper.CreateCourseWithUser();

        Assert.Throws<ArgumentNullException>(
            () => course.SetName(null!)
        );
    }

    [Test]
    public static void TestCreateForUser()
    {
        var user = UserTestHelper.CreateUser();
        var course = Course.CreateForUser(
            user: user,
            id: 1u,
            name: new EntityName("Test name"),
            description: new EntityDescription("Test description"),
            createdAt: new DateTimeOffset(year: 2022, month: 01, day: 01, hour: 00, minute: 00, second: 00, TimeSpan.Zero)
        );

        Assert.That(course.Id, Is.EqualTo(1u));
        Assert.That(course.UserId, Is.EqualTo(user.Id));
        Assert.That(course.CreatedAt, Is.EqualTo(new DateTimeOffset(year: 2022, month: 01, day: 01, hour: 00, minute: 00, second: 00, TimeSpan.Zero)));
        Assert.That(course.Name, Is.EqualTo(new EntityName("Test name")));
        Assert.That(course.Description, Is.EqualTo(new EntityDescription("Test description")));
        Assert.That(course.Vocabularies, Is.Empty);
    }

    [Test]
    public static void CannotAddNullVocabulary()
    {
        var (course, _) = CourseTestHelper.CreateCourseWithUser();

        Assert.Throws<ArgumentNullException>(
            () => course.AddVocabulary(null!)
        );
    }

    [Test]
    public static void CannotAddVocabularyFromOtherCourse()
    {
        var user = UserTestHelper.CreateUser();
        var course1 = CourseTestHelper.CreateCourseForUser(user);
        var course2 = CourseTestHelper.CreateAnotherCourseForUser(user);
        var vocabulary = VocabularyTestHelper.CreateVocabularyForCourse(course1);

        Assert.Throws<InvalidOperationException>(
            () => course2.AddVocabulary(vocabulary)
        );
    }

    [Test]
    public static void CannotAddVocabularyTwice()
    {
        var (vocabulary, course, _) = VocabularyTestHelper.CreateVocabularyWithCourseAndUser();

        Assert.Throws<InvalidOperationException>(
            () => course.AddVocabulary(vocabulary)
        );
    }

    [Test]
    public static void AddVocabulary()
    {
        var (course, _) = CourseTestHelper.CreateCourseWithUser();

        var vocabulary = Vocabulary.CreateForCourse(
            course: course,
            id: 1u,
            name: new EntityName("test"),
            description: null,
            createdAt: new DateTimeOffset(year: 2022, month: 01, day: 02, hour: 00, minute: 00, second: 00, TimeSpan.Zero)
        );

        Assert.That(course.Vocabularies, Is.EqualTo(new[] { vocabulary }).Using(VocabularyComparer.Instance));
    }

    [Test]
    public static void CannotRemoveNullVocabulary()
    {
        var (course, _) = CourseTestHelper.CreateCourseWithUser();

        Assert.Throws<ArgumentNullException>(
            () => course.RemoveVocabulary(null!)
        );
    }

    [Test]
    public static void CannotRemoveVocabularyThatDoesNotBelong()
    {
        var user = UserTestHelper.CreateUser();
        var course1 = CourseTestHelper.CreateCourseForUser(user);
        var course2 = CourseTestHelper.CreateAnotherCourseForUser(user);
        var vocabulary = VocabularyTestHelper.CreateVocabularyForCourse(course1);

        Assert.Throws<InvalidOperationException>(
            () => course2.RemoveVocabulary(vocabulary)
        );
    }

    [Test]
    public static void RemoveVocabulary()
    {
        var (course, _) = CourseTestHelper.CreateCourseWithUser();

        var vocabulary = Vocabulary.CreateForCourse(
            course: course,
            id: 1u,
            name: new EntityName("test"),
            description: null,
            createdAt: new DateTimeOffset(year: 2022, month: 01, day: 02, hour: 00, minute: 00, second: 00, TimeSpan.Zero)
        );

        Assert.That(course.Vocabularies, Is.EqualTo(new[] { vocabulary }).Using(VocabularyComparer.Instance));

        course.RemoveVocabulary(vocabulary);
        Assert.That(course.Vocabularies, Is.Empty);
    }
}
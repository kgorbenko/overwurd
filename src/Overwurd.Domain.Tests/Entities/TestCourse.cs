namespace Overwurd.Domain.Tests.Entities;

public static class TestCourse
{
    [Test]
    public static void EqualityWithSameIds()
    {
        var (user1, user2) = (UserTestHelper.CreateUser(), UserTestHelper.CreateAnotherUser());
        var course1 = CourseTestHelper.CreateCourseForUser(user1, id: 10u);
        var course2 = CourseTestHelper.CreateCourseForUser(user2, id: 10u);

        Assert.IsTrue(course1.Equals(course2));
        Assert.IsTrue(course1.Equals((object) course2));
    }

    [Test]
    public static void EqualityWithDifferentIds()
    {
        var (user1, user2) = (UserTestHelper.CreateUser(), UserTestHelper.CreateAnotherUser());
        var course1 = CourseTestHelper.CreateCourseForUser(user1, id: 10u);
        var course2 = CourseTestHelper.CreateCourseForUser(user2, id: 20u);

        Assert.IsFalse(course1.Equals(course2));
        Assert.IsFalse(course1.Equals((object) course2));
    }

    [Test]
    public static void GetHashCodeSameIds()
    {
        var (course1, _) = CourseTestHelper.CreateCourseWithUser(id: 123u);
        var (course2, _) = CourseTestHelper.CreateCourseWithUser(id: 123u);

        Assert.AreEqual(course1.GetHashCode(), course2.GetHashCode());
    }

    [Test]
    public static void GetHashCodeDifferentIds()
    {
        var (course1, _) = CourseTestHelper.CreateCourseWithUser(id: 100u);
        var (course2, _) = CourseTestHelper.CreateCourseWithUser(id: 200u);

        Assert.AreNotEqual(course1.GetHashCode(), course2.GetHashCode());
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

        Assert.AreEqual(course.Id, 1u);
        Assert.AreEqual(course.UserId, user.Id);
        Assert.AreEqual(course.CreatedAt, new DateTimeOffset(year: 2022, month: 01, day: 01, hour: 00, minute: 00, second: 00, TimeSpan.Zero));
        Assert.AreEqual(course.Name, new EntityName("Test name"));
        Assert.AreEqual(course.Description, new EntityDescription("Test description"));
        Assert.IsEmpty(course.Vocabularies);
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
        Assert.IsEmpty(course.Vocabularies);
    }
}
using System;
using NUnit.Framework;
using Overwurd.Domain.Entities;
using Overwurd.Domain.Tests.Helpers;

namespace Overwurd.Domain.Tests.Entities;

public static class TestVocabulary
{
    [Test]
    public static void EqualityWithSameIds()
    {
        var (course1, _) = CourseTestHelper.CreateCourseWithUser();
        var (course2, _) = CourseTestHelper.CreateAnotherCourseWithUser();

        var vocabulary1 = VocabularyTestHelper.CreateVocabularyForCourse(course1, id: 10u);
        var vocabulary2 = VocabularyTestHelper.CreateVocabularyForCourse(course2, id: 10u);

        Assert.IsTrue(vocabulary1.Equals(vocabulary2));
        Assert.IsTrue(vocabulary1.Equals((object) vocabulary2));
    }

    [Test]
    public static void EqualityWithDifferentIds()
    {
        var (course1, _) = CourseTestHelper.CreateCourseWithUser();
        var (course2, _) = CourseTestHelper.CreateAnotherCourseWithUser();

        var vocabulary1 = VocabularyTestHelper.CreateVocabularyForCourse(course1, id: 10u);
        var vocabulary2 = VocabularyTestHelper.CreateVocabularyForCourse(course2, id: 20u);

        Assert.IsFalse(vocabulary1.Equals(vocabulary2));
        Assert.IsFalse(vocabulary1.Equals((object) vocabulary2));
    }

    [Test]
    public static void GetHashCodeSameIds()
    {
        var (vocabulary1, _, _) = VocabularyTestHelper.CreateVocabularyWithCourseAndUser(id: 123u);
        var (vocabulary2, _, _) = VocabularyTestHelper.CreateVocabularyWithCourseAndUser(id: 123u);

        Assert.AreEqual(vocabulary1.GetHashCode(), vocabulary2.GetHashCode());
    }

    [Test]
    public static void GetHashCodeDifferentIds()
    {
        var (vocabulary1, _, _) = VocabularyTestHelper.CreateVocabularyWithCourseAndUser(id: 100u);
        var (vocabulary2, _, _) = VocabularyTestHelper.CreateVocabularyWithCourseAndUser(id: 200u);

        Assert.AreNotEqual(vocabulary1.GetHashCode(), vocabulary2.GetHashCode());
    }

    [Test]
    public static void CreateForCourseNameCannotBeNull()
    {
        var (course, _) = CourseTestHelper.CreateCourseWithUser();
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = Vocabulary.CreateForCourse(
                course: course,
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
        var (vocabulary, _, _) = VocabularyTestHelper.CreateVocabularyWithCourseAndUser();

        Assert.Throws<ArgumentNullException>(
            () => vocabulary.SetName(null!)
        );
    }

    [Test]
    public static void TestCreateForCourse()
    {
        var (course, _) = CourseTestHelper.CreateCourseWithUser();
        var vocabulary = Vocabulary.CreateForCourse(
            course: course,
            id: 2u,
            name: new EntityName("Test name"),
            description: new EntityDescription("Test description"),
            createdAt: new DateTimeOffset(year: 2022, month: 01, day: 01, hour: 00, minute: 00, second: 00, TimeSpan.Zero)
        );

        Assert.AreEqual(vocabulary.Id, 2u);
        Assert.AreEqual(vocabulary.CourseId, course.Id);
        Assert.AreEqual(vocabulary.CreatedAt, new DateTimeOffset(year: 2022, month: 01, day: 01, hour: 00, minute: 00, second: 00, TimeSpan.Zero));
        Assert.AreEqual(vocabulary.Name, new EntityName("Test name"));
        Assert.AreEqual(vocabulary.Description, new EntityDescription("Test description"));
    }
}
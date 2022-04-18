namespace Overwurd.Domain.Tests.Helpers;

public static class VocabularyTestHelper
{
    public static Vocabulary CreateVocabularyForCourse(Course course, uint id = 19001, string name = "Vocabulary19001", string description = "Description19001") =>
        Vocabulary.CreateForCourse(
            course: course,
            id: id,
            name: new EntityName(name),
            description: new EntityDescription(description),
            createdAt: new DateTimeOffset(year: 2020, month: 1, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero)
        );

    public static Vocabulary CreateAnotherVocabularyForCourse(Course course, uint id = 19002, string name = "Vocabulary19002", string description = "Description19002") =>
        Vocabulary.CreateForCourse(
            course: course,
            id: id,
            name: new EntityName(name),
            description: new EntityDescription(description),
            createdAt: new DateTimeOffset(year: 2020, month: 1, day: 2, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero)
        );

    public static (Vocabulary Vocabulary, Course Course, User User) CreateVocabularyWithCourseAndUser(uint id = 19003, string name = "Vocabulary19003", string description = "Description19003")
    {
        var (course, user) = CourseTestHelper.CreateCourseWithUser();
        var vocabulary = Vocabulary.CreateForCourse(
            course: course,
            id: id,
            name: new EntityName(name),
            description: new EntityDescription(description),
            createdAt: new DateTimeOffset(year: 2020, month: 1, day: 3, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero)
        );

        return (vocabulary, course, user);
    }
}
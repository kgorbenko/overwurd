namespace Overwurd.Domain.Tests.Helpers;

public static class CourseTestHelper
{
    public static Course CreateCourseForUser(User user, uint id = 15001, string name = "Course15001", string description = "Description15001") =>
        Course.CreateForUser(
            user: user,
            id: id,
            name: new EntityName(name),
            description: new EntityDescription(description),
            createdAt: new DateTimeOffset(year: 2020, month: 1, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero)
        );

    public static Course CreateAnotherCourseForUser(User user, uint id = 15002, string name = "Course15002", string description = "Description15002") =>
        Course.CreateForUser(
            user: user,
            id: id,
            name: new EntityName(name),
            description: new EntityDescription(description),
            createdAt: new DateTimeOffset(year: 2020, month: 1, day: 2, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero)
        );

    public static (Course Course, User User) CreateCourseWithUser(uint id = 15003, string name = "Course15003", string description = "Description15003")
    {
        var user = UserTestHelper.CreateUser();
        var course = Course.CreateForUser(
            user: user,
            id: id,
            name: new EntityName(name),
            description: new EntityDescription(description),
            createdAt: new DateTimeOffset(year: 2020, month: 1, day: 3, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero)
        );

        return (course, user);
    }

    public static (Course Course, User User) CreateAnotherCourseWithUser(uint id = 15004, string name = "Course15004", string description = "Description15004")
    {
        var user = UserTestHelper.CreateAnotherUser();
        var course = Course.CreateForUser(
            user: user,
            id: id,
            name: new EntityName(name),
            description: new EntityDescription(description),
            createdAt: new DateTimeOffset(year: 2020, month: 1, day: 4, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero)
        );

        return (course, user);
    }
}
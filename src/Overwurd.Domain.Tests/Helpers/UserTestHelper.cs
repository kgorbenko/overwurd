using System;
using Overwurd.Domain.Entities;

namespace Overwurd.Domain.Tests.Helpers;

public static class UserTestHelper
{
    public static User CreateUser(uint id = 17001, string login = "User17001") =>
        new(
            id: id,
            login: new UserLogin(login),
            passwordHash: new UserPasswordHash("test-password-17001"),
            createdAt: new DateTimeOffset(year: 2020, month: 1, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero)
        );

    public static User CreateAnotherUser(uint id = 17002, string login = "User17001") =>
        new(
            id: id,
            login: new UserLogin(login),
            passwordHash: new UserPasswordHash("test-password-17001"),
            createdAt: new DateTimeOffset(year: 2020, month: 1, day: 2, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero)
        );
}
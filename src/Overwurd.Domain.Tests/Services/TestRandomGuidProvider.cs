namespace Overwurd.Domain.Tests.Services;

public static class TestRandomGuidProvider
{
    [Test]
    public static void GeneratesNonEmptyGuid()
    {
        var guidProvider = new RandomGuidGenerator();
        var guid = guidProvider.Generate();

        Assert.AreNotEqual(guid, Guid.Empty);
    }

    [Test]
    public static void GeneratesNonEqualGuids()
    {
        var generator = new RandomGuidGenerator();
        var guid1 = generator.Generate();
        var guid2 = generator.Generate();

        Assert.AreNotEqual(guid1, guid2);
    }
}
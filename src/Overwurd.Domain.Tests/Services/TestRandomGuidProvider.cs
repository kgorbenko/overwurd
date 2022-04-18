namespace Overwurd.Domain.Tests.Services;

public static class TestRandomGuidProvider
{
    [Test]
    public static void GeneratesNonEmptyGuid()
    {
        var guidProvider = new RandomGuidGenerator();
        var guid = guidProvider.Generate();

        Assert.That(Guid.Empty, Is.Not.EqualTo(guid));
    }

    [Test]
    public static void GeneratesNonEqualGuids()
    {
        var generator = new RandomGuidGenerator();
        var guid1 = generator.Generate();
        var guid2 = generator.Generate();

        Assert.That(guid2, Is.Not.EqualTo(guid1));
    }
}
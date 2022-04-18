namespace Overwurd.Domain.Tests.Services.Authentication;

public static class TestParseUserIdHelper
{
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("a")]
    [TestCase("1.1")]
    [TestCase("1,1")]
    public static void ParseNegative(string value)
    {
        Assert.Throws<InvalidOperationException>(
            () => ParseUserIdHelper.Parse(value)
        );
    }

    [Test]
    [TestCase("1", 1)]
    [TestCase("100500", 100500)]
    public static void ParsePositive(string value, int expected)
    {
        var result = ParseUserIdHelper.Parse(value);

        Assert.That(expected, Is.EqualTo(result));
    }
}
using System;
using NUnit.Framework;
using Overwurd.Domain.Entities;

namespace Overwurd.Domain.Tests.Entities;

public static class TestEntityWithIdComparer
{
    private class TestEntityWithId1 : IEntityWithId
    {
        public TestEntityWithId1(uint id)
        {
            Id = id;
        }

        public uint Id { get; }
    }

    private class TestEntityWithId2 : IEntityWithId
    {
        public TestEntityWithId2(uint id)
        {
            Id = id;
        }

        public uint Id { get; }
    }

    private static TestCaseData[] GenerateEqualsTestCases()
    {
        var instance = new TestEntityWithId1(1u);

        return new TestCaseData[]
        {
            new(null, null, false),
            new(new TestEntityWithId1(1u), null, false),
            new(null, new TestEntityWithId1(1u), false),
            new(instance, instance, true),
            new(new TestEntityWithId1(1u), new TestEntityWithId2(1u), false),
            new(new TestEntityWithId1(1u), new TestEntityWithId2(2u), false),
            new(new TestEntityWithId1(1u), new TestEntityWithId1(2u), false),
            new(new TestEntityWithId1(1u), new TestEntityWithId1(1u), true),
            new(new TestEntityWithId1(10u), new TestEntityWithId1(10u), true)
        };
    }

    [TestCaseSource(nameof(GenerateEqualsTestCases))]
    public static void Equals(IEntityWithId first, IEntityWithId second, bool expected)
    {
        var comparer = new EntityWithIdComparer();

        var actual = comparer.Equals(first, second);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public static void GetHashCodeNullThrowsArgumentNullException()
    {
        var comparer = new EntityWithIdComparer();

        Assert.Throws<ArgumentNullException>(() => _ = comparer.GetHashCode(null!));
    }

    [TestCase(1u, 1u, true)]
    [TestCase(10u, 10u, true)]
    [TestCase(100500u, 100500u, true)]
    [TestCase(1523512352u, 1523512352u, true)]
    [TestCase(1u, 2u, false)]
    [TestCase(123u, 124u, false)]
    public static void GetHashCode(uint firstId, uint secondId, bool expected)
    {
        var comparer = new EntityWithIdComparer();

        var entity1 = new TestEntityWithId1(firstId);
        var entity2 = new TestEntityWithId2(secondId);

        var actual = comparer.GetHashCode(entity1) == comparer.GetHashCode(entity2);

        Assert.That(actual, Is.EqualTo(expected));
    }
}
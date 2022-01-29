using System;
using NUnit.Framework;
using Overwurd.Model.Models;

namespace Overwurd.Model.Tests.Models;

public static class TestCourse
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public static void TestNamesSetterInvalidValues(string invalidName)
    {
        void Action(string name)
        {
            var course = new Course(name: "Test name", description: "Test description");
            course.Name = name;
        }

        Assert.Throws<ArgumentException>(() => Action(invalidName));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public static void TestConstructorInvalidValuesForName(string invalidName)
    {
        void Action(string name) =>
            new Course(name: name, description: "Test description");

        Assert.Throws<ArgumentException>(() => Action(invalidName));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public static void TestDescriptionSetterWithInvalidValues(string invalidDescription)
    {
        void Action(string description)
        {
            var course = new Course("Test name", "Test description");
            course.Description = description;
        }

        Assert.Throws<ArgumentException>(() => Action(invalidDescription));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public static void TestConstructorInvalidValuesForDescription(string invalidDescription)
    {
        void Action(string description) =>
            new Course(name: "Test name", description: description);

        Assert.Throws<ArgumentException>(() => Action(invalidDescription));
    }
}
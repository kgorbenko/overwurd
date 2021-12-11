using System;
using NUnit.Framework;
using Overwurd.Model.Models;

namespace Overwurd.Model.Tests.Models
{
    public static class TestVocabulary {
        // TODO: Use C# 10 constant string interpolation
        private const string CannotSetNameToEmptyStringMessage =
            "An attempt to set Vocabulary name to an invalid value. Vocabulary name cannot be null, empty or whitespace, but was empty.";

        private const string CannotSetNameToWhitespaceMessage =
            "An attempt to set Vocabulary name to an invalid value. Vocabulary name cannot be null, empty or whitespace, but was whitespace.";

        private const string CannotSetNameToNullMessage =
            "An attempt to set Vocabulary name to an invalid value. Vocabulary name cannot be null, empty or whitespace, but was null.";

        [Test]
        public static void SettingEmptyStringToNameThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => new Vocabulary("Test").Name = "");
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo(CannotSetNameToEmptyStringMessage));
        }

        [Test]
        public static void SettingWhitespaceToNameThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => new Vocabulary("Test").Name = " ");
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo(CannotSetNameToWhitespaceMessage));
        }

        [Test]
        public static void SettingNullToNameThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => new Vocabulary("Test").Name = null);
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo(CannotSetNameToNullMessage));
        }

        [Test]
        public static void PassingEmptyStringNameToConstructorThrowsArgumentException() {
            var exception = Assert.Throws<ArgumentException>(() => { var vocabulary = new Vocabulary(""); });
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo(CannotSetNameToEmptyStringMessage));
        }

        [Test]
        public static void PassingWhitespaceNameToConstructorThrowsArgumentException() {
            var exception = Assert.Throws<ArgumentException>(() => { var vocabulary = new Vocabulary(" "); });
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo(CannotSetNameToWhitespaceMessage));
        }

        [Test]
        public static void PassingNullNameToConstructorThrowsArgumentException() {
            var exception = Assert.Throws<ArgumentException>(() => { var vocabulary = new Vocabulary(name: null); });
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo(CannotSetNameToNullMessage));
        }
    }
}
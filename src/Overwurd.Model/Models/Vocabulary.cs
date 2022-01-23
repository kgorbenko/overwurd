using System;
using JetBrains.Annotations;
using Overwurd.Model.Helpers;

namespace Overwurd.Model.Models
{
    public class Vocabulary : IEntity
    {
        public int Id { get; [UsedImplicitly] private set; }

        public DateTimeOffset CreatedAt { get; [UsedImplicitly] private set; }

        private Course course;

        public Course Course
        {
            get => course;
            set => course = value ?? throw new ArgumentNullException(nameof(value));
        }

        private string name;

        public string Name
        {
            get => name;
            set
            {
                string MakeMessage(string invalidValue) =>
                    $"An attempt to set {nameof(Vocabulary)} name to an invalid value. " +
                    $"{nameof(Vocabulary)} name cannot be null, empty or whitespace, but was {invalidValue}.";

                name = value switch
                {
                    null => throw new ArgumentException(MakeMessage("null")),
                    var s when string.IsNullOrEmpty(s) => throw new ArgumentException(MakeMessage("empty")),
                    var s when string.IsNullOrWhiteSpace(s) => throw new ArgumentException(MakeMessage("whitespace")),
                    var s => s
                };
            }
        }

        private string description;

        public string Description
        {
            get => description;
            set
            {
                string MakeMessage(string invalidValue) =>
                    $"An attempt to set {nameof(Vocabulary)} description to an invalid value. " +
                    $"{nameof(Vocabulary)} description cannot be null, empty or whitespace, but was {invalidValue}.";

                description = value switch
                {
                    null => throw new ArgumentException(MakeMessage("null")),
                    var s when string.IsNullOrEmpty(s) => throw new ArgumentException(MakeMessage("empty")),
                    var s when string.IsNullOrWhiteSpace(s) => throw new ArgumentException(MakeMessage("whitespace")),
                    var s => s
                };
            }
        }

        public Vocabulary(string name, string description)
        {
            Name = name;
            Description = description;
            CreatedAt = DateTimeOffsetHelper.NowUtcSeconds();
        }
    }
}
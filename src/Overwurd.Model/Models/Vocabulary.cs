using System;
using JetBrains.Annotations;

namespace Overwurd.Model.Models
{
    public class Vocabulary : IEntity
    {
        public long Id { get; [UsedImplicitly] private set; }

        public DateTimeOffset CreatedAt { get; [UsedImplicitly] private set; }

        private string name;
        public string Name {
            get => name;
            set {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("An attempt to set Vocabulary name to an invalid value. " +
                                                $"Vocabulary name cannot be null, empty or whitespace, but was '{value}'.");
                }

                name = value;
            }
        }

        public Vocabulary(string name)
        {
            Name = name;
        }
    }
}
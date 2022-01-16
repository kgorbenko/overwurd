using System;
using JetBrains.Annotations;
using Overwurd.Model.Helpers;

namespace Overwurd.Model.Models {
    public class Vocabulary : IEntity {
        public int Id { get; [UsedImplicitly] private set; }

        public DateTimeOffset CreatedAt { get; [UsedImplicitly] private set; }

        private string name;

        public string Name {
            get => name;
            set {
                string MakeMessage(string invalidValue) =>
                    "An attempt to set Vocabulary name to an invalid value. " +
                    $"Vocabulary name cannot be null, empty or whitespace, but was {invalidValue}.";

                name = value switch {
                    null => throw new ArgumentException(MakeMessage("null")),
                    var s when string.IsNullOrEmpty(s) => throw new ArgumentException(MakeMessage("empty")),
                    var s when string.IsNullOrWhiteSpace(s) => throw new ArgumentException(MakeMessage("whitespace")),
                    var s => s
                };
            }
        }

        public Vocabulary(string name) {
            Name = name;
            CreatedAt = DateTimeOffset.Now.TrimSeconds();
        }
    }
}
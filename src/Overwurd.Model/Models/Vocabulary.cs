using System;
using JetBrains.Annotations;

namespace Overwurd.Model.Models {
    public class Vocabulary : IEntity {
        public long Id { get; [UsedImplicitly] private set; }

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

            var now = DateTimeOffset.Now;
            CreatedAt = new DateTimeOffset(year: now.Year, month: now.Month, day: now.Day, hour: now.Hour, minute: now.Minute, second: now.Second, offset: now.Offset);
        }
    }
}
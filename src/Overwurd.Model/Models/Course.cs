using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Overwurd.Model.Helpers;

namespace Overwurd.Model.Models;

public class Course : IEntity
{
    public int Id { get; [UsedImplicitly] private set; }

    public DateTimeOffset CreatedAt { get; [UsedImplicitly] private set; }

    private User user;

    public User User
    {
        get => user;
        set => user = value ?? throw new ArgumentNullException(nameof(value));
    }

    private string name;

    public string Name
    {
        get => name;
        set
        {
            string MakeMessage(string invalidValue) =>
                $"An attempt to set {nameof(Course)} name to an invalid value. " +
                $"{nameof(Course)} name cannot be null, empty or whitespace, but was {invalidValue}.";

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
                $"An attempt to set {nameof(Course)} description to an invalid value. " +
                $"{nameof(Course)} description cannot be null, empty or whitespace, but was {invalidValue}.";

            description = value switch
            {
                null => throw new ArgumentException(MakeMessage("null")),
                var s when string.IsNullOrEmpty(s) => throw new ArgumentException(MakeMessage("empty")),
                var s when string.IsNullOrWhiteSpace(s) => throw new ArgumentException(MakeMessage("whitespace")),
                var s => s
            };
        }
    }

    [UsedImplicitly]
    public ICollection<Vocabulary> Vocabularies { get; private set; }

    public Course(string name, string description)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTimeOffsetHelper.NowUtcSeconds();
    }
}
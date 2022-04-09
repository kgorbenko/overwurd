namespace Overwurd.Domain.Tests.Entities;

public static class TestEntityDescription
{
    private const int MaxLength = 255;

    private static readonly string emptyMessage = "Description cannot be null.";

    private static readonly string maxLengthMessage = $"Description cannot be longer than {MaxLength} characters.";

    [TestCaseSource(nameof(GeneratePositiveValidationTestCases))]
    [TestCaseSource(nameof(GenerateNegativeValidationTestCases))]
    public static void Validate(string? value, ValidationResult expected)
    {
        var actual = EntityDescription.Validate(value);

        Assert.That(actual, Is.EqualTo(expected).Using(ValidationResultComparer.Instance));
    }

    [TestCaseSource(nameof(GenerateNegativeValidationTestCases))]
    public static void ConstructorThrowsValidationException(string? value, ValidationResult validationResult)
    {
        Assert.Throws<ValidationException>(
            () => _ = new EntityDescription(value!)
        );
    }

    [TestCaseSource(nameof(GeneratePositiveValidationTestCases))]
    public static void ConstructorPositiveCases(string value, ValidationResult validationResult)
    {
        var login = new EntityDescription(value);

        Assert.That(login.Value, Is.EqualTo(value));
    }

    private static TestCaseData[] GenerateNegativeValidationTestCases() => new[]
    {
        new TestCaseData(
            null,
            new ValidationResult(
                IsValid: false,
                Errors: new[] { emptyMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { emptyMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            " ",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { emptyMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "                                                                                                                                                                                                                                                                ",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { emptyMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long .",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { maxLengthMessage }.ToImmutableArray()
            )
        )
    };

    private static TestCaseData[] GeneratePositiveValidationTestCases() => new[]
    {
        new TestCaseData(
            "1",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        ),
        new TestCaseData(
            "ABCabc123 ,.<>/|\\'\":;[]{}()!?@#$%^&*-_+=~`",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        ),
        new TestCaseData(
            "Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long name.Some test long ",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        )
    };
}
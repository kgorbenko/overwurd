namespace Overwurd.Domain.Tests.Entities;

public static class TestEntityName
{
    private const int MaxLength = 60;

    private static readonly string maxLengthMessage = $"Name cannot be longer than {MaxLength} characters.";

    private static readonly string emptyMessage = "Name cannot be empty.";

    [TestCaseSource(nameof(GeneratePositiveValidationTestCases))]
    [TestCaseSource(nameof(GenerateNegativeValidationTestCases))]
    public static void Validate(string? value, ValidationResult expected)
    {
        var actual = EntityName.Validate(value);

        Assert.That(actual, Is.EqualTo(expected).Using(ValidationResultComparer.Instance));
    }

    [TestCaseSource(nameof(GenerateNegativeValidationTestCases))]
    public static void ConstructorThrowsValidationException(string? value, ValidationResult validationResult)
    {
        Assert.Throws<ValidationException>(
            () => _ = new EntityName(value!)
        );
    }

    [TestCaseSource(nameof(GeneratePositiveValidationTestCases))]
    public static void ConstructorPositiveCases(string value, ValidationResult validationResult)
    {
        var login = new EntityName(value);

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
            "                                                             ",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { emptyMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Some test long name.Some test long name.Some test long name. ",
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
            "Some test long name.Some test long name.Some test long name.",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        )
    };
}
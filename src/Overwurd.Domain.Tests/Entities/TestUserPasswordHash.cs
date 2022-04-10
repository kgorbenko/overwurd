namespace Overwurd.Domain.Tests.Entities;

public static class TestUserPasswordHash
{
    private const string EmptyMessage = "Password cannot be empty.";

    [TestCaseSource(nameof(GeneratePositiveValidationTestCases))]
    [TestCaseSource(nameof(GenerateNegativeValidationTestCases))]
    public static void Validate(string? value, ValidationResult expected)
    {
        var actual = UserPasswordHash.Validate(value);

        Assert.That(actual, Is.EqualTo(expected).Using(ValidationResultComparer.Instance));
    }

    [TestCaseSource(nameof(GenerateNegativeValidationTestCases))]
    public static void ConstructorThrowsValidationException(string? value, ValidationResult validationResult)
    {
        Assert.Throws<Domain.Entities.Validation.ValidationException>(
            () => _ = new UserPasswordHash(value!)
        );
    }

    [TestCaseSource(nameof(GeneratePositiveValidationTestCases))]
    public static void ConstructorPositiveCases(string value, ValidationResult validationResult)
    {
        var passwordHash = new UserPasswordHash(value);

        Assert.That(passwordHash.Value, Is.EqualTo(value));
    }

    private static TestCaseData[] GeneratePositiveValidationTestCases() => new[]
    {
        new TestCaseData(
            "EightChr",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        ),
        new TestCaseData(
            "0123456789",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        ),
        new TestCaseData(
            "Some different chars ,.<>/|\\'\":;[]{}()!?@#$%^&*-_+=~`",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        ),
        new TestCaseData(
            "Some very long value. Some very long value. Some very long value. Some very long value. Some very long value. Some very long value. Some very long value.",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        ),
    };

    private static TestCaseData[] GenerateNegativeValidationTestCases() => new[]
    {
        new TestCaseData(
            null,
            new ValidationResult(
                IsValid: false,
                Errors: new[] { EmptyMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { EmptyMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            " ",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { EmptyMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "            ",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { EmptyMessage }.ToImmutableArray()
            )
        )
    };
}
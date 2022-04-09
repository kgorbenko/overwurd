using System.Collections.Immutable;
using NUnit.Framework;
using Overwurd.Domain.Entities;
using Overwurd.Domain.Entities.Validation;
using Overwurd.Domain.Tests.Comparers;

namespace Overwurd.Domain.Tests.Entities;

public static class TestUserLogin
{
    private const int MinLength = 8;

    private const int MaxLength = 30;

    private static readonly string emptyMessage = "Login cannot be empty.";

    private static readonly string minLengthMessage = $"Login cannot be shorter than {MinLength} characters.";

    private static readonly string maxLengthMessage = $"Login cannot be longer than {MaxLength} characters.";

    private static readonly string invalidCharactersMessage = "Valid characters are lowercase and uppercase Latin letters, digits, '-', '.' and '_'.";

    [TestCaseSource(nameof(GeneratePositiveValidationTestCases))]
    [TestCaseSource(nameof(GenerateNegativeValidationTestCases))]
    public static void Validate(string? value, ValidationResult expected)
    {
        var actual = UserLogin.Validate(value);

        Assert.That(actual, Is.EqualTo(expected).Using(ValidationResultComparer.Instance));
    }

    [TestCaseSource(nameof(GenerateNegativeValidationTestCases))]
    public static void ConstructorThrowsValidationException(string? value, ValidationResult validationResult)
    {
        Assert.Throws<ValidationException>(
            () => _ = new UserLogin(value!)
        );
    }

    [TestCaseSource(nameof(GeneratePositiveValidationTestCases))]
    public static void ConstructorPositiveCases(string value, ValidationResult validationResult)
    {
        var login = new UserLogin(value);

        Assert.That(login.Value, Is.EqualTo(value));
    }

    private static TestCaseData[] GeneratePositiveValidationTestCases() => new[]
    {
        new TestCaseData(
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        ),
        new TestCaseData(
            "abcdefghijklmnopqrstuvwxyz",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        ),
        new TestCaseData(
            "AllowedNumbers0123456789",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        ),
        new TestCaseData(
            "AllowedChars.-_",
            new ValidationResult(
                IsValid: true,
                Errors: ImmutableArray<string>.Empty
            )
        )
    };

    private static TestCaseData[] GenerateNegativeValidationTestCases() => new[]
    {
        new TestCaseData(
            null,
            new ValidationResult(
                IsValid: false,
                new[] { emptyMessage }.ToImmutableArray()
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
            "                      ",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { emptyMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "gerrard",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { minLengthMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "XXXSomeTestVeryLongUserNameXXX1",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { maxLengthMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "whitespace is not permitted",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "DisallowedParentheses(",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "DisallowedParentheses)",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "DisallowedBrackets[",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "DisallowedBrackets]",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "DisallowedBraces{",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "DisallowedBraces}",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "DisallowedBraces}",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed!",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed?",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed,",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed/",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed\\",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed|",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed@",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed#",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed$",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed^",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed%",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed&",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed*",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed+",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed=",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        ),
        new TestCaseData(
            "Disallowed~",
            new ValidationResult(
                IsValid: false,
                Errors: new[] { invalidCharactersMessage }.ToImmutableArray()
            )
        )
    };
}
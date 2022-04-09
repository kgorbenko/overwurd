using System.Collections.Immutable;
using FluentValidation;
using Overwurd.Domain.Entities.Validation;

namespace Overwurd.Domain.Entities;

public record UserLogin
{
    public const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";

    public string Value { get; }

    public UserLogin(string login)
    {
        var validationModel = new UserLoginValidationModel(login);
        ValidationHelper.EnsureValid(validationModel, SetRules);

        Value = login;
    }

    public static ValidationResult Validate(string? login)
    {
        var validationModel = new UserLoginValidationModel(login);
        return ValidationHelper.Validate(validationModel, SetRules);
    }

    private record UserLoginValidationModel(string? Value);

    private static void SetRules(AbstractValidator<UserLoginValidationModel> validator)
    {
        const int minLength = 8;
        const int maxLength = 30;

        var allowedCharacters = AllowedCharacters.ToImmutableHashSet();

        validator.RuleFor(x => x.Value)
                 .NotEmpty()
                 .WithMessage("Login cannot be empty.");

        validator.RuleFor(x => x.Value)
                 .MinimumLength(minLength)
                 .When(x => !string.IsNullOrWhiteSpace(x.Value))
                 .WithMessage($"Login cannot be shorter than {minLength} characters.");

        validator.RuleFor(x => x.Value)
                 .MaximumLength(maxLength)
                 .When(x => !string.IsNullOrWhiteSpace(x.Value))
                 .WithMessage($"Login cannot be longer than {maxLength} characters.");

        validator.RuleFor(x => x.Value)
                 .Must(login => login!.All(character => allowedCharacters.Contains(character)))
                 .When(x => !string.IsNullOrWhiteSpace(x.Value))
                 .WithMessage("Valid characters are lowercase and uppercase Latin letters, digits, '-', '.' and '_'.");
    }
}
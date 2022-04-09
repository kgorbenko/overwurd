using FluentValidation;
using Overwurd.Domain.Entities.Validation;

namespace Overwurd.Domain.Entities;

public record EntityName
{
    public string Value { get; }

    public EntityName(string name)
    {
        var validationModel = new EntityNameValidationModel(name);
        ValidationHelper.EnsureValid(validationModel, SetRules);

        Value = name;
    }

    public static ValidationResult Validate(string? name)
    {
        var validationModel = new EntityNameValidationModel(name);
        return ValidationHelper.Validate(validationModel, SetRules);
    }

    private record EntityNameValidationModel(string? Value);

    private static void SetRules(AbstractValidator<EntityNameValidationModel> validator)
    {
        const int maxLength = 60;

        validator.RuleFor(x => x.Value)
                 .NotEmpty()
                 .WithMessage("Name cannot be empty.");

        validator.RuleFor(x => x.Value)
                 .MaximumLength(maxLength)
                 .When(x => !string.IsNullOrWhiteSpace(x.Value))
                 .WithMessage($"Name cannot be longer than {maxLength} characters.");
    }
}
namespace Overwurd.Domain.Entities;

public record EntityDescription
{
    public string Value { get; }

    public EntityDescription(string description)
    {
        var validationModel = new EntityDescriptionValidationModel(description);
        ValidationHelper.EnsureValid(validationModel, SetRules);

        Value = description;
    }

    public static ValidationResult Validate(string? description)
    {
        var validationModel = new EntityDescriptionValidationModel(description);
        return ValidationHelper.Validate(validationModel, SetRules);
    }

    private record EntityDescriptionValidationModel(string? Value);

    private static void SetRules(InlineValidator<EntityDescriptionValidationModel> validator)
    {
        const int maxLength = 255;

        validator.RuleFor(x => x.Value)
                 .NotEmpty()
                 .WithMessage("Description cannot be null.");

        validator.RuleFor(x => x.Value)
                 .MaximumLength(maxLength)
                 .When(x => !string.IsNullOrWhiteSpace(x.Value))
                 .WithMessage($"Description cannot be longer than {maxLength} characters.");
    }
}
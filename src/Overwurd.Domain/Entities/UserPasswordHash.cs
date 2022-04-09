namespace Overwurd.Domain.Entities;

public record UserPasswordHash
{
    public string Value { get; }

    public UserPasswordHash(string passwordHashValue)
    {
        var validationModel = new UserPasswordValidationModel(passwordHashValue);
        ValidationHelper.EnsureValid(validationModel, SetRules);

        Value = passwordHashValue;
    }

    public static ValidationResult Validate(string? passwordHashValue)
    {
        var validationModel = new UserPasswordValidationModel(passwordHashValue);
        return ValidationHelper.Validate(validationModel, SetRules);
    }

    private record UserPasswordValidationModel(string? Value);

    private static void SetRules(AbstractValidator<UserPasswordValidationModel> validator)
    {
        validator.RuleFor(x => x.Value)
                 .NotEmpty()
                 .WithMessage("Password cannot be empty.");
    }
}
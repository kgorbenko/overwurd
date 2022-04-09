namespace Overwurd.Domain.Entities.Validation;

internal static class ValidationHelper
{
    public static void EnsureValid<TValidationModel>(TValidationModel validationModel, Action<InlineValidator<TValidationModel>> setRules)
    {
        try
        {
            _ = Validate(validationModel, setRules, ensureValid: true);
        } catch (FluentValidation.ValidationException validationException)
        {
            throw new ValidationException("Domain entity validation error. See inner exception for details", validationException);
        }
    }

    public static ValidationResult Validate<TValidationModel>(TValidationModel validationModel, Action<InlineValidator<TValidationModel>> setRules) =>
        Validate(validationModel, setRules, ensureValid: false);

    private static ValidationResult Validate<TValidationModel>(TValidationModel validationModel, Action<InlineValidator<TValidationModel>> setRules, bool ensureValid)
    {
        var validator = new InlineValidator<TValidationModel>();
        setRules(validator);

        var validationResult = validator.Validate(
            instance: validationModel,
            options: options =>
            {
                if (ensureValid)
                {
                    options.ThrowOnFailures();
                }
            });

        return MapValidationResult(validationResult);
    }

    private static ValidationResult MapValidationResult(FluentValidation.Results.ValidationResult result) =>
        new(
            IsValid: result.IsValid,
            Errors: result.Errors.Select(x => x.ErrorMessage).ToImmutableArray()
        );
}

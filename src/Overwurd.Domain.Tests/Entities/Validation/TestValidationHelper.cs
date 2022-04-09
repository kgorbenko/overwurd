using System.Collections.Immutable;
using FluentValidation;
using NUnit.Framework;
using Overwurd.Domain.Entities.Validation;
using Overwurd.Domain.Tests.Comparers;

namespace Overwurd.Domain.Tests.Entities.Validation;

public static class TestValidationHelper
{
    private record TestValidationModel(string? Value);

    [Test]
    public static void EnsureValidModelIsValid()
    {
        var validationModel = new TestValidationModel("test");

        ValidationHelper.EnsureValid(
            validationModel: validationModel,
            validator => validator.RuleFor(model => model.Value).NotNull()
        );
    }

    [Test]
    public static void EnsureValidModelIsInvalid()
    {
        var validationModel = new TestValidationModel(null);

        var exception = Assert.Throws<Domain.Entities.Validation.ValidationException>(
            () => ValidationHelper.EnsureValid(
                validationModel: validationModel,
                validator => validator.RuleFor(model => model.Value).NotNull()
            )
        );

        Assert.That(exception!.InnerException is FluentValidation.ValidationException);
    }

    [Test]
    public static void ValidateModelIsValid()
    {
        var validationModel = new TestValidationModel("test");

        var actual = ValidationHelper.Validate(
            validationModel: validationModel,
            validator => validator.RuleFor(model => model.Value).NotNull()
        );

        var expected = new ValidationResult(
            IsValid: true,
            Errors: ImmutableArray<string>.Empty
        );

        Assert.That(actual, Is.EqualTo(expected).Using(ValidationResultComparer.Instance));
    }

    [Test]
    public static void ValidationModelIsInvalid()
    {
        var validationModel = new TestValidationModel(null);

        const string validationMessage = "Value cannot be null.";
        var actual = ValidationHelper.Validate(
            validationModel: validationModel,
            validator => validator.RuleFor(model => model.Value)
                                  .NotNull()
                                  .WithMessage(validationMessage)
        );

        var expected = new ValidationResult(
            IsValid: false,
            Errors: new[] { validationMessage }.ToImmutableArray()
        );

        Assert.That(actual, Is.EqualTo(expected).Using(ValidationResultComparer.Instance));
    }
}
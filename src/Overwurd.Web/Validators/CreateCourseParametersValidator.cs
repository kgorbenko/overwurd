using FluentValidation;

namespace Overwurd.Web.Validators;

public class CreateCourseParametersValidator : AbstractValidator<CreateCourseParameters>
{
    public CreateCourseParametersValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotNull().MaximumLength(255);
    }
}
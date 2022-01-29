using FluentValidation;
using JetBrains.Annotations;

namespace Overwurd.Web.Validators;

[UsedImplicitly]
public class CourseParametersValidator : AbstractValidator<CourseParameters>
{
    public CourseParametersValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotNull().MaximumLength(255);
    }
}
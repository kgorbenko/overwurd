using FluentValidation;
using JetBrains.Annotations;

namespace Overwurd.Web.Validators;

[UsedImplicitly]
public class LoginRequestParametersValidator : AbstractValidator<LoginRequestParameters>
{
    public LoginRequestParametersValidator()
    {
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
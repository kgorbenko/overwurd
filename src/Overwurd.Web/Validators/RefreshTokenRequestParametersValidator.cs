using FluentValidation;
using Overwurd.Web.Controllers;

namespace Overwurd.Web.Validators;

public class RefreshTokenRequestParametersValidator : AbstractValidator<RefreshTokenRequestParameters>
{
    public RefreshTokenRequestParametersValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
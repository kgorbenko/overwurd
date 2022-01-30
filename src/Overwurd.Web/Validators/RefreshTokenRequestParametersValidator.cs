using FluentValidation;
using JetBrains.Annotations;

namespace Overwurd.Web.Validators;

[UsedImplicitly]
public class RefreshTokenRequestParametersValidator : AbstractValidator<RefreshTokenRequestParameters>
{
    public RefreshTokenRequestParametersValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
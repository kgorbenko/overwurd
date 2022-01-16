using FluentValidation;
using Overwurd.Web.Controllers;

namespace Overwurd.Web.Validators
{
    public class LoginRequestParametersValidator : AbstractValidator<LoginRequestParameters>
    {
        public LoginRequestParametersValidator()
        {
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
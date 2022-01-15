using FluentValidation;
using Overwurd.Web.Controllers;

namespace Overwurd.Web.Validators
{
    public class RegisterRequestParametersValidator : AbstractValidator<RegisterRequestParameters>
    {
        public RegisterRequestParametersValidator()
        {
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
        }
    }
}
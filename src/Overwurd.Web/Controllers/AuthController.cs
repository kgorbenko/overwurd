using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Overwurd.Model.Helpers;
using Overwurd.Model.Models;
using Overwurd.Web.Services.Auth;

namespace Overwurd.Web.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IJwtAuthService jwtAuthService;
        private readonly ClaimsIdentityOptions claimsIdentityOptions;
        private readonly ILogger<AuthController> logger;

        public AuthController([NotNull] UserManager<User> userManager,
                              [NotNull] IJwtAuthService jwtAuthService,
                              [NotNull] ClaimsIdentityOptions claimsIdentityOptions,
                              [NotNull] ILogger<AuthController> logger)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.jwtAuthService = jwtAuthService ?? throw new ArgumentNullException(nameof(jwtAuthService));
            this.claimsIdentityOptions = claimsIdentityOptions ?? throw new ArgumentNullException(nameof(claimsIdentityOptions));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [UsedImplicitly]
        public record RegistrationRequestParameters(string Login, string Password);

        [UsedImplicitly]
        public record RegistrationResult(string AccessToken, string RefreshToken);

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestParameters parameters, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow.TrimSeconds();
            var existingUser = await userManager.FindByNameAsync(parameters.Login);

            if (existingUser is not null)
            {
                logger.LogInformation("Unsuccessful attempt to register a new user. Following user login exists already: {Login}", parameters.Login);
                return BadRequest();
            }

            var newUser = new User { Login = parameters.Login };
            var identityResult = await userManager.CreateAsync(newUser, parameters.Password);

            if (!identityResult.Succeeded)
            {
                var errorsClause = string.Join(", ", identityResult.Errors.Select(x => $"'{x.Description}'"));
                logger.LogInformation("Unsuccessful attempt to register a new user. Login = {Login}. Errors: {ErrorsClause}", parameters.Login, errorsClause);
                return BadRequest();
            }

            var tokens = await jwtAuthService.GenerateTokensAsync(newUser.Id,
                                                                  AuthHelper.GetUserClaims(newUser, claimsIdentityOptions),
                                                                  now,
                                                                  cancellationToken);

            logger.LogInformation("User #{UserId} has been registered", newUser.Id);

            return Ok(new RegistrationResult(
                AccessToken: tokens.AccessToken,
                RefreshToken: tokens.RefreshToken
            ));
        }

        [UsedImplicitly]
        public record RefreshTokenRequestParameters(string AccessToken, string RefreshToken);

        [UsedImplicitly]
        public record RefreshTokenResult(string AccessToken);

        [AllowAnonymous]
        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestParameters parameters, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow.TrimSeconds();

                var tokens = await jwtAuthService.RefreshAccessTokenAsync(parameters.AccessToken,
                                                                          parameters.RefreshToken,
                                                                          now,
                                                                          cancellationToken);

                var userId = AuthHelper.GetUserIdFromAccessToken(tokens.AccessToken, claimsIdentityOptions.UserIdClaimType);
                logger.LogInformation("User #{UserId} has refreshed access token", userId);

                return Ok(new RefreshTokenResult(AccessToken: tokens.AccessToken));
            } catch (Exception exception)
            {
                var hasUserId = AuthHelper.TryGetUserIdFromAccessToken(parameters.AccessToken, claimsIdentityOptions.UserIdClaimType, out var userId);
                var userClause = hasUserId
                    ? $"user {userId}"
                    : "unknown user";
                logger.LogInformation(exception, "Unsuccessful attempt to refresh access token from {UserClause}", userClause);

                return BadRequest();
            }
        }
    }
}
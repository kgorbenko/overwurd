using System;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        public record JwtAuthViewModel(string AccessToken, string RefreshToken);

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestParameters parameters, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow.TrimSeconds();
            var existingUser = await userManager.FindByNameAsync(parameters.Login);

            if (existingUser is not null)
            {
                logger.LogInformation("Unsuccessful attempt to register a new user. Following user login exists already: {0}", parameters.Login);
                return BadRequest();
            }

            var newUser = new User { Login = parameters.Login };
            var identityResult = await userManager.CreateAsync(newUser, parameters.Password);

            if (!identityResult.Succeeded)
            {
                var errorsClause = string.Join(", ", identityResult.Errors.Select(x => $"'{x.Description}'"));
                logger.LogInformation("Unsuccessful attempt to register a new user. Login = {0}. Errors: {1}", parameters.Login, errorsClause);
                return BadRequest();
            }

            var (isSuccess, tokens, errorMessage) = await jwtAuthService.GenerateTokensAsync(newUser.Id,
                                                                                             GetUserClaims(newUser),
                                                                                             now,
                                                                                             cancellationToken);

            if (!isSuccess)
            {
                logger.LogInformation("Unsuccessful attempt to register a new user. Login = {0}. Error: {1}", parameters.Login, errorMessage);
                logger.LogInformation("Trying to remove user #{0}", newUser.Id);
                await userManager.DeleteAsync(newUser);
                return BadRequest(errorMessage);
            }

            logger.LogInformation("User '{0}'", newUser);
            return Ok(new JwtAuthViewModel(
                AccessToken: tokens.AccessToken,
                RefreshToken: tokens.RefreshToken)
            );
        }

        [UsedImplicitly]
        public record RefreshTokenRequestParameters(string AccessToken, string RefreshToken);

        [AllowAnonymous]
        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestParameters parameters, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow.TrimSeconds();

            var (isSuccess, tokens, errorMessage) = await jwtAuthService.RefreshAccessTokenAsync(parameters.AccessToken,
                                                                                                 parameters.RefreshToken,
                                                                                                 now,
                                                                                                 cancellationToken);

            if (!isSuccess)
            {
                var hasUserId = TryGetUserIdFromAccessToken(parameters.AccessToken, out var userId);
                if (hasUserId)
                {
                    logger.LogInformation("Unsuccessful attempt to refresh access token from user #{0}. Error: {1}", userId, errorMessage);
                } else
                {
                    logger.LogInformation("Unsuccessful attempt to refresh access token from unknown user. Error: {0}", errorMessage);
                }
                return BadRequest(errorMessage);
            }

            return Ok(new JwtAuthViewModel(
                AccessToken: tokens.AccessToken,
                RefreshToken: tokens.RefreshToken));
        }

        private ImmutableArray<Claim> GetUserClaims(User user)
        {
            var identityClaims = new Claim[]
            {
                new(claimsIdentityOptions.UserIdClaimType, user.Id.ToString()),
                new(claimsIdentityOptions.UserNameClaimType, user.Login)
            };

            var roleClaims = user.Roles
                                 .Select(x => new Claim(claimsIdentityOptions.RoleClaimType, x.Name));

            return identityClaims.Concat(roleClaims).ToImmutableArray();
        }

        private bool TryGetUserIdFromAccessToken(string tokenString, out long id)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(tokenString);
                var userIdString = token.Claims.Single(x => x.Type == claimsIdentityOptions.UserIdClaimType).Value;

                id = long.Parse(userIdString);
                return true;
            } catch
            {
                id = 0;
                return false;
            }
        }
    }
}
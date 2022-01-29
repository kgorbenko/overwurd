using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Overwurd.Web.Controllers;

[UsedImplicitly]
public record RegisterRequestParameters(string UserName, string Password, string FirstName, string LastName);

[UsedImplicitly]
public record RefreshTokenRequestParameters(string AccessToken, string RefreshToken);

[UsedImplicitly]
public record LoginRequestParameters(string UserName, string Password);

[UsedImplicitly]
public record LoginResult(long Id, string UserName, string FirstName, string LastName, string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt);

[UsedImplicitly]
public record RefreshTokenResult(string AccessToken, DateTimeOffset ExpiresAt);

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

    private const string InvalidUserNameOrPasswordMessage = "You have entered an invalid username or password. Please double-check and try again.";

    [AllowAnonymous]
    [HttpPost]
    [Route("signup")]
    public async Task<IActionResult> SignUp([FromBody] RegisterRequestParameters parameters, CancellationToken cancellationToken)
    {
        var now = DateTimeOffsetHelper.NowUtcSeconds();
        var existingUser = await userManager.FindByNameAsync(parameters.UserName);

        if (existingUser is not null)
        {
            logger.LogInformation("Unsuccessful attempt to register a new user. Following UserName exists already: {UserName}", parameters.UserName);
            ModelState.AddModelError(nameof(parameters.UserName), "Provided User name is occupied. Please enter a new one.");
            return BadRequest(ModelState);
        }

        var newUser = new User
        {
            UserName = parameters.UserName,
            FirstName = parameters.FirstName,
            LastName = parameters.LastName
        };
        var identityResult = await userManager.CreateAsync(newUser, parameters.Password);

        if (!identityResult.Succeeded)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError("errors", error.Description);
            }

            return BadRequest(ModelState);
        }

        var tokenPairData = await jwtAuthService.GenerateTokensAsync(newUser.Id,
                                                                     AuthHelper.GetUserClaims(newUser, claimsIdentityOptions),
                                                                     now,
                                                                     cancellationToken);

        logger.LogInformation("User #{UserId} has been registered", newUser.Id);

        return Ok(new LoginResult(
                      Id: newUser.Id,
                      UserName: newUser.UserName,
                      FirstName: newUser.FirstName,
                      LastName: newUser.LastName,
                      AccessToken: tokenPairData.AccessToken,
                      RefreshToken: tokenPairData.RefreshToken,
                      AccessTokenExpiresAt: tokenPairData.AccessTokenExpiresAt
                  ));
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestParameters parameters, CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTimeOffsetHelper.NowUtcSeconds();

            var tokenPairData = await jwtAuthService.RefreshAccessTokenAsync(parameters.AccessToken,
                                                                             parameters.RefreshToken,
                                                                             now,
                                                                             cancellationToken);

            var userId = AuthHelper.GetUserIdFromAccessToken(tokenPairData.AccessToken, claimsIdentityOptions.UserIdClaimType);
            logger.LogInformation("User #{UserId} has refreshed access token", userId);

            return Ok(new RefreshTokenResult(
                          AccessToken: tokenPairData.AccessToken,
                          ExpiresAt: tokenPairData.AccessTokenExpiresAt)
            );
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

    [AllowAnonymous]
    [HttpPost]
    [Route("signin")]
    public async Task<IActionResult> SignIn([FromBody] LoginRequestParameters parameters, CancellationToken cancellationToken)
    {
        var now = DateTimeOffsetHelper.NowUtcSeconds();

        var user = await userManager.FindByNameAsync(parameters.UserName);

        if (user is null)
        {
            ModelState.AddModelError(nameof(parameters.UserName), InvalidUserNameOrPasswordMessage);
            logger.LogInformation("Unsuccessful attempt to login. User with such UserName '{UserName}' is not found", parameters.UserName);
            return BadRequest(ModelState);
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, parameters.Password);
        if (!isPasswordValid)
        {
            ModelState.AddModelError(nameof(parameters.UserName), InvalidUserNameOrPasswordMessage);
            logger.LogInformation("Unsuccessful attempt to login from User #{UserId}. Invalid password provided", user.Id);
            return BadRequest(ModelState);
        }

        var tokenPairData = await jwtAuthService.GenerateTokensAsync(user.Id,
                                                                     AuthHelper.GetUserClaims(user, claimsIdentityOptions),
                                                                     now,
                                                                     cancellationToken);

        logger.LogInformation("User #{UserId} successfully logged in", user.Id);

        return Ok(new LoginResult(
                      Id: user.Id,
                      UserName: user.UserName,
                      FirstName: user.FirstName,
                      LastName: user.LastName,
                      AccessToken: tokenPairData.AccessToken,
                      RefreshToken: tokenPairData.RefreshToken,
                      AccessTokenExpiresAt: tokenPairData.AccessTokenExpiresAt
                  ));
    }
}
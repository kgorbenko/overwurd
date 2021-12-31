namespace Overwurd.Web.Services.Auth
{
    public record JwtAuthResult(
        string AccessToken,
        string RefreshToken
    );
}
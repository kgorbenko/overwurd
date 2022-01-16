namespace Overwurd.Web.Options
{
    public record JwtConfiguration(
        string SecurityAlgorithmSignature,
        string SigningKey,
        string Issuer,
        string Audience,
        int AccessTokenExpirationInMinutes,
        int RefreshTokenExpirationInDays
    );
}
namespace Overwurd.Domain.Services.Authentication;

public interface IJwtConfiguration
{
    string SecurityAlgorithmSignature { get; }

    string SigningKey { get; }

    string Issuer { get; }

    string Audience { get; }

    TimeSpan AccessTokenExpiration { get; }

    TimeSpan RefreshTokenExpiration { get; }
}
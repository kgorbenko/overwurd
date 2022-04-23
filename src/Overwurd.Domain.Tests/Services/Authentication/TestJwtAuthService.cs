using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

// ReSharper disable StringLiteralTypo

namespace Overwurd.Domain.Tests.Services.Authentication;

public class TestJwtAuthService
{
    private static string MakeInvalidRefreshTokenMessage(int userId) =>
        $"Provided by user #{userId} refresh token is not valid. Refresh token should not be expired or revoked, " +
        "and it should be related with provided user and provided access token";

    private record JwtConfiguration(
        string SecurityAlgorithmSignature,
        string SigningKey,
        string Issuer,
        string Audience,
        TimeSpan AccessTokenExpiration,
        TimeSpan RefreshTokenExpiration
    ) : IJwtConfiguration;

    private record ClaimsIdentityOptions(
        string RoleClaimType,
        string UserNameClaimType,
        string UserIdClaimType,
        string EmailClaimType,
        string SecurityStampClaimType
    ) : IClaimsIdentityOptions;

    private static IClaimsIdentityOptions GetClaimsIdentityOptions() =>
        new ClaimsIdentityOptions(
            RoleClaimType: ClaimTypes.Role,
            UserNameClaimType: ClaimTypes.Name,
            UserIdClaimType: ClaimTypes.NameIdentifier,
            EmailClaimType: ClaimTypes.Email,
            SecurityStampClaimType: "AspNet.Identity.SecurityStamp"
        );

    private static IJwtConfiguration GetJwtConfiguration() =>
        new JwtConfiguration(
            SecurityAlgorithmSignature: SecurityAlgorithms.HmacSha256Signature,
            SigningKey: "123123123123123123123123123123123123123123123123123123123123123123123123",
            Issuer: "http://localhost:5000",
            Audience: "http://localhost:5000",
            AccessTokenExpiration: TimeSpan.FromMinutes(5),
            RefreshTokenExpiration: TimeSpan.FromDays(10)
        );

    private static TokenValidationParameters GetValidationParameters(IJwtConfiguration jwtConfiguration) =>
        new()
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfiguration.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(AuthHelper.GetBytesFromSigningKey(jwtConfiguration.SigningKey)),
            ValidateAudience = true,
            ValidAudience = jwtConfiguration.Audience,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidAlgorithms = new[] { jwtConfiguration.SecurityAlgorithmSignature }
        };

    [Test]
    public async Task TestTokensGeneration()
    {
        const int userId = 25;
        const string userName = "TestUser";
        var accessTokenId = Guid.Parse("f49d7a3b-56c1-406d-9404-0328ef63120a");
        var refreshTokenString = Guid.Parse("dccd2715-7d25-4223-b44c-920cd9d240e0");
        var date = new DateTimeOffset(year: 2021, month: 2, day: 1, hour: 0, minute: 0, second: 0, TimeSpan.Zero);

        var claimsIdentityOptions = GetClaimsIdentityOptions();
        var claims = new Claim[]
        {
            new(claimsIdentityOptions.UserIdClaimType, userId.ToString()),
            new(claimsIdentityOptions.UserNameClaimType, userName)
        }.ToImmutableArray();

        var jwtConfiguration = GetJwtConfiguration();
        var refreshToken = new JwtRefreshToken(
            AccessTokenId: accessTokenId.ToString(),
            UserId: userId,
            TokenString: refreshTokenString.ToString(),
            ExpiresAt: date.Add(jwtConfiguration.RefreshTokenExpiration),
            CreatedAt: date,
            IsRevoked: false
        );

        var guidProvider = Substitute.For<IRandomGuidGenerator>();
        var jwtRefreshTokenProvider = Substitute.For<IJwtRefreshTokenProvider>();
        var tokenValidationParameters = GetValidationParameters(jwtConfiguration);
        var jwtAuthService = new JwtAuthService(jwtConfiguration, tokenValidationParameters, claimsIdentityOptions, jwtRefreshTokenProvider, guidProvider);

        guidProvider.Generate().Returns(accessTokenId, refreshTokenString);

        var actual = await jwtAuthService.GenerateTokensAsync(userId, claims, date, CancellationToken.None);

        await jwtRefreshTokenProvider.Received().RemoveUserTokenAsync(userId, CancellationToken.None);
        await jwtRefreshTokenProvider.Received().AddTokenAsync(refreshToken, CancellationToken.None);

        var expected = new JwtAccessRefreshTokensPair(
            AccessToken:
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJmNDlkN2EzYi01NmMxLTQwNmQtOTQwNC0wMzI4ZWY2MzEyMGEiLCJzdWIiOiIyNSIsImh0dHA6Ly9zY2hlbWFzL" +
            "nhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMjUiLCJodHRwOi8vc2NoZW" +
            "1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE2MTIxMzc" +
            "5MDAsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCJ9." +
            "FOGmAT3If5aPwbyiqTW0RdpqS0SXNf3y2rTFx0_mtxo",
            RefreshToken: refreshTokenString.ToString(),
            AccessTokenExpiresAt: new DateTimeOffset(year: 2021, month: 2, day: 1, hour: 0, minute: 5, second: 0, TimeSpan.Zero)
        );

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task TestAccessTokenRefresh()
    {
        var jwtConfiguration = GetJwtConfiguration();
        var tokenValidationParameters = GetValidationParameters(jwtConfiguration);
        var claimsIdentityOptions = GetClaimsIdentityOptions();

        var guidProvider = Substitute.For<IRandomGuidGenerator>();
        var jwtRefreshTokenProvider = Substitute.For<IJwtRefreshTokenProvider>();
        var jwtAuthService = new JwtAuthService(jwtConfiguration, tokenValidationParameters, claimsIdentityOptions, jwtRefreshTokenProvider, guidProvider);

        var newAccessTokenId = Guid.Parse("ec637ff3-512b-4efc-a99f-3c82fec63f0b");
        guidProvider.Generate().Returns(newAccessTokenId);

        const int userId = 25;
        const string refreshTokenString = "dccd2715-7d25-4223-b44c-920cd9d240e0";
        const string accessTokenId = "f49d7a3b-56c1-406d-9404-0328ef63120a";
        var date = new DateTimeOffset(year: 2021, month: 2, day: 1, hour: 1, minute: 0, second: 0, TimeSpan.Zero);
        var actualRefreshToken = new JwtRefreshToken(
            AccessTokenId: accessTokenId,
            UserId: userId,
            TokenString: refreshTokenString,
            ExpiresAt: new DateTimeOffset(year: 2021, month: 2, day: 11, hour: 0, minute: 0, second: 0, TimeSpan.Zero),
            CreatedAt: date,
            IsRevoked: false
        );
        jwtRefreshTokenProvider.GetUserTokenAsync(userId, CancellationToken.None).Returns(actualRefreshToken);

        const string accessTokenString =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJmNDlkN2EzYi01NmMxLTQwNmQtOTQwNC0wMzI4ZWY2MzEyMGEiLCJzdWIiOiIyNSIsImh0dHA6Ly9zY2hlbWFzL" +
            "nhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMjUiLCJodHRwOi8vc2NoZW" +
            "1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE2MTIxMzc" +
            "5MDAsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCJ9." +
            "FOGmAT3If5aPwbyiqTW0RdpqS0SXNf3y2rTFx0_mtxo";
        var actual = await jwtAuthService.RefreshAccessTokenAsync(accessTokenString, refreshTokenString, date, CancellationToken.None);

        var newRefreshToken = new JwtRefreshToken(
            AccessTokenId: newAccessTokenId.ToString(),
            UserId: userId,
            TokenString: refreshTokenString,
            ExpiresAt: new DateTimeOffset(year: 2021, month: 2, day: 11, hour: 0, minute: 0, second: 0, TimeSpan.Zero),
            CreatedAt: date,
            IsRevoked: false
        );
        await jwtRefreshTokenProvider.Received().RemoveUserTokenAsync(userId, CancellationToken.None);
        await jwtRefreshTokenProvider.Received().AddTokenAsync(newRefreshToken, CancellationToken.None);

        var expected = new JwtAccessRefreshTokensPair(
            AccessToken:
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJlYzYzN2ZmMy01MTJiLTRlZmMtYTk5Zi0zYzgyZmVjNjNmMGIiLCJzdWIiOiIyNSIsImh0dHA6Ly9zY2hlbWFzL" +
            "nhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMjUiLCJodHRwOi8vc2NoZW" +
            "1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE2MTIxNDE" +
            "1MDAsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCJ9." +
            "2ZrBjNMLhaQy0AxsPNxehV4-AVkJlVudTTT0BsSoGe4",
            RefreshToken: "dccd2715-7d25-4223-b44c-920cd9d240e0",
            AccessTokenExpiresAt: new DateTimeOffset(year: 2021, month: 2, day: 1, hour: 1, minute: 5, second: 0, TimeSpan.Zero)
        );

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void ValidationFailsIfInvalidSignature()
    {
        var jwtConfiguration = new JwtConfiguration(
            SecurityAlgorithmSignature: SecurityAlgorithms.HmacSha256Signature,
            SigningKey: "AnotherSigningKey.AnotherSigningKey.AnotherSigningKey.AnotherSigningKey",
            Issuer: "http://localhost:5000",
            Audience: "http://localhost:5000",
            AccessTokenExpiration: TimeSpan.FromMinutes(5),
            RefreshTokenExpiration: TimeSpan.FromDays(10)
        );
        var tokenValidationParameters = GetValidationParameters(jwtConfiguration);
        var claimsIdentityOptions = GetClaimsIdentityOptions();

        var jwtRefreshTokenProvider = Substitute.For<IJwtRefreshTokenProvider>();
        var guidProvider = Substitute.For<IRandomGuidGenerator>();
        var jwtAuthService = new JwtAuthService(jwtConfiguration, tokenValidationParameters, claimsIdentityOptions, jwtRefreshTokenProvider, guidProvider);

        var date = new DateTimeOffset(year: 2021, month: 2, day: 1, hour: 0, minute: 0, second: 0, TimeSpan.Zero);
        const string accessTokenString =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJmNDlkN2EzYi01NmMxLTQwNmQtOTQwNC0wMzI4ZWY2MzEyMGEiLCJzdWIiOiIyNSIsImh0dHA6Ly9zY2hlbWFzL" +
            "nhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMjUiLCJodHRwOi8vc2NoZW" +
            "1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE2MTIxMzc" +
            "5MDAsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCJ9." +
            "FOGmAT3If5aPwbyiqTW0RdpqS0SXNf3y2rTFx0_mtxo";
        Assert.ThrowsAsync<SecurityTokenSignatureKeyNotFoundException>(
            async () => await jwtAuthService.RefreshAccessTokenAsync(accessTokenString, "", date, CancellationToken.None)
        );
    }

    [Test]
    public void RefreshTokenValidationFailsIfRefreshTokenIsNotFound()
    {
        var jwtConfiguration = GetJwtConfiguration();
        var tokenValidationParameters = GetValidationParameters(jwtConfiguration);
        var claimsIdentityOptions = GetClaimsIdentityOptions();

        var jwtRefreshTokenProvider = Substitute.For<IJwtRefreshTokenProvider>();
        var guidProvider = Substitute.For<IRandomGuidGenerator>();
        var jwtAuthService = new JwtAuthService(jwtConfiguration, tokenValidationParameters, claimsIdentityOptions, jwtRefreshTokenProvider, guidProvider);

        const int userId = 25;
        jwtRefreshTokenProvider.GetUserTokenAsync(userId, CancellationToken.None).Returns(_ => Task.FromResult<JwtRefreshToken?>(null));

        var now = new DateTimeOffset(year: 2021, month: 2, day: 2, hour: 0, minute: 0, second: 0, TimeSpan.Zero);
        const string accessTokenString =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJmNDlkN2EzYi01NmMxLTQwNmQtOTQwNC0wMzI4ZWY2MzEyMGEiLCJzdWIiOiIyNSIsImh0dHA6Ly9zY2hlbWFzL" +
            "nhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMjUiLCJodHRwOi8vc2NoZW" +
            "1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE2MTIxMzc" +
            "5MDAsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCJ9." +
            "FOGmAT3If5aPwbyiqTW0RdpqS0SXNf3y2rTFx0_mtxo";
        const string refreshTokenString = "dccd2715-7d25-4223-b44c-920cd9d240e0";

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await jwtAuthService.RefreshAccessTokenAsync(accessTokenString, refreshTokenString, now, CancellationToken.None)
        );

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Is.EqualTo(MakeInvalidRefreshTokenMessage(userId)));
    }

    [Test]
    public void RefreshTokenValidationFailsIfRefreshTokenStringIsInvalid()
    {
        var jwtConfiguration = GetJwtConfiguration();
        var tokenValidationParameters = GetValidationParameters(jwtConfiguration);
        var claimsIdentityOptions = GetClaimsIdentityOptions();

        var jwtRefreshTokenProvider = Substitute.For<IJwtRefreshTokenProvider>();
        var guidProvider = Substitute.For<IRandomGuidGenerator>();
        var jwtAuthService = new JwtAuthService(jwtConfiguration, tokenValidationParameters, claimsIdentityOptions, jwtRefreshTokenProvider, guidProvider);

        const int userId = 25;
        var actualRefreshToken = new JwtRefreshToken(
            AccessTokenId: "f49d7a3b-56c1-406d-9404-0328ef63120a",
            UserId: userId,
            TokenString: "ec637ff3-512b-4efc-a99f-3c82fec63f0b",
            ExpiresAt: new DateTimeOffset(year: 2021, month: 2, day: 3, hour: 0, minute: 0, second: 0, TimeSpan.Zero),
            CreatedAt: new DateTimeOffset(year: 2021, month: 2, day: 1, hour: 0, minute: 0, second: 0, TimeSpan.Zero),
            IsRevoked: false
        );
        jwtRefreshTokenProvider.GetUserTokenAsync(userId, CancellationToken.None).Returns(actualRefreshToken);

        var now = new DateTimeOffset(year: 2021, month: 2, day: 2, hour: 0, minute: 0, second: 0, TimeSpan.Zero);
        const string accessTokenString =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJmNDlkN2EzYi01NmMxLTQwNmQtOTQwNC0wMzI4ZWY2MzEyMGEiLCJzdWIiOiIyNSIsImh0dHA6Ly9zY2hlbWFzL" +
            "nhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMjUiLCJodHRwOi8vc2NoZW" +
            "1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE2MTIxMzc" +
            "5MDAsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCJ9." +
            "FOGmAT3If5aPwbyiqTW0RdpqS0SXNf3y2rTFx0_mtxo";
        const string refreshTokenString = "dccd2715-7d25-4223-b44c-920cd9d240e0";

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await jwtAuthService.RefreshAccessTokenAsync(accessTokenString, refreshTokenString, now, CancellationToken.None)
        );

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Is.EqualTo(MakeInvalidRefreshTokenMessage(userId)));
    }

    [Test]
    public void RefreshTokenValidationFailsIfAccessTokenIdIsNotValid()
    {
        var jwtConfiguration = GetJwtConfiguration();
        var tokenValidationParameters = GetValidationParameters(jwtConfiguration);
        var claimsIdentityOptions = GetClaimsIdentityOptions();

        var jwtRefreshTokenProvider = Substitute.For<IJwtRefreshTokenProvider>();
        var guidProvider = Substitute.For<IRandomGuidGenerator>();
        var jwtAuthService = new JwtAuthService(jwtConfiguration, tokenValidationParameters, claimsIdentityOptions, jwtRefreshTokenProvider, guidProvider);

        const int userId = 25;
        const string refreshTokenString = "dccd2715-7d25-4223-b44c-920cd9d240e0";
        var actualRefreshToken = new JwtRefreshToken(
            AccessTokenId: "ec637ff3-512b-4efc-a99f-3c82fec63f0b",
            UserId: userId,
            TokenString: refreshTokenString,
            ExpiresAt: new DateTimeOffset(year: 2021, month: 2, day: 3, hour: 0, minute: 0, second: 0, TimeSpan.Zero),
            CreatedAt: new DateTimeOffset(year: 2021, month: 2, day: 1, hour: 0, minute: 0, second: 0, TimeSpan.Zero),
            IsRevoked: false
        );
        jwtRefreshTokenProvider.GetUserTokenAsync(userId, CancellationToken.None).Returns(actualRefreshToken);

        var now = new DateTimeOffset(year: 2021, month: 2, day: 2, hour: 0, minute: 0, second: 0, TimeSpan.Zero);
        const string accessTokenString =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJmNDlkN2EzYi01NmMxLTQwNmQtOTQwNC0wMzI4ZWY2MzEyMGEiLCJzdWIiOiIyNSIsImh0dHA6Ly9zY2hlbWFzL" +
            "nhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMjUiLCJodHRwOi8vc2NoZW" +
            "1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE2MTIxMzc" +
            "5MDAsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCJ9." +
            "FOGmAT3If5aPwbyiqTW0RdpqS0SXNf3y2rTFx0_mtxo";

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await jwtAuthService.RefreshAccessTokenAsync(accessTokenString, refreshTokenString, now, CancellationToken.None)
        );

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Is.EqualTo(MakeInvalidRefreshTokenMessage(userId)));
    }

    [Test]
    public void RefreshTokenValidationFailsIfExpired()
    {
        var jwtConfiguration = GetJwtConfiguration();
        var tokenValidationParameters = GetValidationParameters(jwtConfiguration);
        var claimsIdentityOptions = GetClaimsIdentityOptions();

        var jwtRefreshTokenProvider = Substitute.For<IJwtRefreshTokenProvider>();
        var guidProvider = Substitute.For<IRandomGuidGenerator>();
        var jwtAuthService = new JwtAuthService(jwtConfiguration, tokenValidationParameters, claimsIdentityOptions, jwtRefreshTokenProvider, guidProvider);

        const int userId = 25;
        const string refreshTokenString = "dccd2715-7d25-4223-b44c-920cd9d240e0";
        var actualRefreshToken = new JwtRefreshToken(
            AccessTokenId: "f49d7a3b-56c1-406d-9404-0328ef63120a",
            UserId: userId,
            TokenString: refreshTokenString,
            ExpiresAt: new DateTimeOffset(year: 2021, month: 2, day: 3, hour: 0, minute: 0, second: 0, TimeSpan.Zero),
            CreatedAt: new DateTimeOffset(year: 2021, month: 2, day: 1, hour: 0, minute: 0, second: 0, TimeSpan.Zero),
            IsRevoked: false
        );
        jwtRefreshTokenProvider.GetUserTokenAsync(userId, CancellationToken.None).Returns(actualRefreshToken);

        var now = new DateTimeOffset(year: 2021, month: 2, day: 4, hour: 0, minute: 0, second: 0, TimeSpan.Zero);
        const string accessTokenString =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJmNDlkN2EzYi01NmMxLTQwNmQtOTQwNC0wMzI4ZWY2MzEyMGEiLCJzdWIiOiIyNSIsImh0dHA6Ly9zY2hlbWFzL" +
            "nhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMjUiLCJodHRwOi8vc2NoZW" +
            "1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE2MTIxMzc" +
            "5MDAsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCJ9." +
            "FOGmAT3If5aPwbyiqTW0RdpqS0SXNf3y2rTFx0_mtxo";

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await jwtAuthService.RefreshAccessTokenAsync(accessTokenString, refreshTokenString, now, CancellationToken.None)
        );

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Is.EqualTo(MakeInvalidRefreshTokenMessage(userId)));
    }

    [Test]
    public void RefreshTokenValidationFailsIfRevoked()
    {
        var jwtConfiguration = GetJwtConfiguration();
        var tokenValidationParameters = GetValidationParameters(jwtConfiguration);
        var claimsIdentityOptions = GetClaimsIdentityOptions();

        var jwtRefreshTokenProvider = Substitute.For<IJwtRefreshTokenProvider>();
        var guidProvider = Substitute.For<IRandomGuidGenerator>();
        var jwtAuthService = new JwtAuthService(jwtConfiguration, tokenValidationParameters, claimsIdentityOptions, jwtRefreshTokenProvider, guidProvider);

        const int userId = 25;
        const string refreshTokenString = "dccd2715-7d25-4223-b44c-920cd9d240e0";
        var actualRefreshToken = new JwtRefreshToken(
            AccessTokenId: "f49d7a3b-56c1-406d-9404-0328ef63120a",
            UserId: userId,
            TokenString: refreshTokenString,
            ExpiresAt: new DateTimeOffset(year: 2021, month: 2, day: 3, hour: 0, minute: 0, second: 0, TimeSpan.Zero),
            CreatedAt: new DateTimeOffset(year: 2021, month: 2, day: 1, hour: 0, minute: 0, second: 0, TimeSpan.Zero),
            IsRevoked: true
        );
        jwtRefreshTokenProvider.GetUserTokenAsync(userId, CancellationToken.None).Returns(actualRefreshToken);

        var now = new DateTimeOffset(year: 2021, month: 2, day: 2, hour: 0, minute: 0, second: 0, TimeSpan.Zero);
        const string accessTokenString =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJmNDlkN2EzYi01NmMxLTQwNmQtOTQwNC0wMzI4ZWY2MzEyMGEiLCJzdWIiOiIyNSIsImh0dHA6Ly9zY2hlbWFzL" +
            "nhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMjUiLCJodHRwOi8vc2NoZW" +
            "1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGVzdFVzZXIiLCJleHAiOjE2MTIxMzc" +
            "5MDAsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCJ9." +
            "FOGmAT3If5aPwbyiqTW0RdpqS0SXNf3y2rTFx0_mtxo";

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await jwtAuthService.RefreshAccessTokenAsync(accessTokenString, refreshTokenString, now, CancellationToken.None)
        );

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.Message, Is.EqualTo(MakeInvalidRefreshTokenMessage(userId)));
    }

    [Test]
    public void AccessTokenSubjectIsRequired()
    {
        var jwtConfiguration = GetJwtConfiguration();
        var tokenValidationParameters = GetValidationParameters(jwtConfiguration);
        var claimsIdentityOptions = GetClaimsIdentityOptions();

        var jwtRefreshTokenProvider = Substitute.For<IJwtRefreshTokenProvider>();
        var guidProvider = Substitute.For<IRandomGuidGenerator>();
        var jwtAuthService = new JwtAuthService(jwtConfiguration, tokenValidationParameters, claimsIdentityOptions, jwtRefreshTokenProvider, guidProvider);

        var now = new DateTimeOffset(year: 2021, month: 2, day: 2, hour: 0, minute: 0, second: 0, TimeSpan.Zero);
        const string accessTokenString =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJmNDlkN2EzYi01NmMxLTQwNmQtOTQwNC0wMzI4ZWY2MzEyMGEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZ" +
            "y93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjI1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC" +
            "5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlRlc3RVc2VyIiwiZXhwIjoxNjEyMTM3OTAwLCJpc3MiOiJ" +
            "odHRwOi8vbG9jYWxob3N0OjUwMDAiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjUwMDAifQ." +
            "zIA3h4QgutpVTgTSxmjXMaJVDhWDaun3-9nWrVssXcY";

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await jwtAuthService.RefreshAccessTokenAsync(accessTokenString, "", now, CancellationToken.None)
        );
    }

    [Test]
    public void AccessTokenSubjectIsNotAnId()
    {
        var jwtConfiguration = GetJwtConfiguration();
        var tokenValidationParameters = GetValidationParameters(jwtConfiguration);
        var claimsIdentityOptions = GetClaimsIdentityOptions();

        var jwtRefreshTokenProvider = Substitute.For<IJwtRefreshTokenProvider>();
        var guidProvider = Substitute.For<IRandomGuidGenerator>();
        var jwtAuthService = new JwtAuthService(jwtConfiguration, tokenValidationParameters, claimsIdentityOptions, jwtRefreshTokenProvider, guidProvider);

        var now = new DateTimeOffset(year: 2021, month: 2, day: 2, hour: 0, minute: 0, second: 0, TimeSpan.Zero);
        const string accessTokenString =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJqdGkiOiJmNDlkN2EzYi01NmMxLTQwNmQtOTQwNC0wMzI4ZWY2MzEyMGEiLCJzdWIiOiJhYmMiLCJodHRwOi8vc2NoZW1hc" +
            "y54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjI1IiwiaHR0cDovL3NjaG" +
            "VtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlRlc3RVc2VyIiwiZXhwIjoxNjEyMTM" +
            "3OTAwLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjUwMDAiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjUwMDAifQ." +
            "-PdYY6EDx9jykjoTabLtMA3MC-nSBEzEzEUvVoHRCJ4";

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await jwtAuthService.RefreshAccessTokenAsync(accessTokenString, "", now, CancellationToken.None)
        );
    }
}
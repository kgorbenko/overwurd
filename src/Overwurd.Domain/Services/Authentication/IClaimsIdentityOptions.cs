namespace Overwurd.Domain.Services.Authentication;

public interface IClaimsIdentityOptions
{
    string RoleClaimType { get; }

    string UserNameClaimType { get; }

    string UserIdClaimType { get; }

    string EmailClaimType { get; }

    string SecurityStampClaimType { get; }
}
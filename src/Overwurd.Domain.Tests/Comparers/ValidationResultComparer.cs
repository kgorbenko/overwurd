namespace Overwurd.Domain.Tests.Comparers;

public static class ValidationResultComparer
{
    public static readonly IEqualityComparer<ValidationResult> Instance =
        EqualityComparerBuilder
            .For<ValidationResult>()
            .EquateBy(x => x.IsValid)
            .ThenEquateBy(x => x.Errors);
}
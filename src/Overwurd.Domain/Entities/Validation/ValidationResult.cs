using System.Collections.Immutable;

namespace Overwurd.Domain.Entities.Validation;

public record ValidationResult(bool IsValid, IImmutableList<string> Errors);
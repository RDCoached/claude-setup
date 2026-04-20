namespace Claude_Setup.Domain.Models;

public sealed record ValidationResult(
    bool IsValid,
    IReadOnlyList<ValidationError> Errors
);

public sealed record ValidationError(
    string FilePath,
    string Message,
    ValidationSeverity Severity = ValidationSeverity.Error
);

public enum ValidationSeverity
{
    Warning,
    Error
}

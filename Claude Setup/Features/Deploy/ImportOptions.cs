namespace Claude_Setup.Features.Deploy;

public sealed record ImportOptions(
    bool BackupLocal = true,
    IReadOnlyList<string>? IncludeCategories = null
);

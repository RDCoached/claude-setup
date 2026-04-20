namespace Claude_Setup.Features.Deploy;

public sealed record DeployOptions(
    bool BackupExisting = true,
    bool DryRun = false,
    IReadOnlyList<string>? IncludeCategories = null
);
